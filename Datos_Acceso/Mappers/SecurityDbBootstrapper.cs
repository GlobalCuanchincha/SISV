using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;

namespace Union_Formularios_SISV
{
    public static class SecurityDbBootstrapper
    {
        // Cambia el nombre si tu connectionString tiene otro nombre en App.config
        private const string DefaultConnName1 = "SISV";
        private const string DefaultConnName2 = "SISVConnectionString";

        public static bool AnyUserExists()
        {
            using (var cn = new SqlConnection(GetConnectionString()))
            using (var cmd = new SqlCommand("SELECT TOP 1 1 FROM sec.Usuarios;", cn))
            {
                cn.Open();
                var result = cmd.ExecuteScalar();
                return result != null;
            }
        }

        public static void CreateFirstUser_SuperAdmin(
            string username,
            string email,
            string names,
            string lastnames,
            string plainPassword)
        {
            if (AnyUserExists())
                throw new InvalidOperationException("Ya existe al menos un usuario. Este formulario es solo para crear el primer usuario.");

            // 1) Crear/obtener RoleID del rol SuperAdministrador
            int roleId = EnsureRoleAndGetId("SuperAdministrador");

            // 2) Generar hash + salt (PBKDF2)
            byte[] salt;
            byte[] hash = HashPasswordPBKDF2(plainPassword, out salt);

            // 3) Insertar usuario de forma dinámica (mapeando nombres de columnas reales)
            InsertUserDynamic(username, email, names, lastnames, roleId, hash, salt);
        }

        private static int EnsureRoleAndGetId(string roleName)
        {
            // Tu tabla Roles en el script que compartiste luce como:
            // sec.Roles(RoleID_Roles, NombreRol_Roles)
            // Igual lo hacemos dinámico por si cambió.
            using (var cn = new SqlConnection(GetConnectionString()))
            {
                cn.Open();

                var cols = GetColumns(cn, "sec", "Roles");
                string colRoleId = PickColumn(cols, new[]
                {
                    "RoleID_Roles", "RolID_Roles", "RoleID", "RolID", "IdRol", "IdRole"
                });

                string colRoleName = PickColumn(cols, new[]
                {
                    "NombreRol_Roles", "NombreRol", "RolNombre", "Nombre", "Name"
                });

                if (colRoleId == null || colRoleName == null)
                    throw new InvalidOperationException("No se pudieron detectar columnas esperadas en sec.Roles (RoleID/NombreRol).");

                // Si no existe el rol, lo inserta
                string sqlEnsure =
$@"
IF NOT EXISTS(SELECT 1 FROM sec.Roles WHERE {colRoleName} = @name)
BEGIN
    INSERT INTO sec.Roles({colRoleName}) VALUES(@name);
END
SELECT CAST({colRoleId} AS INT) FROM sec.Roles WHERE {colRoleName} = @name;
";
                using (var cmd = new SqlCommand(sqlEnsure, cn))
                {
                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = roleName;
                    object idObj = cmd.ExecuteScalar();
                    if (idObj == null || idObj == DBNull.Value)
                        throw new InvalidOperationException("No se pudo obtener el ID del rol.");
                    return Convert.ToInt32(idObj);
                }
            }
        }

