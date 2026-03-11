using Datos_Acceso.Common;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Reportes
{
    public sealed class ReporteClientesRepository
    {
        public DataTable ListarOpcionesFiltrarPor(int usuarioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_Cliente_Filtros_Listar",
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable BuscarReporteClientes(
            int usuarioId,
            DateTime fechaDesde,
            DateTime fechaHasta,
            string estado,
            string conFacturas,
            string filtrarPor,
            string texto,
            string ordenar
        )
        {
            // Solo SP (sin SQL)
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_Reporte_Clientes_Buscar",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@FechaDesde", (object)fechaDesde.Date),
                new SqlParameter("@FechaHasta", (object)fechaHasta.Date),
                new SqlParameter("@Estado", (object)(estado ?? "todos")),
                new SqlParameter("@ConFacturas", (object)(conFacturas ?? "todos")),
                new SqlParameter("@FiltrarPor", (object)(filtrarPor ?? "todos")),
                new SqlParameter("@Texto", (object)(string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto)),
                new SqlParameter("@Ordenar", (object)(ordenar ?? "fechaCreacion"))
            );
        }
    }
}