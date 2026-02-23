using Capa_Corte_Transversal.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Ordenes_de_Servicio;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Seleccion_Orden : Form
    {
        private object _session;
        private int _usuarioId = 0;

        public int OrdenServicioIDSeleccionado { get; private set; } = 0;

        private readonly Timer _debounce = new Timer { Interval = 250 };

        public Seleccion_Orden()
        {
            InitializeComponent();
            Shown += async (s, e) => await InicializarAsync();
        }

        public Seleccion_Orden(object session)
        {
            InitializeComponent();
            _session = session;
            Shown += async (s, e) => await InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            if (_session == null) _session = GetSessionFromPrincipal();
            _usuarioId = TryGetUsuarioSesionId(_session);
            if (_usuarioId <= 0) _usuarioId = 1;

            // Combo filtro
            var dtFiltro = new DataTable();
            dtFiltro.Columns.Add("Value", typeof(string));
            dtFiltro.Columns.Add("Text", typeof(string));

            dtFiltro.Rows.Add("ORDEN", "Orden");
            dtFiltro.Rows.Add("CLIENTE", "Cliente");
            dtFiltro.Rows.Add("CORREO", "Correo");
            dtFiltro.Rows.Add("EQUIPO", "Equipo");
            dtFiltro.Rows.Add("ESTADO", "Estado");

            cmbox_Filtrarpor_Ordenes.DataSource = dtFiltro;
            cmbox_Filtrarpor_Ordenes.ValueMember = "Value";
            cmbox_Filtrarpor_Ordenes.DisplayMember = "Text";
            cmbox_Filtrarpor_Ordenes.SelectedIndex = 0;

            // Debounce buscador
            _debounce.Tick += async (s, e) =>
            {
                _debounce.Stop();
                await CargarOrdenesAsync();
            };

            txt_Buscador_Items_Ordenes.TextChanged += (s, e) =>
            {
                _debounce.Stop();
                _debounce.Start();
            };

            cmbox_Filtrarpor_Ordenes.SelectedIndexChanged += async (s, e) => await CargarOrdenesAsync();

            await CargarOrdenesAsync();
        }

        private async Task CargarOrdenesAsync()
        {
            string filtro = cmbox_Filtrarpor_Ordenes.SelectedValue?.ToString();
            string busqueda = (txt_Buscador_Items_Ordenes.Text ?? "").Trim();

            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Listar_Notificacion", cmd =>
            {
                cmd.Parameters.AddWithValue("@Filtro", string.IsNullOrWhiteSpace(filtro) ? (object)DBNull.Value : filtro);
                cmd.Parameters.AddWithValue("@Busqueda", string.IsNullOrWhiteSpace(busqueda) ? (object)DBNull.Value : busqueda);
            });

            lbl_OrdenesDisponibles_Ordenes.Text = $"Órdenes disponibles: {dt.Rows.Count}";

            flowSeleccionOrdenes.SuspendLayout();
            flowSeleccionOrdenes.Controls.Clear();
            foreach (DataRow r in dt.Rows)
            {
                int ordenId = Convert.ToInt32(r["OrdenServicioID"]);

                var card = new OrdenTaskCard();

                card.Bind(
                    ordenServicioId: ordenId,
                    codigo: Convert.ToString(r["CodigoOrden"]),
                    cliente: Convert.ToString(r["Cliente"]),
                    correo: Convert.ToString(r["Correo"]),
                    equipo: Convert.ToString(r["Equipo"]),
                    estado: Convert.ToString(r["Estado"])
                );

                card.CardClicked += (s, e) =>
                {
                    OrdenServicioIDSeleccionado = ordenId;
                    DialogResult = DialogResult.OK;
                    Close();
                };

                flowSeleccionOrdenes.Controls.Add(card);
            }

            flowSeleccionOrdenes.ResumeLayout();
        }

        // =========================
        // Sesión helpers
        // =========================
        private object GetSessionFromPrincipal()
        {
            try
            {
                var principal = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
                if (principal == null) return null;

                var field = principal.GetType().GetField("_session",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                return field != null ? field.GetValue(principal) : null;
            }
            catch { return null; }
        }

        private int TryGetUsuarioSesionId(object session)
        {
            try
            {
                if (session == null) return 0;
                return SessionHelper.GetUsuarioID(session);
            }
            catch { return 0; }
        }

        // =========================
        // DB helper
        // =========================
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

        private static async Task<DataTable> ExecDataTableAsync(string sp, Action<SqlCommand> fill)
        {
            var dt = new DataTable();

            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fill?.Invoke(cmd);

                await cn.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    if (rd.FieldCount > 0)
                        dt.Load(rd);
                }
            }

            return dt;
        }
    }
}
