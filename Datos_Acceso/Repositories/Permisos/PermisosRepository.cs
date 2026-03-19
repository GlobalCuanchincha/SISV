using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Connection;  

namespace Base_De_Datos_SISV.Repositories.Permisos
{
    public class PermisosRepository
    {
        public void SeedCatalogo()
        {
            ExecNonQuery("sec.usp_Permisos_SeedCatalogo");
        }

        public DataTable ListarUsuariosBasico()
        {
            return ExecDataTable("sec.usp_Usuarios_ListarBasico");
        }

        public DataTable GetPermisosByRol(int roleId)
        {
            return ExecDataTable("sec.usp_Permisos_GetByRol",
                new SqlParameter("@RoleID", SqlDbType.Int) { Value = roleId });
        }

        public DataTable GetPermisosByUsuario(int usuarioId)
        {
            return ExecDataTable("sec.usp_Permisos_GetByUsuario",
                new SqlParameter("@UsuarioIDTarget", SqlDbType.Int) { Value = usuarioId });
        }

        public void SavePermisosByRol(int actorUsuarioId, int roleIdTarget, IEnumerable<string> codigos)
        {
            var tvp = BuildTvp(codigos);

            ExecNonQuery("sec.usp_Permisos_SaveByRol",
                new SqlParameter("@UsuarioIDActor", SqlDbType.Int) { Value = actorUsuarioId },
                new SqlParameter("@RoleIDTarget", SqlDbType.Int) { Value = roleIdTarget },
                new SqlParameter("@Seleccion", SqlDbType.Structured)
                {
                    TypeName = "sec.TVP_PermisoCodigo",
                    Value = tvp
                });
        }
        public DataTable GetPermisosEfectivosByUsuario(int usuarioId)
        {
            return ExecDataTable("sec.usp_Permisos_Efectivos_GetByUsuario",
                new SqlParameter("@UsuarioID", SqlDbType.Int) { Value = usuarioId });
        }

        public void SavePermisosByUsuario(int actorUsuarioId, int usuarioIdTarget, IEnumerable<string> codigos)
        {
            var tvp = BuildTvp(codigos);

            ExecNonQuery("sec.usp_Permisos_SaveByUsuario",
                new SqlParameter("@UsuarioIDActor", SqlDbType.Int) { Value = actorUsuarioId },
                new SqlParameter("@UsuarioIDTarget", SqlDbType.Int) { Value = usuarioIdTarget },
                new SqlParameter("@Seleccion", SqlDbType.Structured)
                {
                    TypeName = "sec.TVP_PermisoCodigo",
                    Value = tvp
                });
        }

        private static DataTable BuildTvp(IEnumerable<string> codigos)
        {
            var dt = new DataTable();
            dt.Columns.Add("Codigo", typeof(string));

            if (codigos != null)
            {
                foreach (var c in codigos)
                {
                    if (string.IsNullOrWhiteSpace(c)) continue;
                    dt.Rows.Add(c.Trim());
                }
            }
            return dt;
        }

        private static DataTable ExecDataTable(string sp, params SqlParameter[] ps)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(sp, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
                var dt = new DataTable();
                cn.Open();
                da.Fill(dt);
                return dt;
            }
        }

        private static void ExecNonQuery(string sp, params SqlParameter[] ps)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (ps != null && ps.Length > 0) cmd.Parameters.AddRange(ps);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}