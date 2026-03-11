using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs.Reportes;
using Dominio_SISV.Services.Reportes;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Union_Formularios_SISV.Controls.Reportes;
using Union_Formularios_SISV.Logica_Presentacion.Reportes.Helpers;

namespace Union_Formularios_SISV.Logica_Presentacion.Reportes
{
    public partial class UC_Report_Servicio : UserControl
    {
        private readonly IReporteServicioService _service = new ReporteServicioService();

        private DataTable _tabla;
        private DataView _view;

        private int? _clienteId;
        private int? _tecnicoId;
        private int? _ordenServicioId;

        private Panel _pnlCharts;
        private Chart _chMetodoPago;
        private Chart _chEstados;

        public UC_Report_Servicio()
        {
            InitializeComponent();
            ConfigurarUI();
            ConstruirGraficos();

            this.Load += (s, e) =>
            {
                CargarCombos();
                MostrarTabla();
                LimpiarFiltros();
            };
        }

        private void ConfigurarUI()
        {
            dtp_FechaHasta_Servicio.MaxDate = DateTime.Today;
            if (dtp_FechaHasta_Servicio.Value.Date > DateTime.Today)
                dtp_FechaHasta_Servicio.Value = DateTime.Today;

            txt_Clientes_Servicio.ReadOnly = true;
            txt_Tecnico_Servicio.ReadOnly = true;
            txt_OS_Servicio.ReadOnly = true;

            dgv_Servicio.AllowUserToAddRows = false;
            dgv_Servicio.AllowUserToDeleteRows = false;
            dgv_Servicio.ReadOnly = true;
            dgv_Servicio.MultiSelect = false;
            dgv_Servicio.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_Servicio.RowHeadersVisible = false;
            dgv_Servicio.AutoGenerateColumns = true;
            dgv_Servicio.Dock = DockStyle.Fill;
            dgv_Servicio.DataBindingComplete += (s, e) => AjustarGrid();

            btn_AplicarFiltros_Servicio.Click += (s, e) => AplicarFiltros();
            btn_LimpiarFiltros_Servicio.Click += (s, e) => LimpiarFiltros();

            btn_Tabla_Servicio.Click += (s, e) => MostrarTabla();
            btn_Grafica_Servicio.Click += (s, e) => MostrarGraficos();

            btn_Export_Excel_Servicio.Click += (s, e) => ExcelExportHelper.Exportar(GetTablaVisible(), "Reporte_Servicio");
            btn_Export_PDF_Servicio.Click += (s, e) => PdfExportHelper.Exportar(GetTablaVisible(), "Reporte_Servicio");

            btn_SelectCliente_Servicio.Click += (s, e) => SeleccionarCliente();
            btn_SelectTecnico_Servicio.Click += (s, e) => SeleccionarTecnico();
            btn_OS_Servicio.Click += (s, e) => SeleccionarOrden();

            btn_LimpiarCliente.Click += (s, e) => LimpiarClienteSeleccionado();
            btn_LimpiarTecnico.Click += (s, e) => LimpiarTecnicoSeleccionado();
            btn_LimpiarOrden.Click += (s, e) => LimpiarOrdenSeleccionada();

            txt_BuscarResultados_Servicio.TextChanged += (s, e) =>
            {
                AplicarFiltroLocal();
                ActualizarKPIsYCharts();
            };

            txt_BuscadorFiltro_Servicio.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    AplicarFiltros();
                }
            };
        }

        private void CargarCombos()
        {
            int usuarioId = Session.UsuarioId;

            var dtPago = _service.ListarMetodosPago(usuarioId);
            cmbox_MetodoPago_Servicio.DataSource = dtPago;
            cmbox_MetodoPago_Servicio.DisplayMember = "Nombre";
            cmbox_MetodoPago_Servicio.ValueMember = "TipoPagoID";

            var dtEstado = _service.ListarEstadosFactura(usuarioId);
            cmbox_Estado_Servicio.DataSource = dtEstado;
            cmbox_Estado_Servicio.DisplayMember = "EstadoNombre";
            cmbox_Estado_Servicio.ValueMember = "EstadoID";

            var dtContenido = _service.ListarContenido();
            cmbox_Contenido_Servicio.DataSource = dtContenido;
            cmbox_Contenido_Servicio.DisplayMember = "Text";
            cmbox_Contenido_Servicio.ValueMember = "Value";
        }

        private void LimpiarFiltros()
        {
            dtp_FechaHasta_Servicio.Value = DateTime.Today;
            dtp_Fechadesde_Servicio.Value = DateTime.Today.AddDays(-30);

            txt_BuscadorFiltro_Servicio.Text = "";
            txt_BuscarResultados_Servicio.Text = "";

            _clienteId = null;
            _tecnicoId = null;
            _ordenServicioId = null;

            txt_Clientes_Servicio.Text = "Todos";
            txt_Tecnico_Servicio.Text = "Todos";
            txt_OS_Servicio.Text = "Todos";

            if (cmbox_MetodoPago_Servicio.Items.Count > 0) cmbox_MetodoPago_Servicio.SelectedIndex = 0;
            if (cmbox_Estado_Servicio.Items.Count > 0) cmbox_Estado_Servicio.SelectedIndex = 0;
            if (cmbox_Contenido_Servicio.Items.Count > 0) cmbox_Contenido_Servicio.SelectedIndex = 0;

            nuc_Totalmin_Servicio.Value = 0;
            nuc_Totalmax_Servicio.Value = 0;

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            int usuarioId = Session.UsuarioId;

            DateTime desde = dtp_Fechadesde_Servicio.Value.Date;
            DateTime hasta = dtp_FechaHasta_Servicio.Value.Date;
            if (hasta > DateTime.Today) hasta = DateTime.Today;
            if (desde > hasta) desde = hasta;

            int? metodoPagoId = GetComboIntNullable(cmbox_MetodoPago_Servicio);
            int? estadoId = GetComboIntNullable(cmbox_Estado_Servicio);
            string contenido = GetComboTextValue(cmbox_Contenido_Servicio, "todos");

            decimal? totalMin = nuc_Totalmin_Servicio.Value > 0 ? (decimal?)nuc_Totalmin_Servicio.Value : null;
            decimal? totalMax = nuc_Totalmax_Servicio.Value > 0 ? (decimal?)nuc_Totalmax_Servicio.Value : null;

            var filtro = new FiltroReporteServicioDto
            {
                UsuarioID = usuarioId,
                FechaDesde = desde,
                FechaHasta = hasta,
                Texto = (txt_BuscadorFiltro_Servicio.Text ?? "").Trim(),
                MetodoPagoID = metodoPagoId,
                EstadoID = estadoId,
                Contenido = contenido,
                ClienteID = _clienteId,
                TecnicoID = _tecnicoId,
                OrdenServicioID = _ordenServicioId,
                TotalMin = totalMin,
                TotalMax = totalMax
            };

            try
            {
                _tabla = _service.BuscarReporte(filtro);
                PrepararTabla(_tabla);

                _view = new DataView(_tabla);
                dgv_Servicio.DataSource = _view;

                AplicarFiltroLocal();
                ActualizarKPIsYCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte de servicio:\n\n" + ex.Message,
                    "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrepararTabla(DataTable dt)
        {
            if (dt == null) return;

            if (dt.Columns.Contains("Anulada") && !dt.Columns.Contains("AnuladaTexto"))
            {
                var col = new DataColumn("AnuladaTexto", typeof(string));
                col.Expression = "IIF(Anulada = True, 'Sí', 'No')";
                dt.Columns.Add(col);
            }
        }

        private void AjustarGrid()
        {
            if (dgv_Servicio.Columns == null) return;

            if (dgv_Servicio.Columns.Contains("FacturaID"))
                dgv_Servicio.Columns["FacturaID"].Visible = false;

            if (dgv_Servicio.Columns.Contains("Anulada"))
                dgv_Servicio.Columns["Anulada"].Visible = false;

            if (dgv_Servicio.Columns.Contains("AnuladaTexto"))
                dgv_Servicio.Columns["AnuladaTexto"].HeaderText = "Anulada";

            if (dgv_Servicio.Columns.Contains("Fecha"))
                dgv_Servicio.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgv_Servicio.Columns.Contains("Total"))
            {
                dgv_Servicio.Columns["Total"].DefaultCellStyle.Format = "N2";
                dgv_Servicio.Columns["Total"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void AplicarFiltroLocal()
        {
            if (_view == null) return;

            string q = (txt_BuscarResultados_Servicio.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                _view.RowFilter = "";
                return;
            }

            q = q.Replace("'", "''");

            string f = "";
            AddLike(ref f, "NumeroFactura", q);
            AddLike(ref f, "Cliente", q);
            AddLike(ref f, "Tecnico", q);
            AddLike(ref f, "OrdenServicio", q);
            AddLike(ref f, "MetodoPago", q);
            AddLike(ref f, "Estado", q);
            AddLike(ref f, "Contenido", q);

            _view.RowFilter = f;
        }

        private void AddLike(ref string filter, string col, string q)
        {
            if (_view == null || _view.Table == null) return;
            if (!_view.Table.Columns.Contains(col)) return;

            if (!string.IsNullOrEmpty(filter)) filter += " OR ";
            filter += string.Format("{0} LIKE '%{1}%'", col, q);
        }

        private void ActualizarKPIsYCharts()
        {
            var dt = GetTablaVisible();

            decimal ingresos = 0m;
            int facturasTotal = dt.Rows.Count;
            int ordenes = 0;
            int anuladas = 0;

            if (dt.Columns.Contains("OrdenServicio"))
                ordenes = dt.AsEnumerable()
                    .Select(r => Convert.ToString(r["OrdenServicio"]))
                    .Where(s => !string.IsNullOrWhiteSpace(s) && s != "-")
                    .Distinct()
                    .Count();

            foreach (DataRow r in dt.Rows)
            {
                bool anulada = SafeBool(dt.Columns.Contains("Anulada") ? r["Anulada"] : null);
                if (anulada) anuladas++;
                else ingresos += SafeDec(dt.Columns.Contains("Total") ? r["Total"] : null);
            }

            lbl_Ingresos.Text = "$" + ingresos.ToString("N2");
            lbl_FacturasTotal.Text = facturasTotal.ToString();
            lbl_OrdenesServicio.Text = ordenes.ToString();
            lbl_FacturaAnulada.Text = anuladas.ToString();

            RenderCharts(dt);
        }

        private DataTable GetTablaVisible()
        {
            return _view == null ? new DataTable() : _view.ToTable();
        }

        private void MostrarTabla()
        {
            dgv_Servicio.Visible = true;
            if (_pnlCharts != null) _pnlCharts.Visible = false;
        }

        private void MostrarGraficos()
        {
            dgv_Servicio.Visible = false;
            if (_pnlCharts != null) _pnlCharts.Visible = true;
            RenderCharts(GetTablaVisible());
        }

        private void ConstruirGraficos()
        {
            _pnlCharts = new Panel { Dock = DockStyle.Fill, Visible = false };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));

            _chMetodoPago = NewChart("Ingresos por método de pago");
            _chEstados = NewChart("Facturas por estado");

            layout.Controls.Add(_chMetodoPago, 0, 0);
            layout.Controls.Add(_chEstados, 1, 0);

            _pnlCharts.Controls.Add(layout);
            pnl_dgv_Servicio.Controls.Add(_pnlCharts);
            _pnlCharts.BringToFront();
        }

        private Chart NewChart(string title)
        {
            var ch = new Chart { Dock = DockStyle.Fill };
            ch.ChartAreas.Add(new ChartArea("A") { AxisX = { Interval = 1 } });
            ch.Titles.Add(title);
            ch.Legends.Add(new Legend());
            return ch;
        }

        private void RenderCharts(DataTable dt)
        {
            if (dt == null) return;

            _chMetodoPago.Series.Clear();
            if (dt.Columns.Contains("MetodoPago") && dt.Columns.Contains("Total"))
            {
                var s = new Series("USD") { ChartType = SeriesChartType.Column };

                var grupos = dt.AsEnumerable()
                    .Where(r => !SafeBool(dt.Columns.Contains("Anulada") ? r["Anulada"] : null))
                    .GroupBy(r => Convert.ToString(r["MetodoPago"]))
                    .Select(g => new
                    {
                        Metodo = string.IsNullOrWhiteSpace(g.Key) ? "-" : g.Key,
                        Total = g.Sum(x => SafeDec(x["Total"]))
                    })
                    .OrderByDescending(x => x.Total);

                foreach (var g in grupos)
                    s.Points.AddXY(g.Metodo, (double)g.Total);

                _chMetodoPago.Series.Add(s);
                _chMetodoPago.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            }

            _chEstados.Series.Clear();
            if (dt.Columns.Contains("Estado"))
            {
                var p = new Series("Facturas") { ChartType = SeriesChartType.Pie };

                var grupos = dt.AsEnumerable()
                    .GroupBy(r => Convert.ToString(r["Estado"]))
                    .Select(g => new { Estado = string.IsNullOrWhiteSpace(g.Key) ? "-" : g.Key, Cant = g.Count() });

                foreach (var g in grupos)
                    p.Points.AddXY(g.Estado, g.Cant);

                _chEstados.Series.Add(p);
            }
        }

        private void SeleccionarCliente()
        {
            using (var frm = new Form_SeleccionarClienteTecnico(Form_SeleccionarClienteTecnico.ModoSeleccion.Cliente))
            {
                if (frm.ShowDialog() != DialogResult.OK || frm.Seleccion == null) return;

                _clienteId = frm.Seleccion.ID;
                txt_Clientes_Servicio.Text = frm.Seleccion.TextoPrincipal;
            }
        }

        private void SeleccionarTecnico()
        {
            using (var frm = new Form_SeleccionarClienteTecnico(Form_SeleccionarClienteTecnico.ModoSeleccion.Tecnico))
            {
                if (frm.ShowDialog() != DialogResult.OK || frm.Seleccion == null) return;

                _tecnicoId = frm.Seleccion.ID;
                txt_Tecnico_Servicio.Text = frm.Seleccion.TextoPrincipal;
            }
        }

        private void SeleccionarOrden()
        {
            using (var frm = new Form_SeleccionarClienteTecnico(
                Form_SeleccionarClienteTecnico.ModoSeleccion.OrdenServicio,
                _tecnicoId))
            {
                if (frm.ShowDialog() != DialogResult.OK || frm.Seleccion == null) return;

                _ordenServicioId = frm.Seleccion.ID;
                txt_OS_Servicio.Text = frm.Seleccion.TextoPrincipal;
            }
        }

        private int? GetComboIntNullable(ComboBox cb)
        {
            if (cb == null || cb.SelectedValue == null) return null;

            int id;
            if (!int.TryParse(cb.SelectedValue.ToString(), out id)) return null;
            return id > 0 ? (int?)id : null;
        }

        private string GetComboTextValue(ComboBox cb, string def)
        {
            if (cb == null || cb.SelectedValue == null) return def;
            return Convert.ToString(cb.SelectedValue) ?? def;
        }

        private decimal SafeDec(object v)
        {
            decimal d;
            return (v != null && v != DBNull.Value && decimal.TryParse(v.ToString(), out d)) ? d : 0m;
        }

        private bool SafeBool(object v)
        {
            if (v == null || v == DBNull.Value) return false;

            bool b;
            if (bool.TryParse(v.ToString(), out b)) return b;

            int i;
            return int.TryParse(v.ToString(), out i) && i != 0;
        }

        private void LimpiarClienteSeleccionado()
        {
            _clienteId = null;
            txt_Clientes_Servicio.Text = "Todos";
        }

        private void LimpiarTecnicoSeleccionado()
        {
            _tecnicoId = null;
            txt_Tecnico_Servicio.Text = "Todos";
        }

        private void LimpiarOrdenSeleccionada()
        {
            _ordenServicioId = null;
            txt_OS_Servicio.Text = "Todos";
        }
    }
}