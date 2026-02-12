using Capa_Corte_Transversal.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Equipos
{
    public partial class Seleccion_Cliente : Form
    {
        private readonly object _session;
        private readonly int _usuarioId;
        private readonly byte _rolId;

        private int? _clienteSeleccionadoId = null;
        private string _clienteSeleccionadoNombre = null;

        private readonly Timer _debounce = new Timer { Interval = 350 };
        private bool _isLoading = false;

        public int? SelectedClienteID => _clienteSeleccionadoId;
        public string SelectedClienteNombre => _clienteSeleccionadoNombre;

        public Seleccion_Cliente() : this(null) { }

        public Seleccion_Cliente(object session)
        {
            InitializeComponent();

            _session = session;
            _usuarioId = TryGetUsuarioSesionId();
            _rolId = TryGetRolSesionId();

            Load += async (s, e) => await Form_LoadAsync();
        }

        private async Task Form_LoadAsync()
        {
            if (_usuarioId <= 0)
            {
                MessageBox.Show("No se pudo obtener UsuarioID de sesión.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            if (_rolId != 1 && _rolId != 2 && _rolId != 4)
            {
                MessageBox.Show("Acceso denegado.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            _isLoading = true;

            _debounce.Tick += async (s, e) =>
            {
                _debounce.Stop();
                await BuscarClientesAsync();
            };

            txt_Buscador_Items_EquiposCliente.TextChanged += (s, e) =>
            {
                if (_isLoading) return;
                _debounce.Stop();
                _debounce.Start();
            };

            cmbox_Filtrarpor_EquiposClientes.SelectedIndexChanged += async (s, e) =>
            {
                if (_isLoading) return;
                await BuscarClientesAsync();
            };

            // cargar filtros
            var dtFiltros = await TryExecDataTableAsync(
                new[] { "ops.usp_Cliente_Filtros_Listar", "dbo.usp_Cliente_Filtros_Listar" },
                cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
            );

            cmbox_Filtrarpor_EquiposClientes.DisplayMember = "Text";
            cmbox_Filtrarpor_EquiposClientes.ValueMember = "Value";
            cmbox_Filtrarpor_EquiposClientes.DataSource = dtFiltros;

            _isLoading = false;

            await BuscarClientesAsync();
        }

        private async Task BuscarClientesAsync()
        {
            try
            {
                string buscar = (txt_Buscador_Items_EquiposCliente.Text ?? "").Trim();
                string filtro = Convert.ToString(cmbox_Filtrarpor_EquiposClientes.SelectedValue) ?? "todos";

                var dt = await TryExecDataTableAsync(
                    new[] { "ops.usp_Cliente_Activo_Buscar", "dbo.usp_Cliente_Activo_Buscar" },
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                        cmd.Parameters.AddWithValue("@FiltroPor", filtro);
                        cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar);
                        cmd.Parameters.AddWithValue("@Top", 200);
                    }
                );

                RenderFlow(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderFlow(DataTable dt)
        {
            flowSeleccionClientes.SuspendLayout();
            flowSeleccionClientes.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["ClienteID"]);
                string cedula = Convert.ToString(r["Cedula"]);
                string nombre = Convert.ToString(r["NombreCompleto"]);
                string correo = Convert.ToString(r["Correo"]);
                string tel = Convert.ToString(r["Telefono"]);
                bool activo = Convert.ToBoolean(r["Activo"]);

                var pnl = new Pnl_SeleccionClientes();
                pnl.Width = flowSeleccionClientes.ClientSize.Width - 22;
                pnl.Bind(id, cedula, nombre, correo, tel, activo);
                pnl.SetSelected(_clienteSeleccionadoId.HasValue && _clienteSeleccionadoId.Value == id);

                pnl.ClienteSeleccionado += (s, args) =>
                {
                    _clienteSeleccionadoId = args.ClienteID;
                    _clienteSeleccionadoNombre = args.NombreCompleto;

                    // marcar visual
                    foreach (Control c in flowSeleccionClientes.Controls)
                        if (c is Pnl_SeleccionClientes it)
                            it.SetSelected(it.ClienteID == _clienteSeleccionadoId);

                    // aceptar al 1 click (si prefieres, cambia a doble click)
                    DialogResult = DialogResult.OK;
                    Close();
                };

                flowSeleccionClientes.Controls.Add(pnl);
            }

            flowSeleccionClientes.ResumeLayout();
        }

        // ===== Helpers sesión / DB =====
        private int TryGetUsuarioSesionId()
        {
            try
            {
                if (_session == null) return 0;
                return SessionHelper.GetUsuarioID(_session);
            }
            catch { return 0; }
        }

        private byte TryGetRolSesionId()
        {
            try
            {
                if (_session == null) return 0;

                var t = _session.GetType();
                var p = t.GetProperty("RoleId") ?? t.GetProperty("RoleID") ?? t.GetProperty("RolId") ?? t.GetProperty("RolID");
                if (p == null) return 0;

                var v = p.GetValue(_session, null);
                if (v == null) return 0;

                return Convert.ToByte(v);
            }
            catch { return 0; }
        }

        private static async Task<DataTable> TryExecDataTableAsync(string[] sps, Action<SqlCommand> fillParams)
        {
            Exception last = null;
            foreach (var sp in sps)
            {
                try { return await ExecDataTableAsync(sp, fillParams); }
                catch (SqlException ex) { last = ex; }
            }
            throw last ?? new Exception("No se pudo ejecutar el procedimiento almacenado.");
        }

        private static string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString
                  ?? ConfigurationManager.ConnectionStrings["SISV_BD"]?.ConnectionString
                  ?? ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(cs)) return cs;

            if (ConfigurationManager.ConnectionStrings.Count > 0)
                return ConfigurationManager.ConnectionStrings[0].ConnectionString;

            throw new Exception("No se encontró ConnectionString en App.config.");
        }

        private static async Task<DataTable> ExecDataTableAsync(string sp, Action<SqlCommand> fillParams)
        {
            var dt = new DataTable();
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fillParams?.Invoke(cmd);

                await cn.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                    dt.Load(rd);
            }
            return dt;
        }
    }
}
