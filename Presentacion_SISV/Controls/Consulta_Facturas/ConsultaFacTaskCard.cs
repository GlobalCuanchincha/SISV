using System;
using System.Drawing;
using System.Windows.Forms;
using Dominio_SISV.DTOs.Facturacion;

namespace Union_Formularios_SISV.Controls.Consulta_Facturas
{
    public partial class ConsultaFacTaskCard : UserControl
    {
        public event EventHandler<FacturaSelectedEventArgs> FacturaSeleccionada;

        private FacturaConsultaCardVM _vm;

        public ConsultaFacTaskCard()
        {
            InitializeComponent();
            WireClickRecursive(this);
        }

        private void WireClickRecursive(Control root)
        {
            if (root == null) return;

            root.Click -= Root_Click;
            root.Click += Root_Click;

            foreach (Control c in root.Controls)
                WireClickRecursive(c);
        }

        private void Root_Click(object sender, EventArgs e)
        {
            if (_vm == null) return;
            FacturaSeleccionada?.Invoke(this, new FacturaSelectedEventArgs(_vm.FacturaID, _vm.CodigoFactura));
        }

        public void Bind(FacturaConsultaCardVM vm)
        {
            _vm = vm;
            if (vm == null) return;

            lbl_CodigoFactura_CFactura.Text = vm.CodigoFactura ?? "";
            lbl_Cliente_CFactura.Text = vm.Cliente ?? "";
            lbl_FechaFactura_CFactura.Text = vm.FechaFactura.HasValue ? vm.FechaFactura.Value.ToString("dd/MM/yyyy") : "";
            lbl_Total_CFactura.Text = vm.Total.ToString("0.00");
            lbl_Estado_CFactura.Text = vm.EstadoTexto ?? (vm.IsAnulada ? "Anulada" : "Emitida");

            lbl_Point_CFactura.ForeColor = vm.IsAnulada ? Color.FromArgb(220, 38, 38) : Color.FromArgb(34, 197, 94);
        }

        public void SetSelected(bool selected)
        {
            BackColor = selected ? Color.FromArgb(235, 245, 255) : Color.Transparent;
        }
    }
}