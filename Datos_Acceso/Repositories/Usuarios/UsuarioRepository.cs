using Datos_Acceso.Connection;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Datos_Acceso.Repositories.Usuarios
{
    public sealed class UsuarioRepository
    {
        // ===== Helpers (TryExec con fallback sec/dbo) =====
        private static async Task<DataTable> ExecDataTableAsync(string spName, Action<SqlCommand> fillParams)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(spName, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fillParams?.Invoke(cmd);

                var dt = new DataTable();
                await cn.OpenAsync();
                da.Fill(dt);
                return dt;
            }
        }

        private static async Task<int> ExecNonQueryAsync(string spName, Action<SqlCommand> fillParams)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(spName, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fillParams?.Invoke(cmd);

                await cn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task<DataTable> TryExecDataTableAsync(string[] candidates, Action<SqlCommand> fillParams)
        {
            Exception last = null;

            foreach (var sp in candidates)
            {
                try
                {
                    return await ExecDataTableAsync(sp, fillParams);
                }
                catch (SqlException ex)
                {
                    last = ex;
                    if (ex.Number == 2812) continue; // SP no existe
                    throw;
                }
            }

            throw last ?? new Exception("No se pudo ejecutar ningún procedimiento almacenado.");
        }

        private static async Task<int> TryExecNonQueryAsync(string[] candidates, Action<SqlCommand> fillParams)
        {
            Exception last = null;

            foreach (var sp in candidates)
            {
                try
                {
                    return await ExecNonQueryAsync(sp, fillParams);
                }
                catch (SqlException ex)
                {
                    last = ex;
                    if (ex.Number == 2812) continue; // SP no existe
                    throw;
                }
            }

            throw last ?? new Exception("No se pudo ejecutar ningún procedimiento almacenado.");
        }

        // ===== SP Calls =====

        public Task<DataTable> RolesListarAsync(bool soloActivos)
        {
            return TryExecDataTableAsync(
                new[] { "sec.usp_Rol_Listar", "dbo.usp_Rol_Listar" },
                cmd => cmd.Parameters.AddWithValue("@SoloActivos", soloActivos ? 1 : 0)
            );
        }

        public Task<DataTable> UsuarioEstadosListarAsync(string modo)
        {
            return TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Estados_Listar", "dbo.usp_Usuario_Estados_Listar" },
                cmd => cmd.Parameters.AddWithValue("@Modo", modo ?? "Filtro")
            );
        }

        public Task<DataTable> UsuarioListarRecientesAsync(int usuarioSesionId, int top)
        {
            return TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_ListarRecientes", "dbo.usp_Usuario_ListarRecientes" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioSesionId);
                    cmd.Parameters.AddWithValue("@Top", top);
                }
            );
        }

        public Task<DataTable> UsuarioBuscarAsync(int usuarioSesionId, string texto, string filtro, string rolFiltro, string estadoFiltro, int top)
        {
            return TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Buscar", "dbo.usp_Usuario_Buscar" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioSesionId);
                    cmd.Parameters.AddWithValue("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto);
                    cmd.Parameters.AddWithValue("@Filtro", string.IsNullOrWhiteSpace(filtro) ? "todos" : filtro);
                    cmd.Parameters.AddWithValue("@RolFiltro", string.IsNullOrWhiteSpace(rolFiltro) ? "Todos" : rolFiltro);
                    cmd.Parameters.AddWithValue("@EstadoFiltro", string.IsNullOrWhiteSpace(estadoFiltro) ? "Todos" : estadoFiltro);
                    cmd.Parameters.AddWithValue("@Top", top);
                }
            );
        }

        public Task<DataTable> UsuarioGetByIdAsync(int usuarioSesionId, int usuarioTargetId)
        {
            return TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_GetById", "dbo.usp_Usuario_GetById" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioSesionId);
                    cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioTargetId);
                }
            );
        }

        public Task<DataTable> UsuarioGuardarAsync(
            int usuarioSesionId,
            int? usuarioTargetId,
            string username,
            string nombres,
            string apellidos,
            string email,
            string telefono,
            int rolId,
            bool activo,
            byte[] passwordHash,
            byte[] passwordSalt,
            int? passwordIterations
        )
        {
            return TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Guardar", "dbo.usp_Usuario_Guardar" },
                cmd =>
                {
                    cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioSesionId;

                    var pTarget = cmd.Parameters.Add("@UsuarioIDTarget", SqlDbType.Int);
                    pTarget.Value = usuarioTargetId.HasValue ? (object)usuarioTargetId.Value : DBNull.Value;

                    cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username ?? "";
                    cmd.Parameters.Add("@Nombres", SqlDbType.NVarChar, 100).Value = nombres ?? "";
                    cmd.Parameters.Add("@Apellidos", SqlDbType.NVarChar, 100).Value = apellidos ?? "";

                    var pEmail = cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200);
                    pEmail.Value = string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email;

                    var pTel = cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 30);
                    pTel.Value = string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono;

                    cmd.Parameters.Add("@RolID", SqlDbType.Int).Value = rolId;
                    cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;

                    var pHash = cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, -1);
                    pHash.Value = (passwordHash != null && passwordHash.Length > 0) ? (object)passwordHash : DBNull.Value;

                    var pSalt = cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, -1);
                    pSalt.Value = (passwordSalt != null && passwordSalt.Length > 0) ? (object)passwordSalt : DBNull.Value;

                    var pIter = cmd.Parameters.Add("@PasswordIterations", SqlDbType.Int);
                    pIter.Value = passwordIterations.HasValue ? (object)passwordIterations.Value : DBNull.Value;
                }
            );
        }

        public Task UsuarioSetPasswordAsync(int usuarioSesionId, int usuarioTargetId, byte[] hash, byte[] salt, int iterations)
        {
            return TryExecNonQueryAsync(
                new[]
                {
                    "sec.usp_Usuario_Password_Set",
                    "sec.usp_Usuario_SetPassword",
                    "sec.usp_Usuario_SetPasswordHash",
                    "dbo.usp_Usuario_SetPassword",
                    "dbo.usp_Usuario_SetPasswordHash"
                },
                cmd =>
                {
                    cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioSesionId;
                    cmd.Parameters.Add("@UsuarioIDTarget", SqlDbType.Int).Value = usuarioTargetId;

                    var pHash = cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, -1);
                    pHash.Value = (hash != null && hash.Length > 0) ? (object)hash : DBNull.Value;

                    var pSalt = cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, -1);
                    pSalt.Value = (salt != null && salt.Length > 0) ? (object)salt : DBNull.Value;

                    var pIter = cmd.Parameters.Add("@PasswordIterations", SqlDbType.Int);
                    pIter.Value = iterations > 0 ? (object)iterations : DBNull.Value;
                }
            );
        }

        public async Task<byte[]> UsuarioFotoGetAsync(int usuarioSesionId, int usuarioTargetId)
        {
            var dt = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Foto_Get", "dbo.usp_Usuario_Foto_Get" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioSesionId);
                    cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioTargetId);
                }
            );

            if (dt.Rows.Count == 0) return null;
            if (!dt.Columns.Contains("Foto")) return null;
            if (dt.Rows[0]["Foto"] == DBNull.Value) return null;

            return (byte[])dt.Rows[0]["Foto"];
        }

        public Task UsuarioFotoSetAsync(int usuarioSesionId, int usuarioTargetId, byte[] fotoBytes)
        {
            return TryExecNonQueryAsync(
                new[] { "sec.usp_Usuario_Foto_Set", "dbo.usp_Usuario_Foto_Set" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioSesionId);
                    cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioTargetId);

                    var pFoto = cmd.Parameters.Add("@Foto", SqlDbType.VarBinary, -1);
                    pFoto.Value = (fotoBytes != null && fotoBytes.Length > 0) ? (object)fotoBytes : DBNull.Value;
                }
            );
        }
    }
}