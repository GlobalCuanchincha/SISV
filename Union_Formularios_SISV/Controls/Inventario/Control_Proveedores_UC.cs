using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Clientes
{
    public partial class Control_Proveedores_UC : Form
    {
        private readonly int _usuarioActorId;

        private int? _selectedProveedorId = null;
        private string _selectedProveedorNombre = null;

        public int? SelectedProveedorID => _selectedProveedorId;
        public string SelectedProveedorNombre => _selectedProveedorNombre;

        private readonly Timer _debounce = new Timer { Interval = 350 };

        public Control_Proveedores_UC(int usuarioActorId)
        {
            InitializeComponent();
            _usuarioActorId = usuarioActorId;

            Load += async (s, e) => await Control_Proveedores_UC_Load();
        }

        private async Task Control_Proveedores_UC_Load()
        {
            _debounce.Tick += async (s, e) =>
            {
                _debounce.Stop();
                await BuscarProveedoresAsync();
            };

            txt_BuscarProveedor_UCProveedor.TextChanged += (s, e) =>
            {
                _debounce.Stop();
                _debounce.Start();
            };

            btn_UseProveedor_UCProveedor.Click += (s, e) =>
            {
                if (_selectedProveedorId == null)
                {
                    MessageBox.Show("Seleccione un proveedor.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            };

            await BuscarProveedoresAsync();
        }

        private async Task BuscarProveedoresAsync()
        {
            try
            {
                string buscar = (txt_BuscarProveedor_UCProveedor.Text ?? "").Trim();

                var dt = await ExecDataTableAsync("inv.usp_Proveedores_Buscar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioActorId);
                    cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar);
                    cmd.Parameters.AddWithValue("@SoloActivos", 1);
                    cmd.Parameters.AddWithValue("@Top", 200);
                });

                RenderFlowProveedores(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Proveedores", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderFlowProveedores(DataTable dt)
        {
            flowControlProveedor.SuspendLayout();
            flowControlProveedor.Controls.Clear();

            int count = 0;

            foreach (DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["ProveedorID"]);
                string nombre = Convert.ToString(r["ProveedorNombre"]);
                string ruc = Convert.ToString(r["ProveedorRUC"]);
                string tel = Convert.ToString(r["ProveedorTelefono"]);

                var pnl = new PnlProveedor();
                pnl.Width = flowControlProveedor.ClientSize.Width - 22;
                pnl.Bind(id, nombre, ruc, tel);
                pnl.SetSelected(_selectedProveedorId.HasValue && _selectedProveedorId.Value == id);

                pnl.ProveedorSeleccionado += async (s, args) =>
                {
                    await SeleccionarProveedorAsync(args.ProveedorID);
                    // marcar selección visual
                    foreach (Control c in flowControlProveedor.Controls)
                        if (c is PnlProveedor x) x.SetSelected(x.ProveedorID == _selectedProveedorId);
                };

                flowControlProveedor.Controls.Add(pnl);
                count++;
            }

            lbl_Proveedordisponibles_UCProveedor.Text = $"{count} disponibles";
            flowControlProveedor.ResumeLayout();
        }

        private async Task SeleccionarProveedorAsync(int proveedorId)
        {
            _selectedProveedorId = proveedorId;

            try
            {
                var dt = await ExecDataTableAsync("inv.usp_Proveedores_GetById", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioActorId);
                    cmd.Parameters.AddWithValue("@ProveedorID", proveedorId);
                });

                if (dt.Rows.Count == 0) return;

                var row = dt.Rows[0];
                _selectedProveedorNombre = Convert.ToString(row["ProveedorNombre"]);

                lbl_ProveedorSeleccionado_UCProveedor.Text = _selectedProveedorNombre ?? "";
                lbl_ProveedorRUC_UCProveedor.Text = Convert.ToString(row["ProveedorRUC"]);
                lbl_ProveedorTelefono_UCProveedor.Text = Convert.ToString(row["ProveedorTelefono"]);
                lbl_ProveedorCorreo_UCProveedor.Text = Convert.ToString(row["ProveedorCorreo"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Proveedores", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
