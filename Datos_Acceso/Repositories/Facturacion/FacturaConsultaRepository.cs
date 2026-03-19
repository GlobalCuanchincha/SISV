using System;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Common;

namespace Datos_Acceso.Repositories.Facturacion
{
    public sealed class FacturaConsultaRepository
    {
        public DataTable Buscar(string texto, string estadoFiltro, int top = 200)
        {
            return SqlExecutor.ExecuteDataTable(
                "bill.usp_FacturaConsulta_Buscar",
                new SqlParameter("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto),
                new SqlParameter("@EstadoFiltro", string.IsNullOrWhiteSpace(estadoFiltro) ? "Todos" : estadoFiltro),
                new SqlParameter("@Top", top)
            );
        }

        public void Anular(int usuarioId, int facturaId, string motivo)
        {
            SqlExecutor.ExecuteNonQuery(
                "bill.usp_FacturaConsulta_Anular",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@FacturaID", facturaId),
                new SqlParameter("@Motivo", motivo ?? "")
            );
        }

        // Reporte (reusa tus SP existentes)
        public DataTable GetEmpresa() => SqlExecutor.ExecuteDataTable("bill.usp_FacturaReporte_Empresa");

        public DataTable GetCabecera(int facturaId) =>
            SqlExecutor.ExecuteDataTable("bill.usp_FacturaReporte_Cabecera", new SqlParameter("@IdFactura", facturaId));

        public DataTable GetDetalle(int facturaId) =>
            SqlExecutor.ExecuteDataTable("bill.usp_FacturaReporte_Detalle", new SqlParameter("@IdFactura", facturaId));
    }
}