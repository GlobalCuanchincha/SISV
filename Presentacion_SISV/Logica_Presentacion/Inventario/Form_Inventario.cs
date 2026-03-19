using Capa_Corte_Transversal.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Clientes;
using Union_Formularios_SISV.Controls.Inventario;
using Union_Formularios_SISV.Logica_Presentacion.Inventario;

namespace Union_Formularios_SISV.Forms.Inventario
{
    public partial class Form_Inventario : Form, IInventarioView
    {
        private readonly object _session;
        private readonly Timer _debounceBuscar = new Timer { Interval = 350 };
        private readonly InventarioPresenter _presenter;

        private bool _isBinding;

        public Form_Inventario() : this(null) { }

        public Form_Inventario(object session)
        {
            InitializeComponent();

            _session = session;
            _presenter = new InventarioPresenter(this);

            Load += async (s, e) => await Form_Inventario_Load();
        }

        private async System.Threading.Tasks.Task Form_Inventario_Load()
        {
            flowProductCard.FlowDirection = FlowDirection.TopDown;
            flowProductCard.WrapContents = false;
            flowProductCard.AutoScroll = true;

            _debounceBuscar.Tick += async (_, __) =>
            {
                _debounceBuscar.Stop();
                await _presenter.BuscarAsync();
            };

            txt_Buscador_Productos.TextChanged += (_, __) =>
            {
                if (_isBinding) return;
                _debounceBuscar.Stop();
                _debounceBuscar.Start();
            };

            cmbox_CategoriaFiltro_Productos.SelectedIndexChanged += async (_, __) =>
            {
                if (_isBinding) return;
                await _presenter.BuscarAsync();
            };

            cmbox_EstadoFiltro_Producto.SelectedIndexChanged += async (_, __) =>
            {
                if (_isBinding) return;
                await _presenter.BuscarAsync();
            };

            btn_Registrar_Producto.Click += async (_, __) => await _presenter.GuardarAsync();
            btn_Desactivar_Producto.Click += async (_, __) => await _presenter.ToggleActivoAsync();
            btn_Limpiar_Productos.Click += async (_, __) => await _presenter.LimpiarAsync();
            btn_ElegirProveedor_Producto.Click += (_, __) => _presenter.ElegirProveedor();

            nuc_Precio_Producto.DecimalPlaces = 2;
            nuc_Precio_Producto.Maximum = 1000000;
            nuc_Costo_Producto.DecimalPlaces = 2;
            nuc_Costo_Producto.Maximum = 1000000;

            nuc_StockMinimo_Producto.Maximum = 1000000;
            nuc_Stock_Producto.Maximum = 1000000;

            await _presenter.InicializarAsync();
        }

        // =========================
        // IInventarioView
        // =========================

        public int UsuarioId => SessionHelper.GetUsuarioID(_session);

        public string TextoBusqueda => (txt_Buscador_Productos.Text ?? "").Trim();

        public int? CategoriaFiltroId
        {
            get
            {
                int? val = GetComboInt(cmbox_CategoriaFiltro_Productos, "CategoriaID");
                return (val.HasValue && val.Value > 0) ? val : (int?)null;
            }
        }

        public string EstadoFiltroTexto
        {
            get
            {
                if (cmbox_EstadoFiltro_Producto.SelectedItem is ComboItem it)
                    return Convert.ToString(it.Value) ?? "Todos";

                return "Todos";
            }
        }

        public string CodigoProducto
        {
            get => txt_Codigo_Producto.Text ?? "";
            set => txt_Codigo_Producto.Text = value ?? "";
        }

        public string NombreProducto
        {
            get => txt_Nombre_Producto.Text ?? "";
            set => txt_Nombre_Producto.Text = value ?? "";
        }

        public string DescripcionProducto
        {
            get => txt_Descripcion_Producto.Text ?? "";
            set => txt_Descripcion_Producto.Text = value ?? "";
        }

        public int? CategoriaProductoId
        {
            get
            {
                var cmb = FindCombo("cmbox_Categoria_Producto");
                return GetComboInt(cmb, "CategoriaID");
            }
            set
            {
                var cmb = FindCombo("cmbox_Categoria_Producto");
                if (cmb == null) return;

                try
                {
                    if (value.HasValue)
                        cmb.SelectedValue = value.Value;
                    else if (cmb.Items.Count > 0)
                        cmb.SelectedIndex = 0;
                }
                catch
                {
                    if (cmb.Items.Count > 0)
                        cmb.SelectedIndex = 0;
                }
            }
        }

        public int? ProveedorIdSeleccionado { get; set; }

        public string ProveedorNombreSeleccionado
        {
            get => txt_Proveedor_Producto.Text ?? "";
            set => txt_Proveedor_Producto.Text = value ?? "";
        }

