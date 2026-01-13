using System;
using System.Windows.Forms;

namespace Union_Formularios_SISV
{
    public partial class Pedir_Nom_Servidor : Form
    {
        public Pedir_Nom_Servidor()
        {
            InitializeComponent();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            string serverName = txt_Nombre_Servidor.Text;

            this.Close();
        }
    }
}
