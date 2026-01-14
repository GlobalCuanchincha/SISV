using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Capa_Corte_Transversal.Security;

namespace Dominio_SISV.Services
{
    public sealed class AuthService
    {
        private readonly string _cs;

        public AuthService()
        {
            _cs = ConfigurationManager.ConnectionStrings["SISV"] != null
                ? ConfigurationManager.ConnectionStrings["SISV"].ConnectionString
                : null;

            if (string.IsNullOrWhiteSpace(_cs))
                throw new InvalidOperationException("No se encontró el connectionString 'SISV' en App.config.");
        }

        public LoginResult TryLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                return LoginResult.Fail("Ingrese el usuario.");

            if (password == null) password = "";

            UsuarioRow u = GetByUsername(username.Trim());
            if (u == null)
                return LoginResult.Fail("Usuario o contraseña incorrectos.");

            if (!u.Activo)
                return LoginResult.Fail("El usuario está desactivado.");

            if (u.PasswordHash == null || u.PasswordHash.Length == 0 ||
                u.PasswordSalt == null || u.PasswordSalt.Length == 0 ||
                u.PasswordIterations <= 0)
            {
                return LoginResult.Fail("El usuario no tiene credenciales válidas configuradas.");
            }

            byte[] computed = PasswordHasher.ComputeHash(
                password,
                u.PasswordSalt,
                u.PasswordIterations,
                u.PasswordHash.Length
            );

            bool ok = PasswordHasher.FixedTimeEquals(u.PasswordHash, computed);

            if (!ok)
                return LoginResult.Fail("Usuario o contraseña incorrectos.");

            return LoginResult.Success(u);
        }

        private UsuarioRow GetByUsername(string username)
        {
            using (var cn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("dbo.sp_Usuario_GetByUsername", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = username;

                cn.Open();
                using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!rd.Read()) return null;

                    var u = new UsuarioRow
                    {
                        UsuarioId = Convert.ToInt32(rd["UsuarioID_Usuarios"]),
                        Username = Convert.ToString(rd["Username_Usuarios"]),
                        PasswordHash = rd["PasswordHash_Usuarios"] == DBNull.Value ? null : (byte[])rd["PasswordHash_Usuarios"],
                        PasswordSalt = rd["PasswordSalt_Usuarios"] == DBNull.Value ? null : (byte[])rd["PasswordSalt_Usuarios"],
                        PasswordIterations = rd["PasswordIterations_Usuarios"] == DBNull.Value ? 0 : Convert.ToInt32(rd["PasswordIterations_Usuarios"]),
                        Nombres = Convert.ToString(rd["Nombres_Usuarios"]),
                        Apellidos = Convert.ToString(rd["Apellidos_Usuarios"]),
                        Email = rd["Email_Usuarios"] == DBNull.Value ? null : Convert.ToString(rd["Email_Usuarios"]),
                        RoleId = rd["RoleID_Usuarios"] == DBNull.Value ? (byte)0 : Convert.ToByte(rd["RoleID_Usuarios"]),
                        Activo = rd["Activo_Usuarios"] != DBNull.Value && Convert.ToBoolean(rd["Activo_Usuarios"])
                    };

                    return u;
                }
            }
        }
    }

    public sealed class UsuarioRow
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int PasswordIterations { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public byte RoleId { get; set; }
        public bool Activo { get; set; }
    }

    public sealed class LoginResult
    {
        public bool Ok { get; private set; }
        public string Error { get; private set; }
        public UsuarioRow Usuario { get; private set; }

        public static LoginResult Success(UsuarioRow u)
            => new LoginResult { Ok = true, Usuario = u };

        public static LoginResult Fail(string msg)
            => new LoginResult { Ok = false, Error = msg };
    }
}