        public decimal StockProducto
        {
            get => nuc_Stock_Producto.Value;
            set => nuc_Stock_Producto.Value = NormalizeNumeric(value, nuc_Stock_Producto);
        }

        public decimal StockMinimoProducto
        {
            get => nuc_StockMinimo_Producto.Value;
            set => nuc_StockMinimo_Producto.Value = NormalizeNumeric(value, nuc_StockMinimo_Producto);
        }

        public decimal PrecioProducto
        {
            get => nuc_Precio_Producto.Value;
            set => nuc_Precio_Producto.Value = NormalizeNumeric(value, nuc_Precio_Producto);
        }

        public decimal CostoProducto
        {
            get => nuc_Costo_Producto.Value;
            set => nuc_Costo_Producto.Value = NormalizeNumeric(value, nuc_Costo_Producto);
        }

        public bool ActivoProducto
        {
            get
            {
                if (cmbox_Estado_Producto.SelectedItem is ComboItem it && it.Value is bool b)
                    return b;

                return true;
            }
            set
            {
                for (int i = 0; i < cmbox_Estado_Producto.Items.Count; i++)
                {
                    if (cmbox_Estado_Producto.Items[i] is ComboItem it && it.Value is bool b && b == value)
                    {
                        cmbox_Estado_Producto.SelectedIndex = i;
                        return;
                    }
                }

                if (cmbox_Estado_Producto.Items.Count > 0)
                    cmbox_Estado_Producto.SelectedIndex = 0;
            }
        }

        public void BindCategorias(DataTable dtCategorias)
        {
            _isBinding = true;
            try
            {
                cmbox_EstadoFiltro_Producto.DisplayMember = "Text";
                cmbox_EstadoFiltro_Producto.ValueMember = "Value";
                cmbox_EstadoFiltro_Producto.Items.Clear();
                cmbox_EstadoFiltro_Producto.Items.Add(new ComboItem("Todos", "Todos"));
                cmbox_EstadoFiltro_Producto.Items.Add(new ComboItem("Activos", "Activos"));
                cmbox_EstadoFiltro_Producto.Items.Add(new ComboItem("Inactivos", "Inactivos"));
                cmbox_EstadoFiltro_Producto.SelectedIndex = 0;

                cmbox_Estado_Producto.DisplayMember = "Text";
                cmbox_Estado_Producto.ValueMember = "Value";
                cmbox_Estado_Producto.Items.Clear();
                cmbox_Estado_Producto.Items.Add(new ComboItem("Activo", true));
                cmbox_Estado_Producto.Items.Add(new ComboItem("Inactivo", false));
                cmbox_Estado_Producto.SelectedIndex = 0;

                var dtFiltro = new DataTable();
                dtFiltro.Columns.Add("CategoriaID", typeof(int));
                dtFiltro.Columns.Add("CategoriaNombre", typeof(string));
                dtFiltro.Rows.Add(0, "Todos");

                if (dtCategorias != null)
                {
                    foreach (DataRow r in dtCategorias.Rows)
                    {
                        dtFiltro.Rows.Add(
                            Convert.ToInt32(r["CategoriaID"]),
                            Convert.ToString(r["CategoriaNombre"]));
                    }
                }

                cmbox_CategoriaFiltro_Productos.DisplayMember = "CategoriaNombre";
                cmbox_CategoriaFiltro_Productos.ValueMember = "CategoriaID";
                cmbox_CategoriaFiltro_Productos.DataSource = dtFiltro;
                cmbox_CategoriaFiltro_Productos.SelectedIndex = 0;

                var cmbCategoriaProducto = FindCombo("cmbox_Categoria_Producto");
                if (cmbCategoriaProducto != null)
                {
                    cmbCategoriaProducto.DisplayMember = "CategoriaNombre";
                    cmbCategoriaProducto.ValueMember = "CategoriaID";
                    cmbCategoriaProducto.DataSource = dtCategorias;
                }
            }
            finally
            {
                _isBinding = false;
            }
        }

