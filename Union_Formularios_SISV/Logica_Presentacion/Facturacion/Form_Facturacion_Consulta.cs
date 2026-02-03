using System;
using System.Linq;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms.Ventas;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Facturacion_Consulta : Form
    {
        public Form_Facturacion_Consulta()
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

            var ventas = new Form_Facturacion();
            ventas.Ventas_RuntimeInit();

            main.OpenChild(ventas, "Ventas / Facturación", "Emitir factura");
        }
    }
}
