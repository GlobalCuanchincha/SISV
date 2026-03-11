using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs.Reportes;
using Dominio_SISV.Services.Reportes;
using System.Data.SqlClient;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Union_Formularios_SISV.Logica_Presentacion.Reportes.Helpers;

namespace Union_Formularios_SISV.Logica_Presentacion.Reportes
{
    public partial class UC_Report_Cliente : UserControl
    {
        private readonly IReporteClientesService _service = new ReporteClientesService();

        private DataTable _tabla;
        private DataView _view;

        // Vista gráfica dentro de pnl_dtgCliente
        private Panel _pnlCharts;
        private Chart _chTopFact;
        private Chart _chActivos;

        public UC_Report_Cliente()
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
            // FechaHasta no mayor a hoy
            dtp_FechaHasta_Cliente.MaxDate = DateTime.Today;
            if (dtp_FechaHasta_Cliente.Value.Date > DateTime.Today)
                dtp_FechaHasta_Cliente.Value = DateTime.Today;

            btn_AplicarFiltros_Cliente.Click += (s, e) => AplicarFiltros();
            btn_LimpiarFiltros_Cliente.Click += (s, e) => LimpiarFiltros();

            btn_Tabla_Cliente.Click += (s, e) => MostrarTabla();
            btn_Grafica_Cliente.Click += (s, e) => MostrarGraficos();

            btn_Export_Excel_Cliente.Click += (s, e) => ExcelExportHelper.Exportar(GetTablaVisible(), "Reporte_Clientes");
            btn_Export_PDF_Cliente.Click += (s, e) => PdfExportHelper.Exportar(GetTablaVisible(), "Reporte_Clientes");

            dgv_Clientes.ReadOnly = true;
            dgv_Clientes.MultiSelect = false;
            dgv_Clientes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_Clientes.RowHeadersVisible = false;
            dgv_Clientes.AutoGenerateColumns = true;

            dgv_Clientes.DataBindingComplete += (s, e) => AjustarGridClientes();

            // Buscar dentro del resultado
            txt_BuscarResultaos_Cliente.TextChanged += (s, e) =>
            {
                AplicarFiltroLocal();
                ActualizarKPIsYCharts();
            };

