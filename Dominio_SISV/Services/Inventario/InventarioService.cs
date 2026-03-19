using Datos_Acceso.Repositories.Inventario;
using Dominio_SISV.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace Dominio_SISV.Services
{
    public class InventarioService
    {
        private readonly InventarioRepository _repo;

        public InventarioService()
        {
            _repo = new InventarioRepository();
        }

        public PermisosInventarioVM ObtenerPermisos(byte rolId)
        {
            var vm = new PermisosInventarioVM();

            SetProp(vm, "PuedeEntrar", rolId == 1 || rolId == 2 || rolId == 4);
            SetProp(vm, "PuedeGuardar", rolId == 1 || rolId == 2 || rolId == 4);
            SetProp(vm, "PuedeCambiarEstado", rolId == 1 || rolId == 2);
            SetProp(vm, "PuedeBuscarProveedor", rolId == 1 || rolId == 2 || rolId == 4);

            return vm;
        }

        public Task<List<CategoriaInventarioVM>> ListarCategoriasAsync(int usuarioId)
        {
            var dt = _repo.ListarCategorias(usuarioId);
            var list = new List<CategoriaInventarioVM>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new CategoriaInventarioVM
                {
                    CategoriaId = ToInt(r["CategoriaID"]),
                    Nombre = ToStr(r["CategoriaNombre"])
                });
            }

            return Task.FromResult(list);
        }

        public Task<List<InventarioItemVM>> BuscarAsync(int usuarioId, InventarioFiltroVM filtro)
        {
            string texto = GetProp<string>(filtro, "TextoBusqueda", null);
            if (string.IsNullOrWhiteSpace(texto))
                texto = GetProp<string>(filtro, "Texto", null);

            int? categoriaId = GetNullableIntProp(filtro, "CategoriaId");
            if (!categoriaId.HasValue)
                categoriaId = GetNullableIntProp(filtro, "CategoriaFiltroId");

            string estadoTexto = GetProp<string>(filtro, "EstadoTexto", null);
            if (string.IsNullOrWhiteSpace(estadoTexto))
                estadoTexto = GetProp<string>(filtro, "Estado", "Todos");

            int top = GetProp<int>(filtro, "Top", 200);
            if (top <= 0) top = 200;

            var dt = _repo.Buscar(usuarioId, texto, categoriaId, estadoTexto, top);
            var list = new List<InventarioItemVM>();

            foreach (DataRow r in dt.Rows)
            {
                var item = new InventarioItemVM();

                SetProp(item, "ProductoId", ToInt(r["ProductoID"]));
                SetProp(item, "Codigo", ToStr(r["Codigo"]));
                SetProp(item, "Nombre", ToStr(r["Nombre"]));
                SetProp(item, "ProveedorNombre", ToStr(r["ProveedorNombre"]));
                SetProp(item, "CategoriaNombre", ToStr(r["CategoriaNombre"]));
                SetProp(item, "Stock", ToInt(r["Stock"]));
                SetProp(item, "PrecioVenta", ToDecimal(r["PrecioVenta"]));
                SetProp(item, "Activo", ToBool(r["Activo"]));

                list.Add(item);
            }

            return Task.FromResult(list);
        }

        public Task<InventarioDetalleVM> ObtenerPorIdAsync(int usuarioId, int productoId)
        {
            var dt = _repo.GetById(usuarioId, productoId);
            if (dt == null || dt.Rows.Count == 0)
                return Task.FromResult<InventarioDetalleVM>(null);

            DataRow r = dt.Rows[0];
            var item = new InventarioDetalleVM();

            SetProp(item, "ProductoId", ToInt(r["ProductoID"]));
            SetProp(item, "Codigo", ToStr(r["Codigo"]));
            SetProp(item, "Nombre", ToStr(r["Nombre"]));
            SetProp(item, "Descripcion", ToStr(r["Descripcion"]));
            SetProp(item, "CategoriaId", ToNullableInt(r["CategoriaID"]));
            SetProp(item, "CategoriaNombre", ToStr(r["CategoriaNombre"]));
            SetProp(item, "ProveedorId", ToNullableInt(r["ProveedorID"]));
            SetProp(item, "ProveedorNombre", ToStr(r["ProveedorNombre"]));
            SetProp(item, "Stock", ToInt(r["Stock"]));
            SetProp(item, "StockMinimo", ToInt(r["StockMinimo"]));
            SetProp(item, "PrecioVenta", ToDecimal(r["PrecioVenta"]));
            SetProp(item, "Costo", ToNullableDecimal(r["Costo"]));
            SetProp(item, "Activo", ToBool(r["Activo"]));

            return Task.FromResult(item);
        }

        public Task<string> GenerarCodigoAsync(int usuarioId)
        {
            var dt = _repo.GenerarCodigo(usuarioId);

            string codigo = "";
            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("CodigoSugerido"))
                codigo = ToStr(dt.Rows[0]["CodigoSugerido"]);

            return Task.FromResult(codigo);
        }

        public Task<List<ProveedorPickVM>> BuscarProveedoresAsync(int usuarioId, string buscar)
        {
            var dt = _repo.BuscarProveedores(usuarioId, buscar);
            var list = new List<ProveedorPickVM>();

            foreach (DataRow r in dt.Rows)
            {
                var item = new ProveedorPickVM
                {
                    ProveedorId = ToInt(r["ProveedorID"]),
                    NombreProveedor = ToStr(r["Nombre"]),
                    Ruc = ToStr(r["RUC"]),
                    Telefono = ToStr(r["Telefono"]),
                    Correo = ToStr(r["Correo"]),
                    Activo = dt.Columns.Contains("Activo") && ToBool(r["Activo"])
                };

                list.Add(item);
            }

            return Task.FromResult(list);
        }

        public Task<ProveedorPickVM> ObtenerProveedorAsync(int usuarioId, int proveedorId)
        {
            var dt = _repo.GetProveedorById(usuarioId, proveedorId);
            if (dt == null || dt.Rows.Count == 0)
                return Task.FromResult<ProveedorPickVM>(null);

            DataRow r = dt.Rows[0];

            var item = new ProveedorPickVM
            {
                ProveedorId = ToInt(r["ProveedorID"]),
                NombreProveedor = ToStr(r["Nombre"]),
                Ruc = ToStr(r["RUC"]),
                Telefono = ToStr(r["Telefono"]),
                Correo = ToStr(r["Correo"]),
                Activo = dt.Columns.Contains("Activo") && ToBool(r["Activo"])
            };

            return Task.FromResult(item);
        }

        public async Task<int> GuardarAsync(int usuarioId, byte rolId, InventarioGuardarVM model)
        {
            var permisos = ObtenerPermisos(rolId);

            if (!GetProp<bool>(permisos, "PuedeGuardar", false))
                throw new Exception("No tiene permisos para guardar.");

            string codigo = GetProp<string>(model, "Codigo", "");
            string nombre = GetProp<string>(model, "Nombre", "");
            int categoriaId = GetProp<int>(model, "CategoriaId", 0);

            if (string.IsNullOrWhiteSpace(codigo))
                throw new Exception("Complete el código del producto.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new Exception("Complete el nombre del producto.");

            if (categoriaId <= 0)
                throw new Exception("Seleccione la categoría.");

            int? productoId = GetNullableIntProp(model, "ProductoId");
            int? proveedorId = GetNullableIntProp(model, "ProveedorId");
            int stock = GetProp<int>(model, "Stock", 0);
            int stockMinimo = GetProp<int>(model, "StockMinimo", 0);
            decimal precioVenta = GetProp<decimal>(model, "PrecioVenta", 0m);
            decimal? costo = GetNullableDecimalProp(model, "Costo");
            string descripcion = GetProp<string>(model, "Descripcion", null);
            bool activo = GetProp<bool>(model, "Activo", true);

            if (stock < 0)
                throw new Exception("El stock no puede ser negativo.");

            if (stockMinimo < 0)
                throw new Exception("El stock mínimo no puede ser negativo.");

            if (precioVenta <= 0)
                throw new Exception("Complete el precio del producto.");

            if (!costo.HasValue || costo.Value <= 0)
                throw new Exception("Complete el costo del producto.");

            var dt = _repo.Guardar(
                usuarioId,
                productoId,
                codigo,
                nombre,
                categoriaId,
                proveedorId,
                stock,
                stockMinimo,
                precioVenta,
                costo,
                descripcion,
                activo
            );

            int id = productoId ?? 0;

            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("ProductoID"))
                id = ToInt(dt.Rows[0]["ProductoID"]);

            return await Task.FromResult(id);
        }
        public async Task CambiarEstadoAsync(int usuarioId, byte rolId, int productoId, bool activo)
        {
            var permisos = ObtenerPermisos(rolId);

            if (!GetProp<bool>(permisos, "PuedeCambiarEstado", false))
                throw new Exception("No tiene permisos para cambiar el estado.");

            _repo.SetActivo(usuarioId, productoId, activo);
            await Task.CompletedTask;
        }

        private static string ToStr(object value)
        {
            return value == null || value == DBNull.Value ? "" : Convert.ToString(value);
        }

        private static int ToInt(object value)
        {
            return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static int? ToNullableInt(object value)
        {
            return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }

        private static decimal ToDecimal(object value)
        {
            return value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        private static decimal? ToNullableDecimal(object value)
        {
            return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
        }

        private static bool ToBool(object value)
        {
            return value != null && value != DBNull.Value && Convert.ToBoolean(value);
        }

        private static T GetProp<T>(object obj, string propName, T defaultValue)
        {
            if (obj == null) return defaultValue;

            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return defaultValue;

            var value = prop.GetValue(obj);
            if (value == null) return defaultValue;

            if (typeof(T) == typeof(string))
                return (T)(object)Convert.ToString(value);

            if (typeof(T) == typeof(int))
                return (T)(object)Convert.ToInt32(value);

            if (typeof(T) == typeof(decimal))
                return (T)(object)Convert.ToDecimal(value);

            if (typeof(T) == typeof(bool))
                return (T)(object)Convert.ToBoolean(value);

            return (T)value;
        }

        private static int? GetNullableIntProp(object obj, string propName)
        {
            if (obj == null) return null;

            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return null;

            var value = prop.GetValue(obj);
            if (value == null) return null;

            return Convert.ToInt32(value);
        }

        private static decimal? GetNullableDecimalProp(object obj, string propName)
        {
            if (obj == null) return null;

            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return null;

            var value = prop.GetValue(obj);
            if (value == null) return null;

            return Convert.ToDecimal(value);
        }

        private static void SetProp(object obj, string propName, object value)
        {
            if (obj == null) return;

            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanWrite) return;

            if (value == null)
            {
                prop.SetValue(obj, null);
                return;
            }

            Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            object converted = Convert.ChangeType(value, targetType);
            prop.SetValue(obj, converted);
        }
    }
}