        public void RenderCards(DataTable dtProductos, int? selectedId)
        {
            flowProductCard.SuspendLayout();
            try
            {
                flowProductCard.Controls.Clear();

                int cardW = Math.Max(10, flowProductCard.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6);

                if (dtProductos == null) return;

                foreach (DataRow r in dtProductos.Rows)
                {
                    int id = Convert.ToInt32(r["ProductoID"]);
                    string codigo = Convert.ToString(r["Codigo"]);
                    string nombre = Convert.ToString(r["Nombre"]);
                    string proveedor = Convert.ToString(r["ProveedorNombre"]);
                    string categoria = Convert.ToString(r["CategoriaNombre"]);
                    int stock = Convert.ToInt32(r["Stock"]);
                    decimal precio = Convert.ToDecimal(r["PrecioVenta"]);
                    bool activo = Convert.ToBoolean(r["Activo"]);

                    var card = new ProductTaskCard
                    {
                        Width = cardW,
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    card.Bind(id, codigo, nombre, proveedor, categoria, stock, precio, activo);
                    card.SetSelected(selectedId.HasValue && selectedId.Value == id);

                    card.ProductoSeleccionado += async (_, args) =>
                    {
                        await _presenter.SeleccionarAsync(args.ProductoID);
                    };

                    flowProductCard.Controls.Add(card);
                }
            }
            finally
            {
                flowProductCard.ResumeLayout(true);
            }
        }

        public void ClearCardSelection()
        {
            foreach (Control c in flowProductCard.Controls)
            {
                if (c is ProductTaskCard card)
                    card.SetSelected(false);
            }
        }

        public void SetResultados(int total)
        {
            lbl_Num_Resultados_Productos.Text = $"{total} resultados";
        }

        public void SetModoActualizar(bool actualizar)
        {
            btn_Registrar_Producto.Text = actualizar ? "Actualizar" : "Registrar";
        }

        public void SetTextoBotonToggle(string text)
        {
            btn_Desactivar_Producto.Text = string.IsNullOrWhiteSpace(text) ? "Desactivar" : text;
        }

        public void SetAccionesHabilitadas(bool guardar, bool toggleActivo, bool elegirProveedor)
        {
            btn_Registrar_Producto.Enabled = guardar;
            btn_Desactivar_Producto.Enabled = toggleActivo;
            btn_ElegirProveedor_Producto.Enabled = elegirProveedor;
        }

        public bool TryElegirProveedor(int usuarioId, out int? proveedorId, out string proveedorNombre)
        {
            proveedorId = null;
            proveedorNombre = null;

            using (var f = new Control_Proveedores_UC(usuarioId))
            {
                var dr = f.ShowDialog(this);
                if (dr != DialogResult.OK || !f.SelectedProveedorID.HasValue)
                    return false;

                proveedorId = f.SelectedProveedorID.Value;
                proveedorNombre = f.SelectedProveedorNombre ?? "";
                return true;
            }
        }

        public void ShowInfo(string msg)
            => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void ShowWarning(string msg)
            => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowError(string msg, Exception ex)
        {
            string texto = (ex?.Message ?? "").Trim();

            if (EsErrorDeCamposObligatorios(texto))
            {
                MessageBox.Show(
                    "Complete los campos obligatorios del producto antes de guardar.",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (EsErrorDeCosto(texto))
            {
                MessageBox.Show(
                    "Complete el costo del producto.",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (EsErrorDePrecio(texto))
            {
                MessageBox.Show(
                    "Complete el precio del producto.",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                string.IsNullOrWhiteSpace(msg) ? "Ocurrió un error al procesar la operación." : msg,
                "SISV",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static bool EsErrorDeCamposObligatorios(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;

            texto = texto.ToUpperInvariant();

            return texto.Contains("NO SE PUEDE INSERTAR EL VALOR NULL") ||
                   texto.Contains("CANNOT INSERT THE VALUE NULL") ||
                   texto.Contains("NO ADMITE VALORES NULL") ||
                   texto.Contains("COLUMNA") ||
                   texto.Contains("COLUMN");
        }

        private static bool EsErrorDeCosto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;

            texto = texto.ToUpperInvariant();

            return texto.Contains("COSTO") ||
                   texto.Contains("COSTO_ITEMSINVENTARIO");
        }

        private static bool EsErrorDePrecio(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return false;

            texto = texto.ToUpperInvariant();

            return texto.Contains("PRECIO") ||
                   texto.Contains("PRECIOVENTA");
        }

        public void CloseView() => Close();

        // =========================
        // Helpers
        // =========================

        private static decimal NormalizeNumeric(decimal value, dynamic numeric)
        {
            decimal v = value;
            if (v < numeric.Minimum) v = numeric.Minimum;
            if (v > numeric.Maximum) v = numeric.Maximum;
            return v;
        }

        private ComboBox FindCombo(string name)
        {
            var found = this.Controls.Find(name, true);
            if (found != null && found.Length > 0)
                return found[0] as ComboBox;

            return null;
        }

        private static int? GetComboInt(ComboBox cb, string columnName)
        {
            if (cb == null) return null;

            object v = cb.SelectedValue;
            if (v == null) return null;

            if (v is DataRowView drv)
            {
                if (drv.Row == null) return null;
                if (!drv.Row.Table.Columns.Contains(columnName)) return null;

                object cell = drv.Row[columnName];
                if (cell == null || cell == DBNull.Value) return null;

                return Convert.ToInt32(cell);
            }

            try
            {
                return Convert.ToInt32(v);
            }
            catch
            {
                return null;
            }
        }

        private class ComboItem
        {
            public string Text { get; }
            public object Value { get; }

            public ComboItem(string text, object value)
            {
                Text = text;
                Value = value;
            }

            public override string ToString() => Text;
        }
    }
}