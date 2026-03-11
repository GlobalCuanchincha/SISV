using Datos_Acceso.Common;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Reportes
{
    public sealed class ReporteInventarioRepository
    {
        public DataTable ListarCategorias(int usuarioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Inventario_Categorias_Listar",
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable ListarProveedores(int usuarioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Inventario_Proveedores_Listar",
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable BuscarInventario(
            int usuarioId,
            DateTime fechaDesde,
            DateTime fechaHasta,
            string filtrarFecha,
            string texto,
            string sku,
            int? categoriaId,
            int? proveedorId,
            string nombre,
            string stockFiltro,
            string estado,
            decimal? costoMin,
            decimal? costoMax,
            decimal? precioMin,
            decimal? precioMax,
            string ordenar
        )
        {
            var pTexto = new SqlParameter("@Texto", SqlDbType.NVarChar, 100);
            pTexto.Value = string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto;

            var pSku = new SqlParameter("@SKU", SqlDbType.NVarChar, 50);
            pSku.Value = string.IsNullOrWhiteSpace(sku) ? (object)DBNull.Value : sku;

            var pNom = new SqlParameter("@Nombre", SqlDbType.NVarChar, 100);
            pNom.Value = string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : nombre;

            var pCat = new SqlParameter("@CategoriaID", SqlDbType.Int);
            pCat.Value = categoriaId.HasValue ? (object)categoriaId.Value : DBNull.Value;

            var pProv = new SqlParameter("@ProveedorID", SqlDbType.Int);
            pProv.Value = proveedorId.HasValue ? (object)proveedorId.Value : DBNull.Value;

            var pCostoMin = new SqlParameter("@CostoMin", SqlDbType.Decimal) { Precision = 18, Scale = 2 };
            pCostoMin.Value = costoMin.HasValue ? (object)costoMin.Value : DBNull.Value;

            var pCostoMax = new SqlParameter("@CostoMax", SqlDbType.Decimal) { Precision = 18, Scale = 2 };
            pCostoMax.Value = costoMax.HasValue ? (object)costoMax.Value : DBNull.Value;

            var pPrecioMin = new SqlParameter("@PrecioMin", SqlDbType.Decimal) { Precision = 18, Scale = 2 };
            pPrecioMin.Value = precioMin.HasValue ? (object)precioMin.Value : DBNull.Value;

            var pPrecioMax = new SqlParameter("@PrecioMax", SqlDbType.Decimal) { Precision = 18, Scale = 2 };
            pPrecioMax.Value = precioMax.HasValue ? (object)precioMax.Value : DBNull.Value;

            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Reporte_Inventario_Buscar",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@FechaDesde", fechaDesde.Date),
                new SqlParameter("@FechaHasta", fechaHasta.Date),
                new SqlParameter("@FiltrarFecha", filtrarFecha ?? "todos"),
                pTexto,
                pSku,
                pCat,
                pProv,
                pNom,
                new SqlParameter("@StockFiltro", stockFiltro ?? "todos"),
                new SqlParameter("@Estado", estado ?? "todos"),
                pCostoMin, pCostoMax, pPrecioMin, pPrecioMax,
                new SqlParameter("@Ordenar", ordenar ?? "nombre"),
                new SqlParameter("@Debug", 0)
            );
        }
    }
}