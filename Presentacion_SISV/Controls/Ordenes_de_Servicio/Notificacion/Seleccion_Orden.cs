using Capa_Corte_Transversal.Helpers;
using Dominio_SISV.Services.OrdenesServicio;
using System;
using System.Data;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Ordenes_de_Servicio;
using Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Shared;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Seleccion_Orden : Form, ISeleccionOrdenView
    {
        private readonly object _session;
        private readonly Timer _debounce = new Timer { Interval = 250 };
        private readonly SeleccionOrdenPresenter _presenter;
        private bool _isLoading;

        public int OrdenServicioIDSeleccionado { get; private set; } = 0;

        public Seleccion_Orden() : this(null) { }

        public Seleccion_Orden(object session)
        {
            InitializeComponent();

            _session = session;
            _presenter = new SeleccionOrdenPresenter(this, new OrdenesNotificacionService());

            Shown += async (s, e) => await InicializarAsync();
        }

        private async System.Threading.Tasks.Task InicializarAsync()
        {
            _isLoading = true;

            _debounce.Tick += async (s, e) =>
            {
                _debounce.Stop();
                await _presenter.BuscarAsync();
            };

            txt_Buscador_Items_Ordenes.TextChanged += (s, e) =>
            {
                if (_isLoading) return;
                _debounce.Stop();
                _debounce.Start();
            };

            cmbox_Filtrarpor_Ordenes.SelectedIndexChanged += async (s, e) =>
            {
                if (_isLoading) return;
                await _presenter.BuscarAsync();
            };

            _isLoading = false;
            await _presenter.InitializeAsync();
        }

        // ========= ISeleccionOrdenView =========

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

        public string TextoBusqueda => (txt_Buscador_Items_Ordenes.Text ?? "").Trim();

        public string FiltroSeleccionado
        {
            get
            {
                string v = Convert.ToString(cmbox_Filtrarpor_Ordenes.SelectedValue);
                return string.IsNullOrWhiteSpace(v) ? "ORDEN" : v;
            }
        }

        public void BindFiltros(DataTable dt)
        {
            if (dt == null) dt = new DataTable();

            cmbox_Filtrarpor_Ordenes.DataSource = dt;
            cmbox_Filtrarpor_Ordenes.ValueMember = "Value";
            cmbox_Filtrarpor_Ordenes.DisplayMember = "Text";

            if (cmbox_Filtrarpor_Ordenes.Items.Count > 0)
                cmbox_Filtrarpor_Ordenes.SelectedIndex = 0;
        }

        public void RenderOrdenes(DataTable dt, int? selectedOrdenId)
        {
            if (dt == null) dt = new DataTable();

            flowSeleccionOrdenes.SuspendLayout();
            flowSeleccionOrdenes.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int ordenId = ToInt(r, "OrdenServicioID");
                if (ordenId <= 0) continue;

                var card = new OrdenTaskCard
                {
                    Width = Math.Max(200, flowSeleccionOrdenes.ClientSize.Width - 22)
                };

                card.Bind(
                    ordenServicioId: ordenId,
                    codigo: ToStr(r, "CodigoOrden"),
                    cliente: ToStr(r, "Cliente"),
                    correo: ToStr(r, "Correo"),
                    equipo: ToStr(r, "Equipo"),
                    estado: ToStr(r, "Estado")
                );

                if (card.GetType().GetMethod("SetSelected") != null)
                {
                    try
                    {
                        card.GetType().GetMethod("SetSelected")
                            ?.Invoke(card, new object[] { selectedOrdenId.HasValue && selectedOrdenId.Value == ordenId });
                    }
                    catch { }
                }

                card.CardClicked += (s, e) =>
                {
                    _presenter.SeleccionarOrden(ordenId);
                };

                flowSeleccionOrdenes.Controls.Add(card);
            }

            flowSeleccionOrdenes.ResumeLayout();
        }

        public void SetResultados(int total)
        {
            lbl_OrdenesDisponibles_Ordenes.Text = $"Órdenes disponibles: {total}";
        }

        public void SetOrdenSeleccionada(int ordenServicioId)
        {
            OrdenServicioIDSeleccionado = ordenServicioId;
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
    }
}