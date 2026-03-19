using Capa_Corte_Transversal.Helpers;
using System;
using System.Data;
using System.Windows.Forms;
using Dominio_SISV.Services.OrdenesServicio;
using Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Shared;

namespace Presentacion_SISV.Controls.Ordenes_de_Servicio.Equipos
{
    public partial class Seleccion_Cliente : Form, ISeleccionClienteView
    {
        private readonly object _session;
        private readonly Timer _debounce = new Timer { Interval = 350 };
        private readonly SeleccionClientePresenter _presenter;
        private bool _isLoading;

        private int? _clienteSeleccionadoId;
        private string _clienteSeleccionadoNombre;

        public int? SelectedClienteID => _clienteSeleccionadoId;
        public string SelectedClienteNombre => _clienteSeleccionadoNombre;

        public Seleccion_Cliente() : this(null) { }

        public Seleccion_Cliente(object session)
        {
            InitializeComponent();

            _session = session;
            _presenter = new SeleccionClientePresenter(this, new OrdenesRecepcionService());

            Load += async (s, e) => await Form_LoadAsync();
        }

        private async System.Threading.Tasks.Task Form_LoadAsync()
        {
            _isLoading = true;

            _debounce.Tick += async (s, e) =>
            {
                _debounce.Stop();
                await _presenter.BuscarAsync();
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
                await _presenter.BuscarAsync();
            };

            _isLoading = false;
            await _presenter.InitializeAsync();
        }

        // ========= ISeleccionClienteView =========

        public int UsuarioId
        {
            get
            {
                try
                {
                    if (_session == null) return 0;
                    return SessionHelper.GetUsuarioID(_session);
                }
                catch { return 0; }
            }
        }

        public string TextoBusqueda => (txt_Buscador_Items_EquiposCliente.Text ?? "").Trim();

        public string FiltroSeleccionado
        {
            get
            {
                string v = Convert.ToString(cmbox_Filtrarpor_EquiposClientes.SelectedValue);
                return string.IsNullOrWhiteSpace(v) ? "todos" : v;
            }
        }

        public void BindFiltros(DataTable dt)
        {
            if (dt == null) dt = new DataTable();

            cmbox_Filtrarpor_EquiposClientes.DisplayMember = "Text";
            cmbox_Filtrarpor_EquiposClientes.ValueMember = "Value";
            cmbox_Filtrarpor_EquiposClientes.DataSource = dt;
        }

        public void RenderClientes(DataTable dt, int? selectedClienteId)
        {
            if (dt == null) dt = new DataTable();

            flowSeleccionClientes.SuspendLayout();
            flowSeleccionClientes.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int id = ToInt(r, "ClienteID");
                if (id <= 0) continue;

                string cedula = ToStr(r, "Cedula");
                string nombre = ToStr(r, "NombreCompleto");
                string correo = ToStr(r, "Correo");
                string telefono = ToStr(r, "Telefono");
                bool activo = ToBool(r, "Activo");

                var pnl = new Pnl_SeleccionClientes
                {
                    Width = Math.Max(200, flowSeleccionClientes.ClientSize.Width - 22)
                };

                pnl.Bind(id, cedula, nombre, correo, telefono, activo);
                pnl.SetSelected(selectedClienteId.HasValue && selectedClienteId.Value == id);

                pnl.ClienteSeleccionado += (s, args) =>
                {
                    foreach (Control c in flowSeleccionClientes.Controls)
                        if (c is Pnl_SeleccionClientes it)
                            it.SetSelected(it.ClienteID == args.ClienteID);

                    _presenter.SeleccionarCliente(args.ClienteID, args.NombreCompleto);
                };

                flowSeleccionClientes.Controls.Add(pnl);
            }

            flowSeleccionClientes.ResumeLayout();
        }

        public void SetResultados(int total)
        {
            if (lbl_Clientesdisponibles_EquiposCliente != null)
                lbl_Clientesdisponibles_EquiposCliente.Text = $"Clientes disponibles: {total}";
        }

        public void SetClienteSeleccionado(int clienteId, string nombreCompleto)
        {
            _clienteSeleccionadoId = clienteId;
            _clienteSeleccionadoNombre = nombreCompleto ?? "";
        }

        public void CloseWithOk()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public void CloseView()
        {
            BeginInvoke(new Action(() => Close()));
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string msg, Exception ex = null)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(msg) ? "Ocurrió un error al procesar la operación." : msg,
                "SISV",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        // ========= Helpers =========

        private static int ToInt(DataRow r, string col)
        {
            return r.Table.Columns.Contains(col) && r[col] != DBNull.Value
                ? Convert.ToInt32(r[col])
                : 0;
        }

        private static string ToStr(DataRow r, string col)
        {
            return r.Table.Columns.Contains(col) && r[col] != DBNull.Value
                ? Convert.ToString(r[col])
                : "";
        }

        private static bool ToBool(DataRow r, string col)
        {
            return r.Table.Columns.Contains(col) &&
                   r[col] != DBNull.Value &&
                   Convert.ToBoolean(r[col]);
        }
    }
}