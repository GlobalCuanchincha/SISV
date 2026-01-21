using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms.Ventas
{
    public partial class FacturaItemRowCard : UserControl
    {


        private Label lblTitulo = new Label();
        private Label lblSub = new Label();
        private NumericUpDown nudQty = new NumericUpDown();
        private Label lblPU = new Label();
        private Label lblSubTotal = new Label();
        private Button btnRemove = new Button();

        public DetalleItemVM Data { get; private set; }

        public event Action<DetalleItemCard, int> QuantityChanged;
        public event Action<DetalleItemCard> RemoveRequested;

        public int MaxQtyForProducto { get; set; } = 9999; // para validar stock

        public DetalleItemCard()
        {
            this.Height = 70;
            this.Margin = new Padding(8);
            this.Padding = new Padding(12);
            this.BackColor = Color.White;

            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            lblSub.AutoSize = true;
            lblSub.Top = 22;
            lblSub.ForeColor = Color.DimGray;

            nudQty.Width = 70;
            nudQty.Minimum = 1;
            nudQty.Maximum = 9999;
            nudQty.ValueChanged += (_, __) => QuantityChanged?.Invoke(this, (int)nudQty.Value);

            lblPU.AutoSize = true;
            lblSubTotal.AutoSize = true;

            btnRemove.Text = "X";
            btnRemove.Width = 36;
            btnRemove.Height = 28;
            btnRemove.Click += (_, __) => RemoveRequested?.Invoke(this);

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSub);
            this.Controls.Add(nudQty);
            this.Controls.Add(lblPU);
            this.Controls.Add(lblSubTotal);
            this.Controls.Add(btnRemove);

            this.Resize += (_, __) => LayoutControls();
        }

        public void Bind(DetalleItemVM vm)
        {
            Data = vm;

            lblTitulo.Text = vm.Nombre;
            lblSub.Text = $"{vm.Tipo.ToString().ToUpper()} • {vm.Id}";
            nudQty.Value = Math.Max(1, vm.Cantidad);

            lblPU.Text = $"P.Unit: ${vm.PrecioUnit:0.00}";
            lblSubTotal.Text = $"SubTotal: ${vm.Subtotal:0.00}";

            // si es producto, limito por stock
            nudQty.Maximum = (vm.Tipo == CatalogTipo.Producto) ? Math.Max(1, MaxQtyForProducto) : 9999;

            LayoutControls();
        }

        public void UpdateSubtotal()
        {
            if (Data == null) return;
            lblSubTotal.Text = $"SubTotal: ${Data.Subtotal:0.00}";
            LayoutControls();
        }

        private void LayoutControls()
        {
            // layout “tabla”
            nudQty.Left = this.Width - 280;
            nudQty.Top = 20;

            lblPU.Left = this.Width - 190;
            lblPU.Top = 16;

            lblSubTotal.Left = this.Width - 190;
            lblSubTotal.Top = 36;

            btnRemove.Left = this.Width - 60;
            btnRemove.Top = 20;
        }
    }
}
