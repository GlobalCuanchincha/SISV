using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs.Reportes;
using Dominio_SISV.Services.Reportes;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Union_Formularios_SISV.Logica_Presentacion.Reportes.Helpers;

namespace Union_Formularios_SISV.Logica_Presentacion.Reportes
{
    public partial class UC_Report_Inventario : UserControl
    {
        private readonly IReporteInventarioService _service = new ReporteInventarioService();

        private DataTable _tabla;
        private DataView _view;

        private Panel _pnlCharts;
        private Chart _chValorTop;
        private Chart _chCriticos;

        public UC_Report_Inventario()
        {
            InitializeComponent();
            ConfigurarUI();
            ConstruirGraficos();

            this.Load += (s, e) =>
            {
                CargarCombos();
                MostrarTabla();
                LimpiarFiltros(); // pone defaults y aplica
            };
        }

        private void ConfigurarUI()
        {
            dtp_FechaHasta_Inventario.MaxDate = DateTime.Today;
            if (dtp_FechaHasta_Inventario.Value.Date > DateTime.Today)
                dtp_FechaHasta_Inventario.Value = DateTime.Today;

            dgv_Inventario.AllowUserToAddRows = false;
            dgv_Inventario.AllowUserToDeleteRows = false;
            dgv_Inventario.ReadOnly = true;
            dgv_Inventario.MultiSelect = false;
            dgv_Inventario.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv_Inventario.RowHeadersVisible = false;
            dgv_Inventario.AutoGenerateColumns = true;

            dgv_Inventario.DataBindingComplete += (s, e) => AjustarGrid();

            btn_AplicarFiltros_Inventario.Click += (s, e) => AplicarFiltros();
            btn_LimpiarFiltros_Inventario.Click += (s, e) => LimpiarFiltros();

            btn_Tabla_Inventario.Click += (s, e) => MostrarTabla();
            btn_Grafica_Inventario.Click += (s, e) => MostrarGraficos();

            btn_Export_Excel_Inventario.Click += (s, e) => ExcelExportHelper.Exportar(GetTablaVisible(), "Reporte_Inventario");
            btn_Export_PDF_Inventario.Click += (s, e) => PdfExportHelper.Exportar(GetTablaVisible(), "Reporte_Inventario");

            txt_BuscarResultados_Inventario.TextChanged += (s, e) =>
            {
                AplicarFiltroLocal();
                ActualizarKPIsYCharts();
            };

            txt_BuscadorFiltro_Inventario.KeyDown += (s, e) =>
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

            // Filtrar fechas por (fijo)
            cmbox_FiltrarFecha_Inventario.Items.Clear();
            cmbox_FiltrarFecha_Inventario.Items.AddRange(new object[] { "todos", "creacion", "actualizacion", "ingreso" });
            cmbox_FiltrarFecha_Inventario.SelectedIndex = 0;

            // Stock filtro (fijo)
            cmbox_Stock_Inventario.Items.Clear();
            cmbox_Stock_Inventario.Items.AddRange(new object[] { "todos", "constock", "sinstock", "critico" });
            cmbox_Stock_Inventario.SelectedIndex = 0;

            // Estado (fijo)
            cmbox_Estado_Inventario.Items.Clear();
            cmbox_Estado_Inventario.Items.AddRange(new object[] { "todos", "activos", "inactivos" });
            cmbox_Estado_Inventario.SelectedIndex = 0;

            // Ordenar (fijo)
            cmbox_Ordenar_Inventario.Items.Clear();
            cmbox_Ordenar_Inventario.Items.AddRange(new object[] { "nombre", "stock", "costo", "precio", "valor", "fecha", "categoria", "proveedor" });
            cmbox_Ordenar_Inventario.SelectedIndex = 0;

            // Categorías (SP)
            var dtCat = _service.ListarCategorias(usuarioId);
            InsertarFilaTodos(dtCat, "CategoriaID", "Categoria");
            cmbox_Categoria_Inventario.DataSource = dtCat;
            cmbox_Categoria_Inventario.DisplayMember = "Categoria";
            cmbox_Categoria_Inventario.ValueMember = "CategoriaID";

            // Proveedores (SP)
            var dtProv = _service.ListarProveedores(usuarioId);
            InsertarFilaTodos(dtProv, "ProveedorID", "Proveedor");
            cmbox_Proveedor_Inventario.DataSource = dtProv;
            cmbox_Proveedor_Inventario.DisplayMember = "Proveedor";
            cmbox_Proveedor_Inventario.ValueMember = "ProveedorID";
        }

        private void InsertarFilaTodos(DataTable dt, string idCol, string textCol)
        {
            if (dt == null) return;
            if (!dt.Columns.Contains(idCol) || !dt.Columns.Contains(textCol)) return;

            var r = dt.NewRow();
            r[idCol] = 0;
            r[textCol] = "Todos";
            dt.Rows.InsertAt(r, 0);
        }

