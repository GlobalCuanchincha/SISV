using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Logica_Presentacion.Reportes
{
    public partial class Form_Reportes : Form
    {
        // Vistas internas (sin enums externos para evitar conflictos)
        private enum ReporteVista
        {
            Cliente,
            Inventario,
            Servicio
        }

        private Panel _panelScroll;

        // Cache: instancias únicas (no se recrean)
        private UC_Report_Cliente _ucCliente;
        private UC_Report_Inventario _ucInventario;
        private UC_Report_Servicio _ucServicio;

        // Tamaños “de diseñador” para evitar que el UC se aplaste
        private Size _sizeCliente;
        private Size _sizeInventario;
        private Size _sizeServicio;

        private ReporteVista _vistaActual;
        private bool _inAdjust = false;

        public Form_Reportes()
        {
            InitializeComponent();

            // Eventos de navegación (botones del FORM)
            btn_Report_Componentes.Click += Btn_Report_Componentes_Click;
            btn_Report_Cliente.Click += Btn_Report_Cliente_Click;
            btn_Report_Servicio.Click += Btn_Report_Servicio_Click;

            // Setup al cargar
            this.Load += Form_Reportes_Load;

            // Ajustes responsivos
            flow_Reportes.SizeChanged += (_, __) => AjustarLayout();
            flow_Reportes.Layout += (_, __) => AjustarLayout();
        }

        private void Form_Reportes_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                ConfigurarFlowComoHost();
                CrearHostScroll();
                InicializarCacheUCs();

                // Vista por defecto
                MostrarVista(ReporteVista.Cliente, resetScroll: true);
            }));
        }

        // =========================
        // CONFIGURACIÓN CONTENEDOR
        // =========================
        private void ConfigurarFlowComoHost()
        {
            var flp = flow_Reportes as FlowLayoutPanel;
            if (flp == null) return;

            flp.SuspendLayout();

            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;

            // 🔥 El scroll lo maneja el panel interno, no el Flow
            flp.AutoScroll = false;

            flp.AutoSize = false;
            flp.AutoSizeMode = AutoSizeMode.GrowOnly;

            flp.Dock = DockStyle.Bottom;
            flp.Padding = new Padding(0);
            flp.Margin = new Padding(0);

            flp.ResumeLayout(true);
        }

        private void CrearHostScroll()
        {
            // Deja SOLO un host con scroll dentro del flow
            flow_Reportes.SuspendLayout();
            try
            {
                flow_Reportes.Controls.Clear();

                _panelScroll = new Panel();
                _panelScroll.Margin = new Padding(0);
                _panelScroll.Padding = new Padding(0);
                _panelScroll.BackColor = Color.Transparent;

                _panelScroll.AutoScroll = true;
                _panelScroll.AutoSize = false;
                _panelScroll.AutoSizeMode = AutoSizeMode.GrowOnly;

                flow_Reportes.Controls.Add(_panelScroll);

                var flp = flow_Reportes as FlowLayoutPanel;
                if (flp != null)
                    flp.SetFlowBreak(_panelScroll, true);

                AjustarLayout();
            }
            finally
            {
                flow_Reportes.ResumeLayout(true);
            }
        }

        private void InicializarCacheUCs()
        {
            // Se crean UNA SOLA VEZ
            _ucCliente = new UC_Report_Cliente();
            _ucInventario = new UC_Report_Inventario();
            _ucServicio = new UC_Report_Servicio();

            PrepararUC(_ucCliente);
            PrepararUC(_ucInventario);
            PrepararUC(_ucServicio);

            // Guardar tamaño del diseñador (para no aplastar)
            _sizeCliente = GetSafeDesignSize(_ucCliente);
            _sizeInventario = GetSafeDesignSize(_ucInventario);
            _sizeServicio = GetSafeDesignSize(_ucServicio);
        }

        private void PrepararUC(UserControl uc)
        {
            uc.Margin = new Padding(0);
            uc.Padding = new Padding(0);

            // 🔥 No Dock para permitir scroll H/V cuando el UC exceda
            uc.Dock = DockStyle.None;
            uc.Location = new Point(0, 0);

            // En general mejor false para no “romper” el layout
            uc.AutoSize = false;
            uc.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }

        private Size GetSafeDesignSize(UserControl uc)
        {
            if (uc.Size.Width < 10 || uc.Size.Height < 10)
                return new Size(1200, 700);

            return uc.Size;
        }

        // =========================
        // MOSTRAR VISTA (CACHE)
        // =========================
        private void MostrarVista(ReporteVista vista, bool resetScroll)
        {
            if (_panelScroll == null || _panelScroll.IsDisposed) return;

            _vistaActual = vista;

            if (_panelScroll.Controls.Count > 0)
                _panelScroll.Controls.Clear();

            UserControl uc;
            Size designSize;

            switch (vista)
            {
                case ReporteVista.Inventario:
                    uc = _ucInventario;
                    designSize = _sizeInventario;
                    break;

                case ReporteVista.Servicio:
                    uc = _ucServicio;
                    designSize = _sizeServicio;
                    break;

                default:
                    uc = _ucCliente;
                    designSize = _sizeCliente;
                    break;
            }

            _panelScroll.Controls.Add(uc);

            AjustarUCyScroll(uc, designSize, resetScroll);

            // (Opcional) marcar botón activo visualmente
            MarcarBotonActivo(vista);
        }

        private void AjustarLayout()
        {
            if (_panelScroll == null || _panelScroll.IsDisposed) return;

            // Cuando cambia el tamaño del form, reajustar el UC actual
            UserControl uc = null;
            Size designSize = Size.Empty;

            switch (_vistaActual)
            {
                case ReporteVista.Inventario:
                    uc = _ucInventario;
                    designSize = _sizeInventario;
                    break;

                case ReporteVista.Servicio:
                    uc = _ucServicio;
                    designSize = _sizeServicio;
                    break;

                default:
                    uc = _ucCliente;
                    designSize = _sizeCliente;
                    break;
            }

            if (uc == null) return;

            AjustarUCyScroll(uc, designSize, resetScroll: false);
        }

        private void AjustarUCyScroll(UserControl uc, Size designSize, bool resetScroll)
        {
            if (_inAdjust) return;
            _inAdjust = true;

            try
            {
                int hostW = Math.Max(200, flow_Reportes.ClientSize.Width - flow_Reportes.Padding.Horizontal);
                int hostH = Math.Max(200, flow_Reportes.ClientSize.Height - flow_Reportes.Padding.Vertical);
                _panelScroll.Size = new Size(hostW, hostH);

                int targetW = Math.Max(designSize.Width, _panelScroll.ClientSize.Width);
                int targetH = Math.Max(designSize.Height, _panelScroll.ClientSize.Height);

                uc.Size = new Size(targetW, targetH);
                uc.Location = new Point(0, 0);

                _panelScroll.AutoScrollMinSize = uc.Size;

                if (resetScroll)
                {
                    _panelScroll.AutoScrollPosition = new Point(0, 0);
                }
            }
            finally
            {
                _inAdjust = false;
            }
        }

        private void MarcarBotonActivo(ReporteVista vista)
        {
            btn_Report_Componentes.Enabled = true;
            btn_Report_Cliente.Enabled = true;
            btn_Report_Servicio.Enabled = true;

            if (vista == ReporteVista.Inventario) btn_Report_Componentes.Enabled = false;
            if (vista == ReporteVista.Cliente) btn_Report_Cliente.Enabled = false;
            if (vista == ReporteVista.Servicio) btn_Report_Servicio.Enabled = false;
        }

        // =========================
        // EVENTOS BOTONES FORM
        // =========================
        private void Btn_Report_Componentes_Click(object sender, EventArgs e)
        {
            MostrarVista(ReporteVista.Inventario, resetScroll: true);
        }

        private void Btn_Report_Cliente_Click(object sender, EventArgs e)
        {
            MostrarVista(ReporteVista.Cliente, resetScroll: true);
        }

        private void Btn_Report_Servicio_Click(object sender, EventArgs e)
        {
            MostrarVista(ReporteVista.Servicio, resetScroll: true);
        }
    }
}