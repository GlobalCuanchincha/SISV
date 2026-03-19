using Datos_Acceso.Common;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Inventario
{
    public sealed class InventarioRepository
    {
        public DataTable ListarCategorias(int usuarioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_CategoriaInventario_Listar",
                new SqlParameter("@UsuarioID_Actor", usuarioId)
            );
        }

        public DataTable Buscar(int usuarioId, string texto, int? categoriaId, string estadoTexto, int top)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_ItemsInventario_Buscar",
                new SqlParameter("@UsuarioID_Actor", usuarioId),
                new SqlParameter("@Buscar",
                    string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto),
                new SqlParameter("@CategoriaID",
                    categoriaId.HasValue ? (object)categoriaId.Value : DBNull.Value),
                new SqlParameter("@Estado",
                    string.IsNullOrWhiteSpace(estadoTexto) ? (object)"Todos" : estadoTexto),
                new SqlParameter("@Top", top)
            );
        }

        public DataTable GetById(int usuarioId, int productoId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_ItemsInventario_GetById",
                new SqlParameter("@UsuarioID_Actor", usuarioId),
                new SqlParameter("@ProductoID", productoId)
            );
        }

        public DataTable Guardar(
            int usuarioId,
            int? productoId,
            string codigo,
            string nombre,
            int categoriaId,
            int? proveedorId,
            int stock,
            int stockMinimo,
            decimal precioVenta,
            decimal? costo,
            string descripcion,
            bool activo)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_ItemsInventario_Guardar",
                new SqlParameter("@UsuarioID_Actor", usuarioId),
                new SqlParameter("@ProductoID", productoId.HasValue ? (object)productoId.Value : DBNull.Value),
                new SqlParameter("@Codigo", codigo ?? ""),
                new SqlParameter("@Nombre", nombre ?? ""),
                new SqlParameter("@CategoriaID", categoriaId),
                new SqlParameter("@ProveedorID", proveedorId.HasValue ? (object)proveedorId.Value : DBNull.Value),
                new SqlParameter("@Stock", stock),
                new SqlParameter("@StockMinimo", stockMinimo),
                new SqlParameter("@PrecioVenta", precioVenta),
                new SqlParameter("@Costo", costo.HasValue ? (object)costo.Value : DBNull.Value),
                new SqlParameter("@Descripcion", string.IsNullOrWhiteSpace(descripcion) ? (object)DBNull.Value : descripcion),
                new SqlParameter("@Activo", activo)
            );
        }

        public DataTable SetActivo(int usuarioId, int productoId, bool activo)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_ItemsInventario_SetActivo",
                new SqlParameter("@UsuarioID_Actor", usuarioId),
                new SqlParameter("@ProductoID", productoId),
                new SqlParameter("@Activo", activo)
            );
        }

        public DataTable GenerarCodigo(int usuarioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_ItemsInventario_GenerarCodigo",
                new SqlParameter("@UsuarioID_Actor", usuarioId)
            );
        }

        public DataTable BuscarProveedores(int usuarioId, string buscar)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Proveedores_Buscar",
                new SqlParameter("@UsuarioID_Actor", usuarioId),
                new SqlParameter("@Buscar",
                    string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar),
                new SqlParameter("@SoloActivos", 1),
                new SqlParameter("@Top", 200)
            );
        }

        public DataTable GetProveedorById(int usuarioId, int proveedorId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Proveedores_GetById",
                new SqlParameter("@UsuarioID_Actor", usuarioId),
                new SqlParameter("@ProveedorID", proveedorId)
            );
        }
    }
}