using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Reportes
{
    public sealed class ReporteServicioRepository
    {
        private string GetConnectionString()
        {
            string cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs) && ConfigurationManager.ConnectionStrings.Count > 0)
                cs = ConfigurationManager.ConnectionStrings[0]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("No existe connectionString 'SISV' en App.config.");

            return cs;
        }

        private DataTable ExecuteProc(string procName, params SqlParameter[] parameters)
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand(procName, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                con.Open();
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable ListarMetodosPago(int usuarioId)
        {
            return ExecuteProc("bill.usp_ReporteServicio_MetodosPago_Listar");
        }

        public DataTable ListarEstadosFactura(int usuarioId)
        {
            return ExecuteProc("bill.usp_ReporteServicio_EstadosFactura_Listar");
        }

        public DataTable ListarContenido()
        {
            return ExecuteProc("bill.usp_ReporteServicio_Contenido_Listar");
        }

        public DataTable BuscarClientes(int usuarioId, string buscar)
        {
            return ExecuteProc(
                "crm.usp_ReporteServicio_Clientes_Buscar",
                new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar.Trim())
            );
        }

        public DataTable BuscarTecnicos(int usuarioId, string buscar)
        {
            return ExecuteProc(
                "sec.usp_ReporteServicio_Tecnicos_Buscar",
                new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar.Trim())
            );
        }

        public DataTable BuscarOrdenes(int usuarioId, string buscar, int? tecnicoId)
        {
            return ExecuteProc(
                "ops.usp_ReporteServicio_Ordenes_Buscar",
                new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar.Trim()),
                new SqlParameter("@TecnicoID", tecnicoId.HasValue ? (object)tecnicoId.Value : DBNull.Value)
            );
        }

        public DataTable BuscarReporte(
            DateTime fechaDesde,
            DateTime fechaHasta,
            string texto,
            int? metodoPagoId,
            int? estadoId,
            string contenido,
            int? clienteId,
            int? tecnicoId,
            int? ordenServicioId,
            decimal? totalMin,
            decimal? totalMax)
        {
            return ExecuteProc(
                "bill.usp_ReporteServicio_Buscar",
                new SqlParameter("@FechaDesde", fechaDesde.Date),
                new SqlParameter("@FechaHasta", fechaHasta.Date),
                new SqlParameter("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto.Trim()),
                new SqlParameter("@MetodoPagoID", metodoPagoId.HasValue ? (object)metodoPagoId.Value : DBNull.Value),
                new SqlParameter("@EstadoID", estadoId.HasValue ? (object)estadoId.Value : DBNull.Value),
                new SqlParameter("@Contenido", string.IsNullOrWhiteSpace(contenido) ? (object)"todos" : contenido.Trim().ToLowerInvariant()),
                new SqlParameter("@ClienteID", clienteId.HasValue ? (object)clienteId.Value : DBNull.Value),
                new SqlParameter("@TecnicoID", tecnicoId.HasValue ? (object)tecnicoId.Value : DBNull.Value),
                new SqlParameter("@OrdenServicioID", ordenServicioId.HasValue ? (object)ordenServicioId.Value : DBNull.Value),
                new SqlParameter("@TotalMin", totalMin.HasValue ? (object)totalMin.Value : DBNull.Value),
                new SqlParameter("@TotalMax", totalMax.HasValue ? (object)totalMax.Value : DBNull.Value)
            );
        }
    }
}