        private void LimpiarFiltros()
        {
            dtp_FechaHasta_Inventario.Value = DateTime.Today;
            dtp_Fechadesde_Inventario.Value = DateTime.Today.AddDays(-30);

            txt_BuscadorFiltro_Inventario.Text = "";
            txt_SKU_Inventario.Text = "";
            txt_NombreProducto_Inventario.Text = "";
            txt_BuscarResultados_Inventario.Text = "";

            if (cmbox_FiltrarFecha_Inventario.Items.Count > 0) cmbox_FiltrarFecha_Inventario.SelectedIndex = 0;
            if (cmbox_Categoria_Inventario.Items.Count > 0) cmbox_Categoria_Inventario.SelectedIndex = 0;
            if (cmbox_Proveedor_Inventario.Items.Count > 0) cmbox_Proveedor_Inventario.SelectedIndex = 0;
            if (cmbox_Stock_Inventario.Items.Count > 0) cmbox_Stock_Inventario.SelectedIndex = 0;
            if (cmbox_Estado_Inventario.Items.Count > 0) cmbox_Estado_Inventario.SelectedIndex = 0;
            if (cmbox_Ordenar_Inventario.Items.Count > 0) cmbox_Ordenar_Inventario.SelectedIndex = 0;

            nuc_Costomin_Inventario.Value = 0;
            nuc_Costomax_Inventario.Value = 0;
            nuc_Preciomin_Inventario.Value = 0;
            nuc_Preciomax_Inventario.Value = 0;

            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            int usuarioId = Session.UsuarioId;

            DateTime desde = dtp_Fechadesde_Inventario.Value.Date;
            DateTime hasta = dtp_FechaHasta_Inventario.Value.Date;
            if (hasta > DateTime.Today) hasta = DateTime.Today;
            if (desde > hasta) desde = hasta;

            string filtrarFecha = GetToken(cmbox_FiltrarFecha_Inventario, "todos");
            string stockFiltro = GetToken(cmbox_Stock_Inventario, "todos");
            string estado = GetToken(cmbox_Estado_Inventario, "todos");
            string ordenar = GetToken(cmbox_Ordenar_Inventario, "nombre");

            int? categoriaId = GetComboIdNullable(cmbox_Categoria_Inventario);
            int? proveedorId = GetComboIdNullable(cmbox_Proveedor_Inventario);

            string texto = (txt_BuscadorFiltro_Inventario.Text ?? "").Trim();
            string sku = (txt_SKU_Inventario.Text ?? "").Trim();
            string nombre = (txt_NombreProducto_Inventario.Text ?? "").Trim();

            decimal? costoMin = nuc_Costomin_Inventario.Value > 0 ? (decimal?)nuc_Costomin_Inventario.Value : null;
            decimal? costoMax = nuc_Costomax_Inventario.Value > 0 ? (decimal?)nuc_Costomax_Inventario.Value : null;
            decimal? precioMin = nuc_Preciomin_Inventario.Value > 0 ? (decimal?)nuc_Preciomin_Inventario.Value : null;
            decimal? precioMax = nuc_Preciomax_Inventario.Value > 0 ? (decimal?)nuc_Preciomax_Inventario.Value : null;

            var filtro = new FiltroReporteInventarioDto
            {
                UsuarioID = usuarioId,
                FechaDesde = desde,
                FechaHasta = hasta,
                FiltrarFecha = filtrarFecha,

                Texto = texto,
                SKU = sku,
                CategoriaID = categoriaId,
                ProveedorID = proveedorId,
                Nombre = nombre,

                StockFiltro = stockFiltro,
                Estado = estado,

                CostoMin = costoMin,
                CostoMax = costoMax,
                PrecioMin = precioMin,
                PrecioMax = precioMax,

                Ordenar = ordenar
            };

            try
            {
                _tabla = _service.BuscarReporte(filtro);
                PrepararTabla(_tabla);

                _view = new DataView(_tabla);
                dgv_Inventario.DataSource = _view;

                AplicarFiltroLocal();
                ActualizarKPIsYCharts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar inventario:\n" + ex.Message, "Reportes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrepararTabla(DataTable dt)
        {
            if (dt == null) return;

            // ActivoTexto para evitar checkbox
            if (dt.Columns.Contains("Activo") && !dt.Columns.Contains("ActivoTexto"))
            {
                var col = new DataColumn("ActivoTexto", typeof(string));
                col.Expression = "IIF(Activo = True, 'Activo', 'Inactivo')";
                dt.Columns.Add(col);
            }
        }

        private void AjustarGrid()
        {
            if (dgv_Inventario.Columns == null) return;

            if (dgv_Inventario.Columns.Contains("Activo"))
                dgv_Inventario.Columns["Activo"].Visible = false;

            if (dgv_Inventario.Columns.Contains("ActivoTexto"))
                dgv_Inventario.Columns["ActivoTexto"].HeaderText = "Activo";

            if (dgv_Inventario.Columns.Contains("Costo"))
                dgv_Inventario.Columns["Costo"].DefaultCellStyle.Format = "N2";

            if (dgv_Inventario.Columns.Contains("PrecioVenta"))
                dgv_Inventario.Columns["PrecioVenta"].DefaultCellStyle.Format = "N2";

            if (dgv_Inventario.Columns.Contains("ValorStock"))
                dgv_Inventario.Columns["ValorStock"].DefaultCellStyle.Format = "N2";

            if (dgv_Inventario.Columns.Contains("FechaRef"))
                dgv_Inventario.Columns["FechaRef"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void AplicarFiltroLocal()
        {
            if (_view == null) return;

            string q = (txt_BuscarResultados_Inventario.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q))
            {
                _view.RowFilter = "";
                return;
            }

            q = q.Replace("'", "''");

            string f = "";
            AddLike(ref f, "SKU", q);
            AddLike(ref f, "Producto", q);
            AddLike(ref f, "Categoria", q);
            AddLike(ref f, "Proveedor", q);

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

            int items = dt.Rows.Count;
            long stockTotal = 0;
            int criticos = 0;
            decimal valorStock = 0m;

            foreach (DataRow r in dt.Rows)
            {
                stockTotal += SafeLong(r, "Stock");
                if (SafeInt(r, "Critico") == 1) criticos++;
                valorStock += SafeDec(r, "ValorStock");
            }

            lbl_Items.Text = items.ToString();
            lbl_StockTotal.Text = stockTotal.ToString();
            lblStockCritico.Text = criticos.ToString();
            lblValorStock.Text = "$" + valorStock.ToString("N2");

            RenderCharts(dt);
        }

        private DataTable GetTablaVisible()
        {
            return (_view == null) ? new DataTable() : _view.ToTable();
        }

        private void MostrarTabla()
        {
            dgv_Inventario.Visible = true;
            if (_pnlCharts != null) _pnlCharts.Visible = false;
        }

        private void MostrarGraficos()
        {
            dgv_Inventario.Visible = false;
            if (_pnlCharts != null) _pnlCharts.Visible = true;
            RenderCharts(GetTablaVisible());
        }

        private void ConstruirGraficos()
        {
            _pnlCharts = new Panel { Dock = DockStyle.Fill, Visible = false };

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            _chValorTop = NewChart("Top productos por valor de stock");
            _chCriticos = NewChart("Críticos vs OK");

            layout.Controls.Add(_chValorTop, 0, 0);
            layout.Controls.Add(_chCriticos, 1, 0);

            _pnlCharts.Controls.Add(layout);
            pnl_dgv_Inventario.Controls.Add(_pnlCharts);
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

            _chValorTop.Series.Clear();
            _chCriticos.Series.Clear();

            if (dt.Columns.Contains("Producto") && dt.Columns.Contains("ValorStock"))
            {
                var s = new Series("USD") { ChartType = SeriesChartType.Column };

                var top = dt.AsEnumerable()
                            .OrderByDescending(r => SafeDec(r, "ValorStock"))
                            .Take(10);

                foreach (var r in top)
                    s.Points.AddXY(Convert.ToString(r["Producto"]), (double)SafeDec(r, "ValorStock"));

                _chValorTop.Series.Add(s);
                _chValorTop.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            }

            if (dt.Columns.Contains("Critico"))
            {
                int crit = dt.AsEnumerable().Count(r => SafeInt(r, "Critico") == 1);
                int ok = dt.Rows.Count - crit;

                var p = new Series("Items") { ChartType = SeriesChartType.Pie };
                p.Points.AddXY("Críticos", crit);
                p.Points.AddXY("OK", ok);

                _chCriticos.Series.Add(p);
            }
        }

        private string GetToken(ComboBox cb, string def)
        {
            if (cb == null || cb.SelectedItem == null) return def;
            return cb.SelectedItem.ToString();
        }

        private int? GetComboIdNullable(ComboBox cb)
        {
            if (cb == null) return null;
            if (cb.SelectedValue == null) return null;

            int id;
            if (int.TryParse(cb.SelectedValue.ToString(), out id))
            {
                if (id == 0) return null;
                return id;
            }
            return null;
        }

        private int SafeInt(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) ? x : 0;
        }

        private long SafeLong(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0;
            long x; return long.TryParse(Convert.ToString(r[col]), out x) ? x : 0;
        }

        private decimal SafeDec(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0m;
            decimal d; return decimal.TryParse(Convert.ToString(r[col]), out d) ? d : 0m;
        }
    }
}