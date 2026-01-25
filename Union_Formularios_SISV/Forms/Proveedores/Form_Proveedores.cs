using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Capa_Corte_Transversal.Helpers;
using Union_Formularios_SISV.Controls.Proveedor;

namespace Union_Formularios_SISV.Forms.Proveedores
{
    public partial class Form_Proveedores : Form
    {
        private readonly LoginSession _session;
        private readonly Timer _debounce = new Timer();
        private int? _proveedorSeleccionadoId;

        public Form_Proveedores() : this(null) { }

        public Form_Proveedores(LoginSession session)
        {
            InitializeComponent();
            _session = session;

            Load += Form_Proveedores_Load;
        }

        private void Form_Proveedores_Load(object sender, EventArgs e)
        {
            // combos
            cmbox_Filtro_Proveedor.Items.Clear();
            cmbox_Filtro_Proveedor.Items.AddRange(new object[]
            {
                "Todos", "RUC", "Proveedor", "Telefono", "Correo"
            });
            cmbox_Filtro_Proveedor.SelectedIndex = 0;

            cmbox_EstadoFiltro_Proveedor.Items.Clear();
            cmbox_EstadoFiltro_Proveedor.Items.AddRange(new object[]
            {
                "Todos", "Activo", "Inactivo"
            });
            cmbox_EstadoFiltro_Proveedor.SelectedIndex = 0;

            // debounce 350ms
            _debounce.Interval = 350;
            _debounce.Tick += (_, __) =>
            {
                _debounce.Stop();
                _ = CargarProveedores();
            };

            // eventos auto-buscar
            txt_Buscador_Proveedor.TextChanged += (_, __) => DispararBusqueda();
            cmbox_Filtro_Proveedor.SelectionChangeCommitted += (_, __) => DispararBusqueda();
            cmbox_EstadoFiltro_Proveedor.SelectionChangeCommitted += (_, __) => DispararBusqueda();

            // carga inicial
            _ = CargarProveedores();
        }

        private void DispararBusqueda()
        {
            _debounce.Stop();
            _debounce.Start();
        }

        private static string GetConn()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(cs)) return cs;

            return ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>()
                .FirstOrDefault()?.ConnectionString;
        }

        private async System.Threading.Tasks.Task CargarProveedores()
        {
            try
            {
                flowProveedor.SuspendLayout();
                flowProveedor.Controls.Clear();

                var usuarioId = SessionHelper.GetUsuarioID(_session);
                if (usuarioId <= 0)
                {
                    lbl_Resultados_Proveedor.Text = "0 resultados";
                    return;
                }

                var filtro = (cmbox_Filtro_Proveedor.SelectedItem?.ToString() ?? "Todos");
                var texto = (txt_Buscador_Proveedor.Text ?? "").Trim();

                var estadoSel = (cmbox_EstadoFiltro_Proveedor.SelectedItem?.ToString() ?? "Todos");
                string estadoParam = estadoSel.Equals("Todos", StringComparison.OrdinalIgnoreCase) ? null : estadoSel;

                DataTable dt = new DataTable();

                using (var cn = new SqlConnection(GetConn()))
                using (var cmd = new SqlCommand("inv.usp_Proveedor_Buscar", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
                    cmd.Parameters.AddWithValue("@Texto", (object)texto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Filtro", (object)filtro ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EstadoTexto", (object)estadoParam ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Top", 200);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        await System.Threading.Tasks.Task.Run(() => da.Fill(dt));
                    }
                }

                int total = 0;
                if (dt.Rows.Count > 0 && dt.Columns.Contains("TotalRows"))
                    total = Convert.ToInt32(dt.Rows[0]["TotalRows"]);

                lbl_Resultados_Proveedor.Text = $"{total} resultados";

                foreach (DataRow r in dt.Rows)
                {
                    int id = Convert.ToInt32(r["ProveedorID"]);
                    string nombre = r["Nombre"]?.ToString();
                    string ruc = r["RUC"]?.ToString();
                    string tel = r["Telefono"]?.ToString();
                    bool? activo = dt.Columns.Contains("Activo") && r["Activo"] != DBNull.Value ? (bool?)Convert.ToBoolean(r["Activo"]) : null;
                    string estadoTxt = r["EstadoTexto"]?.ToString();

                    var card = new ProveedorTaskCard();
                    card.Bind(id, ruc, nombre, tel, activo, estadoTxt);

                    card.ProveedorSeleccionado += (_, provId) =>
                    {
                        _proveedorSeleccionadoId = provId;
                        CargarDetalleProveedor(provId);
                    };

                    flowProveedor.Controls.Add(card);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "SISV - SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                flowProveedor.ResumeLayout();
            }
        }

        private void CargarDetalleProveedor(int proveedorId)
        {
            try
            {
                var usuarioId = SessionHelper.GetUsuarioID(_session);
                if (usuarioId <= 0)
                {
                    MessageBox.Show("No se encontró el ID del usuario en la sesión (LoginSession).", "SISV",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataTable dt = new DataTable();
                using (var cn = new SqlConnection(GetConn()))
                using (var cmd = new SqlCommand("inv.usp_Proveedor_GetById", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
                    cmd.Parameters.AddWithValue("@ProveedorID", proveedorId);

                    using (var da = new SqlDataAdapter(cmd))
                        da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                    return;

                var r = dt.Rows[0];

                lbl_Seleccion_Proveedor.Text = r["Nombre"]?.ToString() ?? "—";
                txt_Nombre_Proveedor.Text = r["Nombre"]?.ToString() ?? "";
                txt_RUC_Proveedor.Text = r["RUC"]?.ToString() ?? "";
                txt_Telefono_Proveedor.Text = r["Telefono"]?.ToString() ?? "";
                txt_Correo_Proveedor.Text = r["Correo"]?.ToString() ?? "";
                txt_Direccion_Proveedor.Text = r["Direccion"]?.ToString() ?? "";

                var estado = r["EstadoTexto"]?.ToString() ?? "Activo";
                cmbox_Estado_Proveedor.SelectedItem = estado;

                txt_UltimaAct_Proveedor.Text = r["UltimaActualizacion"]?.ToString() ?? "—";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
