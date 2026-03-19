using System;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Common;

namespace Datos_Acceso.Repositories.Facturacion
{
    public sealed class FacturaReporteRepository
    {
        public DataTable GetEmpresa()
        {
            return SqlExecutor.ExecuteDataTable("bill.usp_FacturaReporte_Empresa");
        }

        public DataTable GetCabecera(int idFactura)
        {
            return SqlExecutor.ExecuteDataTable(
                "bill.usp_FacturaReporte_Cabecera",
                new SqlParameter("@IdFactura", idFactura)
            );
        }

        public DataTable GetDetalle(int idFactura)
        {
            return SqlExecutor.ExecuteDataTable(
                "bill.usp_FacturaReporte_Detalle",
                new SqlParameter("@IdFactura", idFactura)
            );
        }
    }
}