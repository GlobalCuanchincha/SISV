using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls
{
    public static class DesktopHost
    {
        private static Form _activo;

        public static void Open(Panel panelEscritorio, Form formHijo)
        {
            if (_activo != null)
            {
                _activo.Close();
                _activo.Dispose();
                _activo = null;
            }

            _activo = formHijo;
            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;

            panelEscritorio.Controls.Clear();
            panelEscritorio.Controls.Add(formHijo);
            formHijo.Show();
        }
    }
}