using Datos_Acceso.Common;
using System;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories
{
    public class UserRepository
    {
        public UserData GetByLogin(string login)
        {
            return SqlExecutor.ExecuteReaderSingle(
                "dbo.sp_User_GetByLogin",
                rd => new UserData
                {
                    UserID = (int)rd["UserID"],
                    LoginName = rd["LoginName"].ToString(),
                    Hash = (byte[])rd["Password"],
                    Salt = (byte[])rd["PasswordSalt"],
                    Iterations = (int)rd["PasswordIterations"],
                    RoleID = (int)rd["RoleID"],
                    IsActive = (bool)rd["Estado_Users"]
                },
                new SqlParameter("@LoginName", login)
            );
        }
        public UsuarioData GetByUsername(string username)
        {
            return SqlExecutor.ExecuteReaderSingle("dbo.sp_Usuario_GetByUsername", rd => new UsuarioData
            {
                UsuarioID = (int)rd["UsuarioID_Usuarios"],
                Username = rd["Username_Usuarios"].ToString(),
                Hash = rd["PasswordHash_Usuarios"] as byte[],
                Salt = rd["PasswordSalt_Usuarios"] as byte[],
                Iterations = rd["PasswordIterations_Usuarios"] == DBNull.Value ? 10000 : (int)rd["PasswordIterations_Usuarios"],
                Activo = (bool)rd["Activo_Usuarios"],
                RoleID = (byte)rd["RoleID_Usuarios"]
            }, new SqlParameter("@Username", username));
        }

    }
    public class UsuarioData
    {
        public int UsuarioID;
        public string Username;
        public byte[] Hash;
        public byte[] Salt;
        public int Iterations;
        public bool Activo;
        public byte RoleID;
    }


    public class UserData
    {
        public int UserID;
        public string LoginName;
        public byte[] Hash;
        public byte[] Salt;
        public int Iterations;
        public int RoleID;
        public bool IsActive;
    }
}
