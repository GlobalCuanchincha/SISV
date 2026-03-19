using System;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Common;

namespace Datos_Acceso.Repositories.OrdenesServicio
{
    public sealed class OrdenesNotificacionRepository
    {
        public DataTable EstadosListar()
        {
            return SqlExecutor.ExecuteDataTable("ops.usp_OrdenServicio_Estados_Listar");
        }

        public DataTable ListarParaSeleccion(string filtro, string busqueda)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_OrdenServicio_Listar_Notificacion",
                new SqlParameter("@Filtro",
                    string.IsNullOrWhiteSpace(filtro) ? (object)DBNull.Value : filtro),
                new SqlParameter("@Busqueda",
                    string.IsNullOrWhiteSpace(busqueda) ? (object)DBNull.Value : busqueda)
            );
        }

        public DataTable GetDetalle(int ordenServicioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_OrdenServicio_GetDetalle_Notificacion",
                new SqlParameter("@OrdenServicioID", ordenServicioId)
            );
        }

        public void GuardarDiagnostico(int usuarioId, int ordenServicioId, string diagnostico)
        {
            SqlExecutor.ExecuteNonQuery(
                "ops.usp_OrdenServicio_GuardarDiagnostico",
                new SqlParameter("@OrdenServicioID", ordenServicioId),
                new SqlParameter("@Diagnostico", diagnostico ?? ""),
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public void ActualizarEstado(int usuarioId, int ordenServicioId, int nuevoEstadoId)
        {
            SqlExecutor.ExecuteNonQuery(
                "ops.usp_OrdenServicio_ActualizarEstado",
                new SqlParameter("@OrdenServicioID", ordenServicioId),
                new SqlParameter("@NuevoEstadoID", nuevoEstadoId),
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public int RegistrarNotificacion(
            int usuarioId,
            int ordenServicioId,
            string correo,
            string asunto,
            string mensaje,
            string estadoEnvio,
            string errorDetalle
        )
        {
            object o = SqlExecutor.ExecuteScalar(
                "ops.usp_OrdenServicio_RegistrarNotificacion",
                new SqlParameter("@OrdenServicioID", ordenServicioId),
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Correo", correo ?? ""),
                new SqlParameter("@Asunto", asunto ?? ""),
                new SqlParameter("@Mensaje", mensaje ?? ""),
                new SqlParameter("@EstadoEnvio", estadoEnvio ?? ""),
                new SqlParameter("@ErrorDetalle", (object)errorDetalle ?? DBNull.Value)
            );

            if (o == null || o == DBNull.Value) return 0;
            return Convert.ToInt32(o);
        }
    }
}