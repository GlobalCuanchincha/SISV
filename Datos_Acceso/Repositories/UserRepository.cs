using System.Data.SqlClient;
using Datos_Acceso.Common;

namespace Datos_Acceso.Repositories
{
    public class UserRepository
    {
        public UserDto GetByLogin(string login)
        {
            return SqlExecutor.ExecuteReaderSingle("sec.sp_User_GetByLogin", rd =>
            {
                return new UserDto
                {
                    UserID = (int)rd["UserID"],
                    LoginName = rd["LoginName"].ToString(),
                    PasswordHash = (byte[])rd["Password"],
                    RoleID = (int)rd["RoleID"],
                    Estado = (bool)rd["Estado_Users"]
                };
            }, new SqlParameter("@LoginName", login));
        }
    }

    public class UserDto
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public byte[] PasswordHash { get; set; }
        public int RoleID { get; set; }
        public bool Estado { get; set; }
    }
}
