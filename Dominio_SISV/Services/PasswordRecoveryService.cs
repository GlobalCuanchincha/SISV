using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Capa_Corte_Transversal.Config;
using Capa_Corte_Transversal.Helpers;
using Capa_Corte_Transversal.Security;

namespace Dominio_SISV.Services
{
    public sealed class PasswordRecoveryService
    {
        private readonly string _cs;

        public PasswordRecoveryService()
        {
            _cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(_cs))
                throw new Exception("No se encontró el connectionString 'SISV' en App.config.");
        }

        public string SendTemporaryPassword(string usernameOrEmail)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail))
                return "Ingresa tu usuario o correo.";

            // Mensaje “anti-enumeración” (para no revelar si existe o no)
            const string okMsg = "Si el usuario/correo existe, se enviaron instrucciones al correo registrado.";

            var u = GetUserByUsernameOrEmail(usernameOrEmail.Trim());
            if (u == null) return okMsg;
            if (!u.Activo) return okMsg;
            if (string.IsNullOrWhiteSpace(u.Email)) return "Ese usuario no tiene correo registrado. Contacta al administrador.";

            // 1) Generar contraseña temporal
            string tempPass = GenerateTempPassword(12);

            // 2) Generar salt + hash
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            int iterations = 100000;
            byte[] hash = PasswordHasher.ComputeHash(tempPass, salt, iterations, 32);

            // 3) Guardar en BD
            SetPassword(u.UsuarioId, hash, salt, iterations);

            // 4) Enviar correo
            var smtp = new SmtpEmailSender(SmtpSettings.FromAppConfig());
            string subject = "SISV - Recuperación de contraseña";
            string body =
                "Se ha solicitado recuperar tu acceso.\n\n" +
                "Usuario: " + u.Username.Trim() + "\n" +
                "Contraseña temporal: " + tempPass + "\n\n" +
                "Recomendación: inicia sesión y cambia tu contraseña desde Gestión de usuarios.\n\n" +
                "Si no fuiste tú, avisa al administrador.";

            smtp.Send(u.Email.Trim(), subject, body);

            return okMsg;
        }

        private UsuarioMini GetUserByUsernameOrEmail(string ident)
        {
            using (var cn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("dbo.sp_Usuario_GetByUsernameOrEmail", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Ident", SqlDbType.NVarChar, 200).Value = ident;

                cn.Open();
                using (var rd = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!rd.Read()) return null;

                    return new UsuarioMini
                    {
                        UsuarioId = Convert.ToInt32(rd["UsuarioID_Usuarios"]),
                        Username = Convert.ToString(rd["Username_Usuarios"]),
                        Email = rd["Email_Usuarios"] == DBNull.Value ? null : Convert.ToString(rd["Email_Usuarios"]),
                        Activo = rd["Activo_Usuarios"] != DBNull.Value && Convert.ToBoolean(rd["Activo_Usuarios"])
                    };
                }
            }
        }

        private void SetPassword(int usuarioId, byte[] hash, byte[] salt, int iterations)
        {
            using (var cn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("sec.usp_Usuario_SetPassword", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;
                cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 255).Value = hash;
                cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 255).Value = salt;
                cmd.Parameters.Add("@PasswordIterations", SqlDbType.Int).Value = iterations;

                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private sealed class UsuarioMini
        {
            public int UsuarioId;
            public string Username;
            public string Email;
            public bool Activo;
        }

        private static string GenerateTempPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#?$%";
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[bytes[i] % chars.Length]);

            return sb.ToString();
        }
    }
}
