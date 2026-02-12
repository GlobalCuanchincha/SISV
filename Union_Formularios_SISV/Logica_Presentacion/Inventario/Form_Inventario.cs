using Capa_Corte_Transversal.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Clientes;
using Union_Formularios_SISV.Controls.Inventario;

namespace Union_Formularios_SISV.Forms.Inventario
{
    public partial class Form_Inventario : Form
    {
        private readonly object _session;

        private readonly int _usuarioId;
        private readonly byte _rolId;

        private int _productoSeleccionadoId = 0;

        // proveedor seleccionado para el producto
        private int? _proveedorIdSeleccionado = null;
        private string _proveedorNombreSeleccionado = null;

        // estado actual del producto seleccionado (para toggle Activar/Desactivar)
        private bool _productoActivoActual = true;

        // Debounce de búsqueda
        private readonly Timer _debounceBuscar = new Timer { Interval = 350 };

        // evita eventos mientras bindea combos
        private bool _isLoading = false;

        public Form_Inventario() : this(null) { }

        public Form_Inventario(object session)
        {
            InitializeComponent();

            _session = session;
            _usuarioId = TryGetUsuarioSesionId();
            _rolId = TryGetRolSesionId();

            Load += async (s, e) => await Form_Inventario_Load();
        }

        private async Task Form_Inventario_Load()
        {
            if (_usuarioId <= 0)
            {
                MessageBox.Show(
                    "No se pudo obtener UsuarioID de sesión.\n\nAbre este formulario pasando la sesión:\nnew Form_Inventario(_session)",
                    "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            // Solo roles: 1 SuperAdmin, 2 Admin, 4 Técnico
            if (_rolId != 1 && _rolId != 2 && _rolId != 4)
            {
                MessageBox.Show("Acceso denegado. Solo SuperAdministrador, Administrador y Técnico.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            _isLoading = true;

            // Debounce buscar
            _debounceBuscar.Tick += async (s, e) =>
            {
                _debounceBuscar.Stop();
                await BuscarProductosAsync();
            };

            txt_Buscador_Productos.TextChanged += (s, e) =>
            {
                if (_isLoading) return;
                _debounceBuscar.Stop();
                _debounceBuscar.Start();
            };

            cmbox_CategoriaFiltro_Productos.SelectedIndexChanged += async (s, e) =>
            {
                if (_isLoading) return;
                await BuscarProductosAsync();
            };

            cmbox_EstadoFiltro_Producto.SelectedIndexChanged += async (s, e) =>
            {
                if (_isLoading) return;
                await BuscarProductosAsync();
            };

            btn_Registrar_Producto.Click += async (s, e) => await GuardarProductoAsync();
            btn_ElegirProveedor_Producto.Click += async (s, e) => await ElegirProveedorAsync();

            // Toggle activar/desactivar
            btn_Desactivar_Producto.Click += async (s, e) => await ToggleActivoProductoAsync();

            // Limpiar (nuevo botón)
            btn_Limpiar_Productos.Click += async (s, e) => await LimpiarCamposAsync();

            await CargarCombosAsync();

            _isLoading = false;

            await LimpiarFormularioNuevoAsync();
            await BuscarProductosAsync();
        }

        // ===================== SESION =====================
        private int TryGetUsuarioSesionId()
        {
            try
            {
                if (_session == null) return 0;
                return SessionHelper.GetUsuarioID(_session);
            }
            catch
            {
                return 0;
            }
        }

        private byte TryGetRolSesionId()
        {
            try
            {
                if (_session == null) return 0;

                var t = _session.GetType();

                var p =
                    t.GetProperty("RoleId") ??
                    t.GetProperty("RoleID") ??
                    t.GetProperty("RolId") ??
                    t.GetProperty("RolID");

                if (p == null) return 0;

                var val = p.GetValue(_session, null);
                if (val == null) return 0;

                return Convert.ToByte(val);
            }
            catch
            {
                return 0;
            }
        }

        // ===================== COMBOS =====================
        private async Task CargarCombosAsync()
        {
            // Estado filtro (Todos / Activos / Inactivos)
            cmbox_EstadoFiltro_Producto.DisplayMember = "Text";
            cmbox_EstadoFiltro_Producto.ValueMember = "Value";
            cmbox_EstadoFiltro_Producto.Items.Clear();
            cmbox_EstadoFiltro_Producto.Items.Add(new ComboItem("Todos", "Todos"));
            cmbox_EstadoFiltro_Producto.Items.Add(new ComboItem("Activos", "Activos"));
            cmbox_EstadoFiltro_Producto.Items.Add(new ComboItem("Inactivos", "Inactivos"));
            cmbox_EstadoFiltro_Producto.SelectedIndex = 0;

            // Estado producto (true/false)
            cmbox_Estado_Producto.DisplayMember = "Text";
            cmbox_Estado_Producto.ValueMember = "Value";
            cmbox_Estado_Producto.Items.Clear();
            cmbox_Estado_Producto.Items.Add(new ComboItem("Activo", true));
            cmbox_Estado_Producto.Items.Add(new ComboItem("Inactivo", false));
            cmbox_Estado_Producto.SelectedIndex = 0;

            // Categorías (SP)
            var dtCat = await ExecDataTableAsync("inv.usp_CategoriaInventario_Listar", cmd =>
            {
                cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
            });

            // Tabla NUEVA para el filtro (sin nulls)
            var dtFiltro = new DataTable();
            dtFiltro.Columns.Add("CategoriaID", typeof(int));
            dtFiltro.Columns.Add("CategoriaNombre", typeof(string));
            dtFiltro.Rows.Add(0, "Todos"); // 0 => Todos

            foreach (DataRow r in dtCat.Rows)
            {
                int id = Convert.ToInt32(r["CategoriaID"]);
                string nom = Convert.ToString(r["CategoriaNombre"]);
                dtFiltro.Rows.Add(id, nom);
            }

            // IMPORTANTE: setear Display/Value ANTES de DataSource evita DataRowView “raro”
            cmbox_CategoriaFiltro_Productos.DisplayMember = "CategoriaNombre";
            cmbox_CategoriaFiltro_Productos.ValueMember = "CategoriaID";
            cmbox_CategoriaFiltro_Productos.DataSource = dtFiltro;
            cmbox_CategoriaFiltro_Productos.SelectedIndex = 0;

            // Combo categoría del panel derecho
            ComboBox cmbCategoriaProducto = FindCombo("cmbox_Categoria_Producto");
            if (cmbCategoriaProducto != null)
            {
                cmbCategoriaProducto.DisplayMember = "CategoriaNombre";
                cmbCategoriaProducto.ValueMember = "CategoriaID";
                cmbCategoriaProducto.DataSource = dtCat;
            }
        }

        // ===================== BUSCAR + FLOW =====================
        private async Task BuscarProductosAsync()
        {
            try
            {
                string buscar = (txt_Buscador_Productos.Text ?? "").Trim();

                // Categoría filtro: 0 => Todos => null
                int? selCat = GetComboInt(cmbox_CategoriaFiltro_Productos, "CategoriaID");
                int? categoriaId = (selCat.HasValue && selCat.Value > 0) ? selCat : (int?)null;

                string estado = "Todos";
                if (cmbox_EstadoFiltro_Producto.SelectedItem is ComboItem st)
                    estado = Convert.ToString(st.Value);

                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_Buscar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar);
                    cmd.Parameters.AddWithValue("@CategoriaID", (object)categoriaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", estado ?? "Todos");
                    cmd.Parameters.AddWithValue("@Top", 200);
                });

                RenderFlowProductos(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderFlowProductos(DataTable dt)
        {
            flowProductCard.SuspendLayout();
            flowProductCard.Controls.Clear();

            int count = 0;

            foreach (DataRow r in dt.Rows)
            {
                int id = Convert.ToInt32(r["ProductoID"]);
                string codigo = Convert.ToString(r["Codigo"]);
                string nombre = Convert.ToString(r["Nombre"]);
                string proveedor = Convert.ToString(r["ProveedorNombre"]);
                string categoria = Convert.ToString(r["CategoriaNombre"]);
                int stock = Convert.ToInt32(r["Stock"]);
                decimal precio = Convert.ToDecimal(r["PrecioVenta"]);
                bool activo = Convert.ToBoolean(r["Activo"]);

                var card = new ProductTaskCard();
                card.Width = flowProductCard.ClientSize.Width - 22;
                card.Bind(id, codigo, nombre, proveedor, categoria, stock, precio, activo);
                card.SetSelected(id == _productoSeleccionadoId);

                card.ProductoSeleccionado += async (s, args) =>
                {
                    await SeleccionarProductoAsync(args.ProductoID);
                };

                flowProductCard.Controls.Add(card);
                count++;
            }

            lbl_Num_Resultados_Productos.Text = $"{count} resultados";
            flowProductCard.ResumeLayout();
        }

        private async Task SeleccionarProductoAsync(int productoId)
        {
            _productoSeleccionadoId = productoId;

            foreach (Control c in flowProductCard.Controls)
                if (c is ProductTaskCard card)
                    card.SetSelected(card.ProductoID == _productoSeleccionadoId);

            UpdateBtnRegistrarText();

            try
            {
                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_GetById", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@ProductoID", productoId);
                });

                if (dt.Rows.Count == 0) return;

                var row = dt.Rows[0];

                txt_Codigo_Producto.Text = Convert.ToString(row["Codigo"]);
                txt_Nombre_Producto.Text = Convert.ToString(row["Nombre"]);
                txt_Descripcion_Producto.Text = Convert.ToString(row["Descripcion"]);

                _proveedorIdSeleccionado = row["ProveedorID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ProveedorID"]);
                _proveedorNombreSeleccionado = Convert.ToString(row["ProveedorNombre"]);
                txt_Proveedor_Producto.Text = _proveedorNombreSeleccionado ?? "";

                _productoActivoActual = Convert.ToBoolean(row["Activo"]);
                SelectEstadoProducto(_productoActivoActual);
                UpdateBtnDesactivarText();

                nuc_Stock_Producto.Value = SafeDecimal(row["Stock"]);
                nuc_StockMinimo_Producto.Value = SafeDecimal(row["StockMinimo"]);
                nuc_Precio_Producto.Value = SafeDecimal(row["PrecioVenta"]);
                nuc_Costo_Producto.Value = SafeDecimal(row["Costo"]);

                ComboBox cmbCategoriaProducto = FindCombo("cmbox_Categoria_Producto");
                if (cmbCategoriaProducto != null && row.Table.Columns.Contains("CategoriaID") && row["CategoriaID"] != DBNull.Value)
                    cmbCategoriaProducto.SelectedValue = Convert.ToInt32(row["CategoriaID"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectEstadoProducto(bool activo)
        {
            for (int i = 0; i < cmbox_Estado_Producto.Items.Count; i++)
            {
                if (cmbox_Estado_Producto.Items[i] is ComboItem it && it.Value is bool b && b == activo)
                {
                    cmbox_Estado_Producto.SelectedIndex = i;
                    return;
                }
            }
            cmbox_Estado_Producto.SelectedIndex = 0;
        }

        // ===================== LIMPIAR =====================
        private async Task LimpiarCamposAsync()
        {
            _productoSeleccionadoId = 0;

            // quita selección visual
            foreach (Control c in flowProductCard.Controls)
                if (c is ProductTaskCard card)
                    card.SetSelected(false);

            await LimpiarFormularioNuevoAsync();
            UpdateBtnRegistrarText();
        }

        private async Task LimpiarFormularioNuevoAsync()
        {
            _proveedorIdSeleccionado = null;
            _proveedorNombreSeleccionado = null;

            txt_Nombre_Producto.Text = "";
            txt_Proveedor_Producto.Text = "";
            txt_Descripcion_Producto.Text = "";

            nuc_Stock_Producto.Value = 0;
            nuc_StockMinimo_Producto.Value = 0;
            nuc_Precio_Producto.Value = 0;
            nuc_Costo_Producto.Value = 0;

            cmbox_Estado_Producto.SelectedIndex = 0;

            ComboBox cmbCategoriaProducto = FindCombo("cmbox_Categoria_Producto");
            if (cmbCategoriaProducto != null && cmbCategoriaProducto.Items.Count > 0)
                cmbCategoriaProducto.SelectedIndex = 0;

            _productoActivoActual = true;
            UpdateBtnDesactivarText();

            // Código sugerido (si existe SP)
            try
            {
                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_GenerarCodigo", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                });

                if (dt.Rows.Count > 0 && dt.Columns.Contains("CodigoSugerido"))
                    txt_Codigo_Producto.Text = Convert.ToString(dt.Rows[0]["CodigoSugerido"]);
                else
                    txt_Codigo_Producto.Text = "";
            }
            catch
            {
                txt_Codigo_Producto.Text = "";
            }
        }

        // ===================== PROVEEDOR =====================
        private async Task ElegirProveedorAsync()
        {
            try
            {
                using (var f = new Control_Proveedores_UC(_usuarioId))
                {
                    var dr = f.ShowDialog(this);
                    if (dr == DialogResult.OK && f.SelectedProveedorID.HasValue)
                    {
                        _proveedorIdSeleccionado = f.SelectedProveedorID.Value;
                        _proveedorNombreSeleccionado = f.SelectedProveedorNombre ?? "";
                        txt_Proveedor_Producto.Text = _proveedorNombreSeleccionado;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Proveedores", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== GUARDAR =====================
        private async Task GuardarProductoAsync()
        {
            try
            {
                string codigo = (txt_Codigo_Producto.Text ?? "").Trim();
                string nombre = (txt_Nombre_Producto.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    MessageBox.Show("Ingrese el código del producto.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("Ingrese el nombre del producto.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                ComboBox cmbCategoriaProducto = FindCombo("cmbox_Categoria_Producto");
                int? categoriaId = GetComboInt(cmbCategoriaProducto, "CategoriaID");
                if (!categoriaId.HasValue || categoriaId.Value <= 0)
                {
                    MessageBox.Show("Seleccione la categoría.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool activo = true;
                if (cmbox_Estado_Producto.SelectedItem is ComboItem est && est.Value is bool b)
                    activo = b;

                int stock = Convert.ToInt32(nuc_Stock_Producto.Value);
                int stockMin = Convert.ToInt32(nuc_StockMinimo_Producto.Value);
                decimal precio = nuc_Precio_Producto.Value;
                decimal? costo = nuc_Costo_Producto.Value <= 0 ? (decimal?)null : nuc_Costo_Producto.Value;

                string descripcion = (txt_Descripcion_Producto.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(descripcion)) descripcion = null;

                bool esNuevo = _productoSeleccionadoId <= 0;

                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_Guardar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@ProductoID", esNuevo ? (object)DBNull.Value : _productoSeleccionadoId);
                    cmd.Parameters.AddWithValue("@Codigo", codigo);
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@CategoriaID", categoriaId.Value);
                    cmd.Parameters.AddWithValue("@ProveedorID", (object)_proveedorIdSeleccionado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Stock", stock);
                    cmd.Parameters.AddWithValue("@StockMinimo", stockMin);
                    cmd.Parameters.AddWithValue("@PrecioVenta", precio);
                    cmd.Parameters.AddWithValue("@Costo", (object)costo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", activo);
                });

                int idGuardado = esNuevo ? Convert.ToInt32(dt.Rows[0]["ProductoID"]) : _productoSeleccionadoId;
                _productoSeleccionadoId = idGuardado;

                _productoActivoActual = activo;
                UpdateBtnDesactivarText();
                UpdateBtnRegistrarText();

                MessageBox.Show(esNuevo ? "Producto registrado." : "Producto actualizado.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await BuscarProductosAsync();
                await SeleccionarProductoAsync(_productoSeleccionadoId);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "SISV - Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===================== ACTIVAR/DESACTIVAR =====================
        private async Task ToggleActivoProductoAsync()
        {
            try
            {
                if (_productoSeleccionadoId <= 0)
                {
                    MessageBox.Show("Seleccione un producto para activar/desactivar.", "SISV",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool nuevoActivo = !_productoActivoActual;

                await ExecDataTableAsync("inv.usp_ItemsInventario_SetActivo", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@ProductoID", _productoSeleccionadoId);
                    cmd.Parameters.AddWithValue("@Activo", nuevoActivo);
                });

                _productoActivoActual = nuevoActivo;
                SelectEstadoProducto(_productoActivoActual);
                UpdateBtnDesactivarText();

                MessageBox.Show(nuevoActivo ? "Producto activado." : "Producto desactivado.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await BuscarProductosAsync();
                await SeleccionarProductoAsync(_productoSeleccionadoId);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "SISV - Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Productos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateBtnDesactivarText()
        {
            if (_productoSeleccionadoId <= 0)
            {
                btn_Desactivar_Producto.Text = "Desactivar";
                return;
            }

            btn_Desactivar_Producto.Text = _productoActivoActual ? "Desactivar" : "Activar";
        }

        private void UpdateBtnRegistrarText()
        {
            btn_Registrar_Producto.Text = (_productoSeleccionadoId > 0) ? "Actualizar" : "Registrar";
        }

        // ===================== HELPERS =====================
        private static decimal SafeDecimal(object o)
        {
            if (o == null || o == DBNull.Value) return 0m;
            return Convert.ToDecimal(o);
        }

        private ComboBox FindCombo(string name)
        {
            var found = this.Controls.Find(name, true);
            if (found != null && found.Length > 0)
                return found[0] as ComboBox;
            return null;
        }

        // ✅ evita error DataRowView -> IConvertible
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

        private class ComboItem
        {
            public string Text { get; }
            public object Value { get; }
            public ComboItem(string text, object value) { Text = text; Value = value; }
            public override string ToString() => Text;
        }
    }
}
