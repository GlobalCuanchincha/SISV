using System;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Common;

namespace Datos_Acceso.Repositories.OrdenesServicio
{
    public sealed class OrdenesRecepcionRepository
    {
        public DataTable EstadosListar()
        {
            return SqlExecutor.ExecuteDataTable("ops.usp_OrdenServicio_Estados_Listar");
        }

        public DataTable TecnicosListarActivos(int usuarioIdActor)
        {
            try
            {
                return SqlExecutor.ExecuteDataTable(
                    "ops.usp_Tecnico_ListarActivos",
                    new SqlParameter("@UsuarioID_Actor", usuarioIdActor)
                );
            }
            catch (SqlException)
            {
                return SqlExecutor.ExecuteDataTable("ops.usp_Tecnico_ListarActivos");
            }
        }

        public DataTable EquiposListarPorCliente(int clienteId)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_Equipo_ListarPorCliente",
                new SqlParameter("@ClienteID", clienteId)
            );
        }

        public DataTable GenerarCodigoOrden()
        {
            return SqlExecutor.ExecuteDataTable("ops.usp_OrdenServicio_GenerarCodigo");
        }

        public DataTable Buscar(string buscar, short estadoValor, int top)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_OrdenServicio_Buscar",
                new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar),
                new SqlParameter("@EstadoValor", estadoValor),
                new SqlParameter("@Top", top)
            );
        }

        public DataTable GetById(int ordenServicioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_OrdenServicio_GetById",
                new SqlParameter("@OrdenServicioID", ordenServicioId)
            );
        }

        public void SetTecnico(int usuarioIdActor, int ordenServicioId, int tecnicoId)
        {
            SqlExecutor.ExecuteNonQuery(
                "ops.usp_OrdenServicio_SetTecnico",
                new SqlParameter("@UsuarioID_Actor", usuarioIdActor),
                new SqlParameter("@OrdenServicioID", ordenServicioId),
                new SqlParameter("@TecnicoID", (object)tecnicoId ?? DBNull.Value)
            );
        }

        public DataTable GuardarRecepcion(
            int usuarioIdActor,
            int? ordenServicioId,
            int clienteId,
            int equipoId,
            int? tecnicoId,
            string problemaReportado,
            string accesoriosRecibidos
        )
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_OrdenServicio_Recepcion_Guardar",
                new SqlParameter("@UsuarioID_Actor", usuarioIdActor),
                new SqlParameter("@OrdenServicioID", (object)ordenServicioId ?? DBNull.Value),
                new SqlParameter("@ClienteID", clienteId),
                new SqlParameter("@EquipoID", equipoId),
                new SqlParameter("@TecnicoID", (object)tecnicoId ?? DBNull.Value),
                new SqlParameter("@ProblemaReportado", string.IsNullOrWhiteSpace(problemaReportado) ? (object)DBNull.Value : problemaReportado),
                new SqlParameter("@AccesoriosRecibidos", string.IsNullOrWhiteSpace(accesoriosRecibidos) ? (object)DBNull.Value : accesoriosRecibidos)
            );
        }

        public DataTable ClienteFiltrosListar(int usuarioIdActor)
        {
            try
            {
                return SqlExecutor.ExecuteDataTable(
                    "ops.usp_Cliente_Filtros_Listar",
                    new SqlParameter("@UsuarioID", usuarioIdActor)
                );
            }
            catch (SqlException)
            {
                return SqlExecutor.ExecuteDataTable(
                    "dbo.usp_Cliente_Filtros_Listar",
                    new SqlParameter("@UsuarioID", usuarioIdActor)
                );
            }
        }

        public DataTable ClientesActivosBuscar(int usuarioIdActor, string filtroPor, string buscar, int top)
        {
            try
            {
                return SqlExecutor.ExecuteDataTable(
                    "ops.usp_Cliente_Activo_Buscar",
                    new SqlParameter("@UsuarioID", usuarioIdActor),
                    new SqlParameter("@FiltroPor", string.IsNullOrWhiteSpace(filtroPor) ? (object)"todos" : filtroPor),
                    new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar),
                    new SqlParameter("@Top", top)
                );
            }
            catch (SqlException)
            {
                return SqlExecutor.ExecuteDataTable(
                    "dbo.usp_Cliente_Activo_Buscar",
                    new SqlParameter("@UsuarioID", usuarioIdActor),
                    new SqlParameter("@FiltroPor", string.IsNullOrWhiteSpace(filtroPor) ? (object)"todos" : filtroPor),
                    new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar),
                    new SqlParameter("@Top", top)
                );
            }
        }
    }
}