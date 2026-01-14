using System;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms;

namespace Union_Formularios_SISV
{
    /// <summary>
    /// Maneja el ciclo de vida de la app:
    /// - Muestra Login
    /// - Si Login OK => abre Panel Principal y CIERRA Login
    /// - Si cierran el Panel Principal => termina la app
    /// </summary>
    public class AppContextSISV : ApplicationContext
    {
        private Form_Login _login;
        private Form_Panel_Principal _main;

        public AppContextSISV()
        {
            _login = new Form_Login();
            _login.LoginSucceeded += Login_LoginSucceeded;

            // Si el usuario cierra el Login sin autenticarse, se termina la app
            _login.FormClosed += (s, e) =>
            {
                if (_main == null)
                    ExitThread();
            };

            _login.Show();
        }

        private void Login_LoginSucceeded(object sender, LoginSession session)
        {
            // Abrimos el panel principal
            _main = new Form_Panel_Principal(session);
            _main.FormClosed += (s, e) => ExitThread();

            _main.Show();

            // Cerramos el login (RECOMENDADO: NO Hide)
            if (_login != null)
            {
                _login.LoginSucceeded -= Login_LoginSucceeded;
                _login.Close();
                _login = null;
            }
        }
    }

    public class LoginSession : EventArgs
    {
        public int UsuarioId { get; set; }
        public string Username { get; set; }
        public byte RoleId { get; set; }
    }
}
