using System;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms;

namespace Union_Formularios_SISV
{
    public class AppContextSISV : ApplicationContext
    {
        private Form_Login _login;
        private Form_Panel_Principal _main;

        public AppContextSISV()
        {
            _login = new Form_Login();
            _login.LoginSucceeded += Login_LoginSucceeded;

            _login.FormClosed += (s, e) =>
            {
                if (_main == null)
                    ExitThread();
            };

            _login.Show();
        }

        private void Login_LoginSucceeded(object sender, LoginSession session)
        {
            _main = new Form_Panel_Principal(session);
            _main.FormClosed += (s, e) => ExitThread();

            _main.Show();

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
