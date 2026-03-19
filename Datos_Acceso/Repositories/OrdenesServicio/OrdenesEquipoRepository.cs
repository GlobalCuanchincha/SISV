using System;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Common;

namespace Datos_Acceso.Repositories.OrdenesServicio
{
    public sealed class OrdenesEquipoRepository
    {
        private static DataTable TryExec(string[] sps, params SqlParameter[] ps)
        {
            SqlException last = null;

            foreach (var sp in sps)
            {
                try
                {
                    return SqlExecutor.ExecuteDataTable(sp, ps);
                }
                catch (SqlException ex)
                {
                    last = ex;
                }
            }

            throw last ?? new Exception("No se pudo ejecutar el procedimiento almacenado.");
        }

        public DataTable FiltrosListar(int usuarioId)
        {
            return TryExec(
                new[] { "ops.usp_Equipo_Filtros_Listar", "dbo.usp_Equipo_Filtros_Listar" },
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable TipoEquipoListar(int usuarioId)
        {
            return TryExec(
                new[] { "ops.usp_TipoEquipo_Listar", "dbo.usp_TipoEquipo_Listar" },
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable ConectividadListar(int usuarioId)
        {
            return TryExec(
                new[] { "ops.usp_Conectividad_Listar", "dbo.usp_Conectividad_Listar" },
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable Buscar(int usuarioId, int? clienteId, string filtroPor, string buscar, bool? soloActivos, int top)
        {
            return TryExec(
                new[] { "ops.usp_Equipo_Buscar_v2", "dbo.usp_Equipo_Buscar_v2" },
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@ClienteID", (object)clienteId ?? DBNull.Value),
                new SqlParameter("@FiltroPor", string.IsNullOrWhiteSpace(filtroPor) ? "todos" : filtroPor),
                new SqlParameter("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar),
                new SqlParameter("@SoloActivos", (object)soloActivos ?? DBNull.Value),
                new SqlParameter("@Top", top)
            );
        }

        public DataTable GetById(int usuarioId, int equipoId)
        {
            return TryExec(
                new[] { "ops.usp_Equipo_GetById", "dbo.usp_Equipo_GetById" },
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@EquipoID", equipoId)
            );
        }

        public DataTable GenerarCodigoInterno(int usuarioId)
        {
            return TryExec(
                new[] { "ops.usp_Equipo_GenerarCodigoInterno", "dbo.usp_Equipo_GenerarCodigoInterno" },
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable Guardar(
            int usuarioId,
            int? equipoId,
            int clienteId,
            int tipoEquipoId,
            string codigoInterno,
            string marca,
            string modelo,
            string serie,
            string color,
            string conectividad,
            string accesorios,
            string observaciones,
            bool activo
        )
        {
            return TryExec(
                new[] { "ops.usp_Equipo_Guardar", "dbo.usp_Equipo_Guardar" },
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@EquipoID", (object)equipoId ?? DBNull.Value),
                new SqlParameter("@ClienteID", clienteId),
                new SqlParameter("@TipoEquipoID", tipoEquipoId),
                new SqlParameter("@CodigoInterno", codigoInterno ?? ""),
                new SqlParameter("@Marca", (object)marca ?? DBNull.Value),
                new SqlParameter("@Modelo", (object)modelo ?? DBNull.Value),
                new SqlParameter("@Serie", (object)serie ?? DBNull.Value),
                new SqlParameter("@Color", (object)color ?? DBNull.Value),
                new SqlParameter("@Conectividad", (object)conectividad ?? DBNull.Value),
                new SqlParameter("@Accesorios", (object)accesorios ?? DBNull.Value),
                new SqlParameter("@Observaciones", (object)observaciones ?? DBNull.Value),
                new SqlParameter("@Activo", activo ? 1 : 0)
            );
        }
    }
}