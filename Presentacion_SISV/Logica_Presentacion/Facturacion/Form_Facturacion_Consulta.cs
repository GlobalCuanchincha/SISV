using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs.Facturacion;
using Dominio_SISV.Services.Facturacion;
using Union_Formularios_SISV.Controls.Consulta_Facturas;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;
using Union_Formularios_SISV.Forms.Ventas;
using Union_Formularios_SISV.Logica_Presentacion.Facturacion.Consulta;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Facturacion_Consulta : Form, IFacturaConsultaView
    {
        private readonly FacturaConsultaPresenter _presenter;
        private readonly Timer _debounce = new Timer();

        private const string RDLC_FACTURA =
            "Union_Formularios_SISV.Controls.Consulta_Facturas.Factura_ReportViewer.FacturaReport.rdlc";

        public Form_Facturacion_Consulta()
        {
            InitializeComponent();

            _presenter = new FacturaConsultaPresenter(this, new FacturaConsultaService());

            Load -= Form_Facturacion_Consulta_Load;
            Load += Form_Facturacion_Consulta_Load;

            btn_EmitirFactura_View.Click -= btn_EmitirFactura_View_Click;
            btn_EmitirFactura_View.Click += btn_EmitirFactura_View_Click;

            btn_VerDetalle_CFactura.Click -= btn_VerDetalle_CFactura_Click;
            btn_VerDetalle_CFactura.Click += btn_VerDetalle_CFactura_Click;

            btn_Anular_Factura_CFactura.Click -= btn_Anular_Factura_CFactura_Click;
            btn_Anular_Factura_CFactura.Click += btn_Anular_Factura_CFactura_Click;

            if (btn_Volver_CFactura != null)
            {
                btn_Volver_CFactura.Click -= btn_Volver_CFactura_Click;
                btn_Volver_CFactura.Click += btn_Volver_CFactura_Click;
            }
        }

        private PermissionContext Perms
        {
            get
            {
                return new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
        }
        public int UsuarioId => Session.UsuarioId;
        public string TextoBusqueda => (txt_Buscador_Items_CFactura.Text ?? "").Trim();
        public string EstadoFiltro => (cmbox_EstadoFiltrar_CFactura.SelectedItem?.ToString() ?? "Todos");
        public string MotivoAnulacion => (txt_MotivoAnulacion_CFactura.Text ?? "").Trim();

        public void SetResultados(int total) => lbl_Resultados_CFactura.Text = total.ToString();
        public void SetSeleccion(string codigoFacturaOrDash)
            => lbl_FacturaSeleccionada_CFactura.Text = string.IsNullOrWhiteSpace(codigoFacturaOrDash) ? "--" : codigoFacturaOrDash;

        public void ShowDetallePanel(bool visible)
        {
            pnl_DetalleFact_CFactura.Visible = visible;
            pnl_DetalleFact_CFactura.Enabled = visible;
            if (visible) pnl_DetalleFact_CFactura.BringToFront();
        }

        public void RenderReporte(DataSet ds)
        {
            var rv = Report_DetalleFactura_CFactura;
            if (rv == null) return;

            if (string.IsNullOrWhiteSpace(rv.LocalReport.ReportEmbeddedResource))
                rv.LocalReport.ReportEmbeddedResource = RDLC_FACTURA;

            var dtEmpresa = ds.Tables["dsEmpresa"];
            var dtCab = ds.Tables["dsFacturaCabecera"];
            var dtDet = ds.Tables["dsFacturaDetalle"];

            rv.LocalReport.DataSources.Clear();
            rv.LocalReport.DataSources.Add(new ReportDataSource("dsEmpresa", dtEmpresa));
            rv.LocalReport.DataSources.Add(new ReportDataSource("dsFacturaCabecera", dtCab));
            rv.LocalReport.DataSources.Add(new ReportDataSource("dsFacturaDetalle", dtDet));
            rv.RefreshReport();
        }

        public void RenderCards(List<FacturaConsultaCardVM> items, int? selectedId)
        {
            flowConsultaFactura.SuspendLayout();
            try
            {
                foreach (Control c in flowConsultaFactura.Controls) c.Dispose();
                flowConsultaFactura.Controls.Clear();

                if (items == null || items.Count == 0) return;

                int cardW = Math.Max(10, flowConsultaFactura.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6);

                foreach (var vm in items)
                {
                    var card = new ConsultaFacTaskCard
                    {
                        Width = cardW,
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    card.Bind(vm);
                    card.SetSelected(selectedId.HasValue && vm.FacturaID == selectedId.Value);
                    card.FacturaSeleccionada += (_, e) => _presenter.SelectFactura(e.FacturaID, e.CodigoFactura);

                    flowConsultaFactura.Controls.Add(card);
                }
            }
            finally
            {
                flowConsultaFactura.ResumeLayout(true);
            }
        }

        public void SetAccionesEnabled(bool verDetalleEnabled, bool anularEnabled)
        {
            btn_VerDetalle_CFactura.Enabled = verDetalleEnabled;
            btn_Anular_Factura_CFactura.Enabled = anularEnabled;
        }

        public void SetTextoBtnAnular(string texto)
        {
            if (btn_Anular_Factura_CFactura != null)
                btn_Anular_Factura_CFactura.Text = texto ?? "Anular factura";
        }

        public void IrAEmitirFactura()
        {
            if (!Perms.Has("BILL_FACTURA_EMITIR"))
            {
                ShowWarn("No tiene permisos para emitir facturas.");
                return;
            }

            btn_EmitirFactura_View_Click(this, EventArgs.Empty);
        }

        public void ClearMotivo() => txt_MotivoAnulacion_CFactura.Text = "";

        public bool Confirm(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void ShowInfo(string msg) => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
        public void ShowWarn(string msg) => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        public void ShowError(string msg) => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void Form_Facturacion_Consulta_Load(object sender, EventArgs e)
        {
            flowConsultaFactura.FlowDirection = FlowDirection.TopDown;
            flowConsultaFactura.WrapContents = false;
            flowConsultaFactura.AutoScroll = true;

            cmbox_EstadoFiltrar_CFactura.Items.Clear();
            cmbox_EstadoFiltrar_CFactura.Items.AddRange(new object[] { "Todos", "Emitida", "Anulada" });
            cmbox_EstadoFiltrar_CFactura.SelectedIndex = 0;

            ShowDetallePanel(false);
            SetSeleccion("--");
            SetAccionesEnabled(false, false);
            SetTextoBtnAnular("Anular factura");

            ConfigurarReportViewer();

            _debounce.Interval = 350;
            _debounce.Tick += (_, __) => { _debounce.Stop(); _presenter.LoadList(); };

            txt_Buscador_Items_CFactura.TextChanged += (_, __) => { _debounce.Stop(); _debounce.Start(); };
            cmbox_EstadoFiltrar_CFactura.SelectionChangeCommitted += (_, __) => _presenter.LoadList();

            if (!ApplyPermisosUI()) return;

            _presenter.LoadList();
        }

        private bool ApplyPermisosUI()
        {
            bool canConsultar = Perms.HasAny("BILL_FACTURA_CONSULTAR", "BILL_FACTURA_ANULAR");
            bool canEmitir = Perms.Has("BILL_FACTURA_EMITIR");
            bool canAnular = Perms.Has("BILL_FACTURA_ANULAR");

            if (!canConsultar)
            {
                MessageBox.Show("Acceso denegado. No tiene permisos para consultar facturas.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return false;
            }

            btn_EmitirFactura_View.Visible = canEmitir;
            btn_Anular_Factura_CFactura.Visible = canAnular || canEmitir;

            txt_MotivoAnulacion_CFactura.Enabled = canAnular;
            if (!canAnular) txt_MotivoAnulacion_CFactura.Text = "";

            return true;
        }

        private void ConfigurarReportViewer()
        {
            var rv = Report_DetalleFactura_CFactura;
            rv.Reset();
            rv.ProcessingMode = ProcessingMode.Local;
            rv.LocalReport.ReportEmbeddedResource = RDLC_FACTURA;
            rv.LocalReport.DataSources.Clear();
        }

        private void btn_VerDetalle_CFactura_Click(object sender, EventArgs e) => _presenter.VerDetalle();
        private void btn_Volver_CFactura_Click(object sender, EventArgs e) => _presenter.VolverDetalle();
        private void btn_Anular_Factura_CFactura_Click(object sender, EventArgs e) => _presenter.AccionAnularOBotonEmitir();

        private void btn_EmitirFactura_View_Click(object sender, EventArgs e)
        {
            if (!Perms.Has("BILL_FACTURA_EMITIR"))
            {
                MessageBox.Show("No tiene permisos para emitir facturas.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var main = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
            if (main == null)
            {
                MessageBox.Show("No se encontró el Panel Principal.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ventas = new Form_Facturacion();
            ventas.Ventas_RuntimeInit();
            main.OpenChild(ventas, "Ventas / Facturación", "Emitir factura");
        }
    }
}