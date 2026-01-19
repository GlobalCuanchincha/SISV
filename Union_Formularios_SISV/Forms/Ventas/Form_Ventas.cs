using System;
using System.Linq;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Ventas : Form
    {
        public Form_Ventas()
        {
            InitializeComponent();

            btn_Consultar_View.Click -= btn_Consultar_View_Click;
            btn_Consultar_View.Click += btn_Consultar_View_Click;
        }

        private void btn_Consultar_View_Click(object sender, EventArgs e)
        {
            var main = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();

            if (main == null)
            {
                MessageBox.Show("No se encontró el Panel Principal.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            main.OpenChild(new Form_Ventas_Consulta(), "Ventas / Facturación", "Consultar • Anular");
        }
    }
}