            // Buscar principal: Enter aplica
            txt_BuscadorFiltro_Cliente.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    AplicarFiltros();
                }
            };

            dgv_Clientes.AutoGenerateColumns = true;
            dgv_Clientes.Dock = DockStyle.Fill;
        }

        private void CargarCombos()
        {
            int usuarioId = Session.UsuarioId;

            // FiltrarPor: SP ops.usp_Cliente_Filtros_Listar -> Text/Value
            var dt = _service.ListarOpcionesFiltrarPor(usuarioId);
            cmbox_FiltrarPor_Cliente.DataSource = dt;
            cmbox_FiltrarPor_Cliente.DisplayMember = "Text";
            cmbox_FiltrarPor_Cliente.ValueMember = "Value";

            // Estado: Todos / Activos / Inactivos -> tokens para SP
            cmbox_EstadoFiltro_Cliente.Items.Clear();
            cmbox_EstadoFiltro_Cliente.Items.AddRange(new object[] { "todos", "activos", "inactivos" });
            cmbox_EstadoFiltro_Cliente.SelectedIndex = 0;

            // Con facturas: todos|si|no
            cmbox_CFacturas_Cliente.Items.Clear();
            cmbox_CFacturas_Cliente.Items.AddRange(new object[] { "todos", "si", "no" });
            cmbox_CFacturas_Cliente.SelectedIndex = 0;

            // Ordenar: tokens para SP
            cmbox_Ordenar_Cliente.Items.Clear();
            cmbox_Ordenar_Cliente.Items.AddRange(new object[] { "fechaCreacion", "totalFacturado", "numFacturas", "ultimaFactura", "cliente" });
            cmbox_Ordenar_Cliente.SelectedIndex = 0;
        }

        private void AplicarFiltros()
        {
            int usuarioId = Session.UsuarioId;

            DateTime desde = dtp_Fechadesde_Cliente.Value.Date;
            DateTime hasta = dtp_FechaHasta_Cliente.Value.Date;
            if (hasta > DateTime.Today) hasta = DateTime.Today;
            if (desde > hasta) desde = hasta;

            string estado = GetComboToken(cmbox_EstadoFiltro_Cliente, "todos");
            string conFac = GetComboToken(cmbox_CFacturas_Cliente, "todos");
            string ordenar = GetComboToken(cmbox_Ordenar_Cliente, "fechaCreacion");

            string filtrarPor = "todos";
            if (cmbox_FiltrarPor_Cliente.SelectedValue != null)
                filtrarPor = cmbox_FiltrarPor_Cliente.SelectedValue.ToString();

            string texto = (txt_BuscadorFiltro_Cliente.Text ?? "").Trim();

            var filtro = new FiltroReporteClientesDto
            {
                UsuarioID = usuarioId,
                FechaDesde = desde,
                FechaHasta = hasta,
                Estado = estado,
                ConFacturas = conFac,
                FiltrarPor = filtrarPor,
                Texto = texto,
                Ordenar = ordenar
            };
            try
            {
                _tabla = _service.BuscarReporte(filtro);
                PrepararTablaParaGrid(_tabla);
                if (_tabla.Columns.Contains("ErrorMessage") && _tabla.Columns.Contains("DebugSql"))
                {
                    MessageBox.Show(
                        Convert.ToString(_tabla.Rows[0]["ErrorMessage"]),
                        "Error SQL",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    dgv_Clientes.DataSource = null;
                    return;
                }
                _view = new DataView(_tabla);
                dgv_Clientes.DataSource = _view;

                AplicarFiltroLocal();
                ActualizarKPIsYCharts();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(
                    "Error SQL al cargar reporte:\n\n" +
                    "Procedimiento: " + ex.Procedure + "\n" +
                    "Línea: " + ex.LineNumber + "\n" +
                    "Número: " + ex.Number + "\n\n" +
                    ex.Message,
                    "Reportes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar reporte:\n" + ex.Message, "Reportes",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarFiltros()
        {
            dtp_FechaHasta_Cliente.Value = DateTime.Today;
            dtp_Fechadesde_Cliente.Value = DateTime.Today.AddDays(-30);

            txt_BuscadorFiltro_Cliente.Text = "";
            txt_BuscarResultaos_Cliente.Text = "";

            if (cmbox_EstadoFiltro_Cliente.Items.Count > 0) cmbox_EstadoFiltro_Cliente.SelectedIndex = 0;
            if (cmbox_CFacturas_Cliente.Items.Count > 0) cmbox_CFacturas_Cliente.SelectedIndex = 0;
            if (cmbox_Ordenar_Cliente.Items.Count > 0) cmbox_Ordenar_Cliente.SelectedIndex = 0;
            if (cmbox_FiltrarPor_Cliente.Items.Count > 0) cmbox_FiltrarPor_Cliente.SelectedIndex = 0;

            AplicarFiltros();
        }

        private void AplicarFiltroLocal()
        {
            if (_view == null) return;

            string q = (txt_BuscarResultaos_Cliente.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                _view.RowFilter = "";
                return;
            }

            q = q.Replace("'", "''");

            // columnas que devuelve el SP
            string f = "";
            AddLikeIfExists(ref f, "Cliente", q);
            AddLikeIfExists(ref f, "Cedula", q);
            AddLikeIfExists(ref f, "Email", q);
            AddLikeIfExists(ref f, "Telefono", q);
            _view.RowFilter = f;
        }

        private void AddLikeIfExists(ref string filter, string col, string q)
        {
            if (_view == null || _view.Table == null) return;
            if (!_view.Table.Columns.Contains(col)) return;

            if (!string.IsNullOrEmpty(filter)) filter += " OR ";
            filter += string.Format("{0} LIKE '%{1}%'", col, q);
        }

        private void ActualizarKPIsYCharts()
        {
            var dt = GetTablaVisible();

            int total = dt.Rows.Count;
            int activos = 0;
            int conFact = 0;
            decimal totalFact = 0m;

            foreach (DataRow r in dt.Rows)
            {
                if (dt.Columns.Contains("Activo") && SafeBool(r["Activo"])) activos++;
                if (dt.Columns.Contains("NumFacturas") && SafeInt(r["NumFacturas"]) > 0) conFact++;
                if (dt.Columns.Contains("TotalFacturado")) totalFact += SafeDec(r["TotalFacturado"]);
            }

            lbl_ContClientes.Text = total.ToString();
            lbl_ClientesActivos.Text = activos.ToString();
            lbl_ClientesCFactura.Text = conFact.ToString();
            lbl_TotalFacturado.Text = "$" + totalFact.ToString("N2");

            RenderCharts(dt);
        }

        private DataTable GetTablaVisible()
        {
            return (_view == null) ? new DataTable() : _view.ToTable();
        }

        private void MostrarTabla()
        {
            dgv_Clientes.Visible = true;
            if (_pnlCharts != null) _pnlCharts.Visible = false;
        }

        private void MostrarGraficos()
        {
            dgv_Clientes.Visible = false;
            if (_pnlCharts != null) _pnlCharts.Visible = true;
            RenderCharts(GetTablaVisible());
        }

        private void ConstruirGraficos()
        {
            _pnlCharts = new Panel { Dock = DockStyle.Fill, Visible = false };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            _chTopFact = NewChart("Top clientes por total facturado");
            _chActivos = NewChart("Activos vs Inactivos");

            layout.Controls.Add(_chTopFact, 0, 0);
            layout.Controls.Add(_chActivos, 1, 0);

            _pnlCharts.Controls.Add(layout);

            pnl_dtgCliente.Controls.Add(_pnlCharts);
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

            // Bar: Top 8 por TotalFacturado
            _chTopFact.Series.Clear();
            if (dt.Columns.Contains("Cliente") && dt.Columns.Contains("TotalFacturado"))
            {
                var s = new Series("USD") { ChartType = SeriesChartType.Column };

                var top = dt.AsEnumerable()
                            .OrderByDescending(r => SafeDec(r["TotalFacturado"]))
                            .Take(8);

                foreach (var r in top)
                    s.Points.AddXY(Convert.ToString(r["Cliente"]), (double)SafeDec(r["TotalFacturado"]));

                _chTopFact.Series.Add(s);
                _chTopFact.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            }

            // Pie: Activos vs Inactivos
            _chActivos.Series.Clear();
            if (dt.Columns.Contains("Activo"))
            {
                int act = dt.AsEnumerable().Count(r => SafeBool(r["Activo"]));
                int ina = dt.Rows.Count - act;

                var p = new Series("Clientes") { ChartType = SeriesChartType.Pie };
                p.Points.AddXY("Activos", act);
                p.Points.AddXY("Inactivos", ina);

                _chActivos.Series.Add(p);
            }
        }

        private string GetComboToken(ComboBox cb, string def)
        {
            if (cb == null) return def;
            if (cb.SelectedItem == null) return def;
            return cb.SelectedItem.ToString();
        }

        private bool SafeBool(object v)
        {
            if (v == null || v == DBNull.Value) return false;
            bool b;
            if (bool.TryParse(v.ToString(), out b)) return b;
            int i;
            if (int.TryParse(v.ToString(), out i)) return i != 0;
            return false;
        }

        private int SafeInt(object v)
        {
            if (v == null || v == DBNull.Value) return 0;
            int i;
            return int.TryParse(v.ToString(), out i) ? i : 0;
        }

        private decimal SafeDec(object v)
        {
            if (v == null || v == DBNull.Value) return 0m;
            decimal d;
            return decimal.TryParse(v.ToString(), out d) ? d : 0m;
        }

        private void PrepararTablaParaGrid(DataTable dt)
        {
            if (dt == null) return;

            // Crear columna texto para mostrar Activo/Inactivo (sin checkbox)
            if (dt.Columns.Contains("Activo") && !dt.Columns.Contains("ActivoTexto"))
            {
                var col = new DataColumn("ActivoTexto", typeof(string));
                col.Expression = "IIF(Activo = True, 'Activo', 'Inactivo')";
                dt.Columns.Add(col);
            }
        }

        private void AjustarGridClientes()
        {
            if (dgv_Clientes.Columns == null) return;

            // Ocultar columna checkbox booleana "Activo" si existe
            if (dgv_Clientes.Columns.Contains("Activo"))
                dgv_Clientes.Columns["Activo"].Visible = false;

            // Mostrar la columna texto y renombrar header a "Activo"
            if (dgv_Clientes.Columns.Contains("ActivoTexto"))
            {
                var c = dgv_Clientes.Columns["ActivoTexto"];
                c.HeaderText = "Activo";
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            // Formatos útiles (opcional)
            if (dgv_Clientes.Columns.Contains("TotalFacturado"))
            {
                dgv_Clientes.Columns["TotalFacturado"].DefaultCellStyle.Format = "N2";
                dgv_Clientes.Columns["TotalFacturado"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgv_Clientes.Columns.Contains("FechaCreacion"))
                dgv_Clientes.Columns["FechaCreacion"].DefaultCellStyle.Format = "dd/MM/yyyy";

            if (dgv_Clientes.Columns.Contains("UltimaFactura"))
                dgv_Clientes.Columns["UltimaFactura"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }
    }
}