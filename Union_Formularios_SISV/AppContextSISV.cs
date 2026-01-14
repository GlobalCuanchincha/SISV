using System;
using System.Windows.Forms;

namespace Union_Formularios_SISV
{
    public class AppContextSISV : ApplicationContext
    {
        private Form_Login _login;
        private Form_Panel_Principal _main;

        public AppContextSISV()
        {
            _login = new Form_Login();

            // Cuando login sea correcto, abrimos el principal
            _login.LoginSucceeded += Login_LoginSucceeded;

            // Si cierran el login SIN entrar, se cierra la app
            _login.FormClosed += (s, e) =>
            {
                if (_main == null)
                    ExitThread();
            };

            _login.Show();
        }

        private void Login_LoginSucceeded(object sender, LoginSession session)
        {
            // Abrir principal
            _main = new Form_Panel_Principal(session);

            // Al cerrar principal, cerrar aplicación
            _main.FormClosed += (s, e) => ExitThread();

            _main.Show();

            // Cerrar login (NO Hide)
            _login.LoginSucceeded -= Login_LoginSucceeded;
            _login.Close();
            _login = null;
        }
    }
}
