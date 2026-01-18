using System;
using System.Windows.Forms;

namespace Union_Formularios_SISV
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContextSISV());
        }
    }
}
