using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos_Acceso.Repositories;
using Capa_Corte_Transversal.Security;
namespace Dominio_SISV.Services
{
    public class AuthService
    {
        private readonly UserRepository _repo = new UserRepository();

        public LoginResult Login(string user, string pass)
        {
            var u = _repo.GetByLogin(user);
            if (u == null || !u.IsActive)
                return LoginResult.Fail("Usuario no existe o está inactivo");

            bool ok = PasswordHasher.Verify(
                pass,
                u.Hash,
                u.Salt,
                u.Iterations
            );

            if (!ok)
                return LoginResult.Fail("Contraseña incorrecta");

            return LoginResult.Success(u.UserID, u.RoleID);
        }
    }

    public class LoginResult
    {
        public bool Ok;
        public string Error;
        public int UserID;
        public int RoleID;

        public static LoginResult Success(int id, int role)
            => new LoginResult { Ok = true, UserID = id, RoleID = role };

        public static LoginResult Fail(string msg)
            => new LoginResult { Ok = false, Error = msg };
    }
}

