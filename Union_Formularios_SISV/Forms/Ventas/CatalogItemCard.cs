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
    public partial class CatalogItemCard : UserControl
    {
        private Label lblTitulo = new Label();
        private Label lblSub = new Label();
        private Label lblRight = new Label(); 
        private Label lblEstado = new Label();

        public CatalogItemVM Data { get; private set; }
        public bool IsSelected { get; private set; }

        public event EventHandler Selected;
        public CatalogItemCard()
        {
            this.Height = 64;
            this.Dock = DockStyle.Top;
            this.Margin = new Padding(8);
            this.Padding = new Padding(12);
            this.BackColor = Color.White;
            this.Cursor = Cursors.Hand;

            lblTitulo.AutoSize = true;
            lblTitulo.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            lblSub.AutoSize = true;
            lblSub.Top = 22;
            lblSub.ForeColor = Color.DimGray;

            lblRight.AutoSize = true;
            lblRight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRight.TextAlign = ContentAlignment.TopRight;

            lblEstado.AutoSize = true;
            lblEstado.Top = 34;

            this.Controls.Add(lblTitulo);
            this.Controls.Add(lblSub);
            this.Controls.Add(lblRight);
            this.Controls.Add(lblEstado);

            this.Click += (_, __) => Selected?.Invoke(this, EventArgs.Empty);
            foreach (Control c in this.Controls)
                c.Click += (_, __) => Selected?.Invoke(this, EventArgs.Empty);

            this.Resize += (_, __) =>
            {
                lblRight.Left = this.Width - lblRight.Width - 12;
            };
        }

        public void Bind(CatalogItemVM vm)
        {
            Data = vm;

            lblTitulo.Text = vm.Nombre;
            lblSub.Text = $"{vm.Id} • {vm.Tipo.ToString().ToUpper()}";
            lblRight.Text = $"${vm.Precio:0.00}";

            if (vm.Tipo == CatalogTipo.Producto)
                lblRight.Text += $"\nStock: {vm.Stock ?? 0}";

            if (vm.Disponible)
            {
                lblEstado.Text = "Disponible";
                lblEstado.ForeColor = Color.SeaGreen;
            }
            else
            {
                lblEstado.Text = "Sin stock";
                lblEstado.ForeColor = Color.IndianRed;
            }

            ApplySelected(false);
            lblRight.Left = this.Width - lblRight.Width - 12;
        }

        public void ApplySelected(bool selected)
        {
            IsSelected = selected;

            this.BackColor = selected ? Color.FromArgb(235, 242, 255) : Color.White;
            this.BorderStyle = selected ? BorderStyle.FixedSingle : BorderStyle.None;
        }
    }
}
