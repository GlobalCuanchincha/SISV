using Capa_Corte_Transversal.Loggin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Inventario
{
    public sealed class InventarioPresenter
    {
        private const string PERM_ACCESO = "INV_PRODUCTOS_ACCESO";
        private const string PERM_REGISTRAR = "INV_PRODUCTOS_REGISTRAR";
        private const string PERM_ACTUALIZAR = "INV_PRODUCTOS_ACTUALIZAR";
        private const string PERM_DESACTIVAR = "INV_PRODUCTOS_DESACTIVAR";

        private readonly IInventarioView _view;
        private readonly PermissionContext _perm;

        private int _selectedId = 0;
        private bool _selectedActivo = true;

        public InventarioPresenter(IInventarioView view)
        {
            _view = view;
            _perm = new PermissionContext(
                Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        public async Task InicializarAsync()
        {
            if (_view.UsuarioId <= 0)
            {
                _view.ShowWarning("No se pudo obtener UsuarioID de sesión.");
                _view.CloseView();
                return;
            }

            if (!_perm.HasAny(PERM_ACCESO, PERM_REGISTRAR, PERM_ACTUALIZAR, PERM_DESACTIVAR))
            {
                _view.ShowWarning("Acceso denegado. No tiene permisos para Productos.");
                _view.CloseView();
                return;
            }

            await CargarCategoriasAsync();
            await LimpiarAsync();
            await BuscarAsync();
        }

        public async Task BuscarAsync()
        {
            try
            {
                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_Buscar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _view.UsuarioId);
                    cmd.Parameters.AddWithValue("@Buscar",
                        string.IsNullOrWhiteSpace(_view.TextoBusqueda) ? (object)DBNull.Value : _view.TextoBusqueda);
                    cmd.Parameters.AddWithValue("@CategoriaID",
                        (object)_view.CategoriaFiltroId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado",
                        string.IsNullOrWhiteSpace(_view.EstadoFiltroTexto) ? "Todos" : _view.EstadoFiltroTexto);
                    cmd.Parameters.AddWithValue("@Top", 200);
                });

                _view.SetResultados(dt.Rows.Count);
                _view.RenderCards(dt, _selectedId > 0 ? (int?)_selectedId : null);

                UpdateActionState();
            }
            catch (Exception ex)
            {
                _view.ShowError("Error cargando productos.", ex);
            }
        }

        public async Task SeleccionarAsync(int productoId)
        {
            try
            {
                _selectedId = productoId;

                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_GetById", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _view.UsuarioId);
                    cmd.Parameters.AddWithValue("@ProductoID", productoId);
                });

                if (dt.Rows.Count == 0) return;

                var row = dt.Rows[0];

                _view.CodigoProducto = Convert.ToString(row["Codigo"]);
                _view.NombreProducto = Convert.ToString(row["Nombre"]);
                _view.DescripcionProducto = Convert.ToString(row["Descripcion"]);

                _view.ProveedorIdSeleccionado =
                    row["ProveedorID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ProveedorID"]);

                _view.ProveedorNombreSeleccionado = Convert.ToString(row["ProveedorNombre"]);

                _selectedActivo = Convert.ToBoolean(row["Activo"]);
                _view.ActivoProducto = _selectedActivo;

                _view.StockProducto = ToDecimal(row["Stock"]);
                _view.StockMinimoProducto = ToDecimal(row["StockMinimo"]);
                _view.PrecioProducto = ToDecimal(row["PrecioVenta"]);
                _view.CostoProducto = ToDecimal(row["Costo"]);

                if (row.Table.Columns.Contains("CategoriaID") && row["CategoriaID"] != DBNull.Value)
                    _view.CategoriaProductoId = Convert.ToInt32(row["CategoriaID"]);
                else
                    _view.CategoriaProductoId = null;

                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("Error cargando detalle del producto.", ex);
            }
        }

        public async Task GuardarAsync()
        {
            try
            {
                bool esNuevo = _selectedId <= 0;

                if (esNuevo)
                {
                    if (!_perm.TryEnsure(PERM_REGISTRAR, "No tiene permisos para registrar productos."))
                        return;
                }
                else
                {
                    if (!_perm.TryEnsure(PERM_ACTUALIZAR, "No tiene permisos para actualizar productos."))
                        return;
                }

                string codigo = (_view.CodigoProducto ?? "").Trim();
                string nombre = (_view.NombreProducto ?? "").Trim();
                int? categoriaId = _view.CategoriaProductoId;

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    _view.ShowWarning("Ingrese el código del producto.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    _view.ShowWarning("Ingrese el nombre del producto.");
                    return;
                }

                if (!categoriaId.HasValue || categoriaId.Value <= 0)
                {
                    _view.ShowWarning("Seleccione la categoría.");
                    return;
                }

                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_Guardar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _view.UsuarioId);
                    cmd.Parameters.AddWithValue("@ProductoID", esNuevo ? (object)DBNull.Value : _selectedId);
                    cmd.Parameters.AddWithValue("@Codigo", codigo);
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@CategoriaID", categoriaId.Value);
                    cmd.Parameters.AddWithValue("@ProveedorID", (object)_view.ProveedorIdSeleccionado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Stock", Convert.ToInt32(_view.StockProducto));
                    cmd.Parameters.AddWithValue("@StockMinimo", Convert.ToInt32(_view.StockMinimoProducto));
                    cmd.Parameters.AddWithValue("@PrecioVenta", _view.PrecioProducto);

                    decimal? costo = _view.CostoProducto <= 0 ? (decimal?)null : _view.CostoProducto;
                    cmd.Parameters.AddWithValue("@Costo", (object)costo ?? DBNull.Value);

                    string descripcion = string.IsNullOrWhiteSpace(_view.DescripcionProducto)
                        ? null
                        : _view.DescripcionProducto.Trim();

                    cmd.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", _view.ActivoProducto);
                });

                int idGuardado = _selectedId;

                if (esNuevo)
                {
                    if (dt.Rows.Count == 0 || !dt.Columns.Contains("ProductoID"))
                    {
                        _view.ShowWarning("No se pudo obtener el ID del producto guardado.");
                        return;
                    }

                    idGuardado = Convert.ToInt32(dt.Rows[0]["ProductoID"]);
                }

                _selectedId = idGuardado;
                _selectedActivo = _view.ActivoProducto;

                _view.ShowInfo(esNuevo ? "Producto registrado." : "Producto actualizado.");

                await BuscarAsync();
                await SeleccionarAsync(_selectedId);
            }
            catch (SqlException ex)
            {
                _view.ShowError("Error SQL guardando producto.", ex);
            }
            catch (Exception ex)
            {
                _view.ShowWarning(ex.Message);
            }
        }

        public async Task ToggleActivoAsync()
        {
            try
            {
                if (!_perm.TryEnsure(PERM_DESACTIVAR, "No tiene permisos para activar/desactivar productos."))
                    return;

                if (_selectedId <= 0)
                {
                    _view.ShowWarning("Seleccione un producto para activar/desactivar.");
                    return;
                }

                bool nuevoActivo = !_selectedActivo;

                await ExecDataTableAsync("inv.usp_ItemsInventario_SetActivo", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _view.UsuarioId);
                    cmd.Parameters.AddWithValue("@ProductoID", _selectedId);
                    cmd.Parameters.AddWithValue("@Activo", nuevoActivo);
                });

                _selectedActivo = nuevoActivo;
                _view.ActivoProducto = nuevoActivo;

                _view.ShowInfo(nuevoActivo ? "Producto activado." : "Producto desactivado.");

                await BuscarAsync();
                await SeleccionarAsync(_selectedId);
            }
            catch (SqlException ex)
            {
                _view.ShowError("Error SQL cambiando estado del producto.", ex);
            }
            catch (Exception ex)
            {
                _view.ShowError("Error cambiando estado del producto.", ex);
            }
        }

        public async Task LimpiarAsync()
        {
            _selectedId = 0;
            _selectedActivo = true;

            _view.ClearCardSelection();

            _view.CodigoProducto = "";
            _view.NombreProducto = "";
            _view.DescripcionProducto = "";
            _view.ProveedorIdSeleccionado = null;
            _view.ProveedorNombreSeleccionado = "";
            _view.StockProducto = 0;
            _view.StockMinimoProducto = 0;
            _view.PrecioProducto = 0;
            _view.CostoProducto = 0;
            _view.ActivoProducto = true;
            _view.CategoriaProductoId = null;

            try
            {
                var dt = await ExecDataTableAsync("inv.usp_ItemsInventario_GenerarCodigo", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _view.UsuarioId);
                });

                if (dt.Rows.Count > 0 && dt.Columns.Contains("CodigoSugerido"))
                    _view.CodigoProducto = Convert.ToString(dt.Rows[0]["CodigoSugerido"]);
            }
            catch
            {
                _view.CodigoProducto = "";
            }

            UpdateActionState();
        }

        public void ElegirProveedor()
        {
            try
            {
                bool puedeEditar = (_selectedId > 0 && _perm.Has(PERM_ACTUALIZAR))
                                   || (_selectedId <= 0 && _perm.Has(PERM_REGISTRAR));

                if (!puedeEditar)
                {
                    _view.ShowWarning("No tiene permisos para modificar el proveedor del producto.");
                    return;
                }

                if (_view.TryElegirProveedor(_view.UsuarioId, out int? proveedorId, out string proveedorNombre))
                {
                    _view.ProveedorIdSeleccionado = proveedorId;
                    _view.ProveedorNombreSeleccionado = proveedorNombre ?? "";
                }
            }
            catch (Exception ex)
            {
                _view.ShowError("Error seleccionando proveedor.", ex);
            }
        }

        private async Task CargarCategoriasAsync()
        {
            var dt = await ExecDataTableAsync("inv.usp_CategoriaInventario_Listar", cmd =>
            {
                cmd.Parameters.AddWithValue("@UsuarioID_Actor", _view.UsuarioId);
            });

            _view.BindCategorias(dt);
        }

        private void UpdateActionState()
        {
            bool editando = _selectedId > 0;

            bool guardarHabilitado = editando
                ? _perm.Has(PERM_ACTUALIZAR)
                : _perm.Has(PERM_REGISTRAR);

            bool toggleHabilitado = editando && _perm.Has(PERM_DESACTIVAR);
            bool elegirProveedorHabilitado = guardarHabilitado;

            _view.SetModoActualizar(editando);
            _view.SetTextoBotonToggle(editando
                ? (_selectedActivo ? "Desactivar" : "Activar")
                : "Desactivar");

            _view.SetAccionesHabilitadas(
                guardarHabilitado,
                toggleHabilitado,
                elegirProveedorHabilitado);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value);
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