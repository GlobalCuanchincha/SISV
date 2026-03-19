using Datos_Acceso.Common;
using System.Data;

namespace Datos_Acceso.Repositories.Resumen
{
    public sealed class ResumenRepository
    {
        public DataTable GetKpis()
        {
            return SqlExecutor.ExecuteDataTable("ops.usp_Resumen_Kpis");
        }

        public DataTable GetIngresosUltimos7Dias()
        {
            return SqlExecutor.ExecuteDataTable("ops.usp_Resumen_IngresosUltimos7Dias");
        }

        public DataTable GetStockBajo()
        {
            return SqlExecutor.ExecuteDataTable("ops.usp_Resumen_StockBajo");
        }

        public DataTable GetActividadReciente(int top)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_Resumen_ActividadReciente",
                new System.Data.SqlClient.SqlParameter("@Top", top)
            );
        }
    }
}