using System;
using System.IO;
using System.Windows.Forms;
using Union_Formularios_SISV;

namespace Union_Formularios
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form_Login());
        }
    }
}