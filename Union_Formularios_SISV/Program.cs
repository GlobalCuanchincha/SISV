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
            string filePath = "servidor_config.txt";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!File.Exists(filePath))
            {
                Application.Run(new Pedir_Nom_Servidor());
            }
            else
            {
                Application.Run(new Form_Login());
            }
        }
    }
}