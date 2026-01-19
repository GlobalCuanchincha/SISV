using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Ventas_Consulta : Form
    {
        public Form_Ventas_Consulta()
        {
            InitializeComponent();
            btn_EmitirFactura_View.Click -= btn_EmitirFactura_View_Click;
            btn_EmitirFactura_View.Click += btn_EmitirFactura_View_Click;
        }
        private void btn_EmitirFactura_View_Click(object sender, EventArgs e)
        {
            var main = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();

            if (main == null)
            {
                MessageBox.Show("No se encontró el Panel Principal.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            main.OpenChild(new Form_Ventas(), "Ventas / Facturación", "Emitir factura");
        }
    }
}
