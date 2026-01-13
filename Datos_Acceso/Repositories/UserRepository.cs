using System.Data.SqlClient;
using Datos_Acceso.Common;

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
