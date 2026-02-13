using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Union_Formularios_SISV.Controls.Inventario
{
    public partial class ProductTaskCard : UserControl
    {
        public class ProductoSeleccionadoEventArgs : EventArgs
        {
            public int ProductoID { get; }
            public ProductoSeleccionadoEventArgs(int id) { ProductoID = id; }
        }

        public event EventHandler<ProductoSeleccionadoEventArgs> ProductoSeleccionado;

        public int ProductoID { get; private set; }

        public ProductTaskCard()
        {
            InitializeComponent();
            HookClickRecursive(this);
        }

        private void HookClickRecursive(Control root)
        {
            if (root == null) return;

            root.Cursor = Cursors.Hand;
            root.Click += (s, e) => ProductoSeleccionado?.Invoke(this, new ProductoSeleccionadoEventArgs(ProductoID));

            foreach (Control child in root.Controls)
                HookClickRecursive(child);
        }

        public void Bind(int productoId, string codigo, string nombre, string proveedor, string categoria,
                         int stock, decimal precio, bool activo)
        {
            ProductoID = productoId;

            lbl_Codigo_Producto_USC.Text = codigo ?? "";
            lbl_Nom_Componente_Producto_USC.Text = nombre ?? "";
            lbl_Proveedor_Producto_USC.Text = proveedor ?? "";
            lbl_Categoria_Producto_USC.Text = categoria ?? "";
            lbl_Stock_Producto_USC.Text = $"({stock})";
            lbl_Precio_Producto.Text = precio.ToString("0.00");

            lbl_Estado_USC.Text = activo ? "Activo" : "Inactivo";
            lbl_Point_USC.Text = "●";

            ApplyEstadoStyle(activo);
        }

        private void ApplyEstadoStyle(bool activo)
        {
            // Panel_Estado_Producto_USC puede ser Panel o Guna2Panel
            var pnlGuna = Panel_Estado_Producto_USC as Guna2GradientPanel;
            if (pnlGuna != null)
            {
                pnlGuna.BorderRadius = 12;
                pnlGuna.BorderThickness = 1;
                pnlGuna.FillColor = activo ? Color.FromArgb(232, 250, 240) : Color.FromArgb(255, 235, 235);
                pnlGuna.BorderColor = activo ? Color.FromArgb(90, 200, 140) : Color.FromArgb(235, 110, 110);
            }
            else
            {
                Panel_Estado_Producto_USC.BackColor = activo ? Color.FromArgb(232, 250, 240) : Color.FromArgb(255, 235, 235);
            }

            lbl_Estado_USC.ForeColor = activo ? Color.FromArgb(16, 122, 70) : Color.FromArgb(190, 30, 30);
            lbl_Point_USC.ForeColor = activo ? Color.FromArgb(16, 122, 70) : Color.FromArgb(190, 30, 30);
        }

        public void SetSelected(bool selected)
        {
            // Panel_ProductTask puede ser Panel o Guna2Panel
            var pnlGuna = Panel_ProductTask as Guna2Panel;
            if (pnlGuna != null)
            {
                pnlGuna.BorderThickness = selected ? 2 : 1;
                pnlGuna.BorderColor = selected ? Color.FromArgb(29, 150, 226) : Color.FromArgb(230, 233, 240);
                pnlGuna.FillColor = selected ? Color.FromArgb(240, 247, 255) : Color.White;
            }
            else
            {
                Panel_ProductTask.BackColor = selected ? Color.FromArgb(240, 247, 255) : Color.White;
            }
        }
    }
}