        private static void InsertUserDynamic(
            string username,
            string email,
            string names,
            string lastnames,
            int roleId,
            byte[] hash,
            byte[] salt)
        {
            using (var cn = new SqlConnection(GetConnectionString()))
            {
                cn.Open();

                // Columnas reales de sec.Usuarios
                var columns = GetColumnsWithTypes(cn, "sec", "Usuarios");

                // Detectar columnas por “familias” de nombres (tolerante a tu naming)
                string colUser = PickColumn(columns.Keys, new[]
                {
                    "NombreUsuario_Usuarios", "Usuario_Usuarios", "Username_Usuarios", "UserName_Usuarios",
                    "NombreUsuario", "Usuario", "Username", "UserName"
                });

                string colEmail = PickColumn(columns.Keys, new[]
                {
                    "Email_Usuarios", "Correo_Usuarios", "Correo", "Email"
                });

                string colNames = PickColumn(columns.Keys, new[]
                {
                    "Nombres_Usuarios", "Nombre_Usuarios", "Nombres", "Nombre"
                });

                string colLastnames = PickColumn(columns.Keys, new[]
                {
                    "Apellidos_Usuarios", "Apellido_Usuarios", "Apellidos", "Apellido", "LastName", "Lastname"
                });

                // Hash/Salt (si no existen, intenta “Password_Usuarios” como contenedor del hash)
                string colHash = PickColumn(columns.Keys, new[]
                {
                    "PasswordHash_Usuarios", "HashPassword_Usuarios", "ClaveHash_Usuarios",
                    "Hash_Usuarios", "PasswordHash", "HashPassword", "ContrasenaHash_Usuarios"
                });

                string colSalt = PickColumn(columns.Keys, new[]
                {
                    "PasswordSalt_Usuarios", "Salt_Usuarios", "ClaveSalt_Usuarios",
                    "SaltPassword_Usuarios", "PasswordSalt", "SaltPassword"
                });

                if (colHash == null)
                {
                    // último recurso: columna de password “genérica”
                    colHash = PickColumn(columns.Keys, new[] { "Password_Usuarios", "Contrasena_Usuarios", "Password", "Contrasena" });
                }

                // Role FK
                string colRoleFk = PickColumn(columns.Keys, new[]
                {
                    "RoleID_Usuarios", "RolID_Usuarios", "RoleID", "RolID",
                    "RoleID_Roles", "RolID_Roles"
                });

                // Campos opcionales (si existen)
                string colEstado = PickColumn(columns.Keys, new[]
                {
                    "Estado_Usuarios", "Activo_Usuarios", "Estado", "Activo", "IsActive"
                });

                string colCreated = PickColumn(columns.Keys, new[]
                {
                    "FechaCreacion_Usuarios", "CreatedAt_Usuarios", "FechaCreacion", "CreatedAt"
                });

                // Validaciones mínimas de detección
                if (colUser == null) throw new InvalidOperationException("No se detectó la columna de usuario en sec.Usuarios.");
                if (colRoleFk == null) throw new InvalidOperationException("No se detectó la columna FK del rol en sec.Usuarios.");
                if (colHash == null) throw new InvalidOperationException("No se detectó columna para guardar el hash de contraseña en sec.Usuarios.");

                // Armar insert con columnas disponibles
                var insertCols = new List<string>();
                var insertParams = new List<string>();

                void add(string col, string paramName)
                {
                    if (string.IsNullOrWhiteSpace(col)) return;
                    insertCols.Add(col);
                    insertParams.Add(paramName);
                }

                add(colRoleFk, "@roleId");
                add(colUser, "@user");
                add(colEmail, "@mail");
                add(colNames, "@names");
                add(colLastnames, "@lastnames");
                add(colHash, "@hash");

                if (!string.IsNullOrWhiteSpace(colSalt))
                    add(colSalt, "@salt");

                if (!string.IsNullOrWhiteSpace(colEstado))
                    add(colEstado, "@estado");

                if (!string.IsNullOrWhiteSpace(colCreated))
                    add(colCreated, "@created");

                // Reglas: impedir duplicados por username/email si existen (best effort)
                string sqlExists =
$@"
IF EXISTS(SELECT 1 FROM sec.Usuarios WHERE {colUser} = @user)
    THROW 50001, 'El usuario ya existe.', 1;

" + (colEmail != null ? $@"
IF EXISTS(SELECT 1 FROM sec.Usuarios WHERE {colEmail} = @mail)
    THROW 50002, 'El email ya existe.', 1;
" : "");

                string sqlInsert =
$@"
INSERT INTO sec.Usuarios ({string.Join(", ", insertCols)})
VALUES ({string.Join(", ", insertParams)});
";

                using (var cmd = new SqlCommand(sqlExists + sqlInsert, cn))
                {
                    cmd.Parameters.Add("@roleId", SqlDbType.Int).Value = roleId;
                    cmd.Parameters.Add("@user", SqlDbType.VarChar, 80).Value = username;

                    if (colEmail != null)
                        cmd.Parameters.Add("@mail", SqlDbType.VarChar, 120).Value = email ?? "";

                    if (colNames != null)
                        cmd.Parameters.Add("@names", SqlDbType.VarChar, 120).Value = names ?? "";

                    if (colLastnames != null)
                        cmd.Parameters.Add("@lastnames", SqlDbType.VarChar, 120).Value = lastnames ?? "";

                    // Guardar hash/salt según tipo de columna
                    AddBytesOrBase64(cmd, "@hash", columns[colHash], hash);

                    if (colSalt != null)
                        AddBytesOrBase64(cmd, "@salt", columns[colSalt], salt);

                    if (colEstado != null)
                    {
                        // si es bit -> true; si es varchar/int -> 1
                        var dt = columns[colEstado];
                        if (dt == "bit")
                            cmd.Parameters.Add("@estado", SqlDbType.Bit).Value = true;
                        else if (dt.Contains("int"))
                            cmd.Parameters.Add("@estado", SqlDbType.Int).Value = 1;
                        else
                            cmd.Parameters.Add("@estado", SqlDbType.VarChar, 10).Value = "1";
                    }

                    if (colCreated != null)
                    {
                        cmd.Parameters.Add("@created", SqlDbType.DateTime).Value = DateTime.Now;
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void AddBytesOrBase64(SqlCommand cmd, string paramName, string dataType, byte[] data)
        {
            // Tipos comunes: varbinary, binary, image, timestamp/rowversion, varchar, nvarchar, etc.
            dataType = (dataType ?? "").ToLowerInvariant();

            if (dataType.Contains("binary") || dataType == "image")
            {
                cmd.Parameters.Add(paramName, SqlDbType.VarBinary).Value = (object)data ?? DBNull.Value;
            }
            else
            {
                string b64 = data == null ? "" : Convert.ToBase64String(data);
                cmd.Parameters.Add(paramName, SqlDbType.VarChar).Value = b64;
            }
        }

        private static byte[] HashPasswordPBKDF2(string password, out byte[] salt)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // 100k iteraciones: buen balance
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                return pbkdf2.GetBytes(32); // 256-bit
            }
        }

        private static string GetConnectionString()
        {
            // 1) App.config
            var cs = TryGetConn(DefaultConnName1) ?? TryGetConn(DefaultConnName2);
            if (!string.IsNullOrWhiteSpace(cs))
                return cs;

            throw new InvalidOperationException(
                "No se encontró el connectionString. Agrega en App.config un <connectionStrings> con nombre 'SISV' o 'SISVConnectionString'.");
        }

        private static string TryGetConn(string name)
        {
            try
            {
                var item = ConfigurationManager.ConnectionStrings[name];
                return item?.ConnectionString;
            }
            catch { return null; }
        }

        private static HashSet<string> GetColumns(SqlConnection cn, string schema, string table)
        {
            using (var cmd = new SqlCommand(@"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @s AND TABLE_NAME = @t;", cn))
            {
                cmd.Parameters.Add("@s", SqlDbType.VarChar, 50).Value = schema;
                cmd.Parameters.Add("@t", SqlDbType.VarChar, 128).Value = table;

                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        set.Add(rd.GetString(0));
                }
                return set;
            }
        }

        private static Dictionary<string, string> GetColumnsWithTypes(SqlConnection cn, string schema, string table)
        {
            using (var cmd = new SqlCommand(@"
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = @s AND TABLE_NAME = @t;", cn))
            {
                cmd.Parameters.Add("@s", SqlDbType.VarChar, 50).Value = schema;
                cmd.Parameters.Add("@t", SqlDbType.VarChar, 128).Value = table;

                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                        dict[rd.GetString(0)] = rd.GetString(1);
                }
                return dict;
            }
        }

        private static string PickColumn(IEnumerable<string> available, IEnumerable<string> candidates)
        {
            var set = new HashSet<string>(available ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var c in candidates)
            {
                if (set.Contains(c)) return c;
            }

            // heurística: si no coincidió exacto, intenta por “contains”
            foreach (var c in candidates)
            {
                var hit = set.FirstOrDefault(x => x.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0);
                if (hit != null) return hit;
            }

            return null;
        }
    }
}
