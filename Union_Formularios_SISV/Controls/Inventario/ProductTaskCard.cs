using Dominio_SISV.DTOs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Inventario
{
    public partial class ProductTaskCard : UserControl
    {
        public event EventHandler<ProductoCardVM> ProductoSeleccionado;

        public ProductoCardVM Data { get; private set; }

        public ProductTaskCard()
        {
            InitializeComponent();

            // Click en toda la tarjeta
            HookClickRecursive(this);
        }

        public void Bind(ProductoCardVM vm)
        {
            Data = vm ?? throw new ArgumentNullException(nameof(vm));

            lbl_Codigo_Producto_USC.Text = vm.Codigo ?? "";
            lbl_Nom_Componente_Producto_USC.Text = vm.Nombre ?? "";
            lbl_Proveedor_Producto_USC.Text = string.IsNullOrWhiteSpace(vm.Proveedor) ? "—" : vm.Proveedor;
            lbl_Categoria_Producto_USC.Text = string.IsNullOrWhiteSpace(vm.Categoria) ? "—" : vm.Categoria;

            lbl_Stock_Producto_USC.Text = $"Stock: {vm.Stock}";
            lbl_Precio_Producto.Text = vm.Precio.ToString("0.00");

            lbl_Estado_USC.Text = vm.Activo ? "Activo" : "Inactivo";
            Panel_Estado_Producto_USC.FillColor = vm.Activo ? Color.FromArgb(16, 185, 129) : Color.FromArgb(148, 163, 184);

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            // Tu contenedor real es guna2Panel1 (según Designer)
            guna2Panel1.BorderColor = selected ? Color.FromArgb(37, 99, 235) : Color.FromArgb(229, 231, 235);
            guna2Panel1.BorderThickness = selected ? 3 : 2;
        }

        private void HookClickRecursive(Control root)
        {
            root.Click += (_, __) => RaiseSelected();
            foreach (Control c in root.Controls)
                HookClickRecursive(c);
        }

        private void RaiseSelected()
        {
            if (Data == null) return;
            ProductoSeleccionado?.Invoke(this, Data);
        }
    }
}
