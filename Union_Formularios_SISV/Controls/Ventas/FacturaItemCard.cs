using Dominio_SISV.DTOs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ventas
{
    public partial class FacturaItemCard : UserControl
    {
        public FacturaItemVM Item { get; private set; }

        public event EventHandler<FacturaItemVM> ItemRemoved;
        public event EventHandler<FacturaItemVM> QuantityChanged;

        private bool _suppress;

        public FacturaItemCard()
        {
            InitializeComponent();
            WireEvents();
            ApplyBaseStyle();
        }

        private void WireEvents()
        {
            nud_Cantidad.ValueChanged += Nud_Cantidad_ValueChanged;

            if (chip_Quitar != null) chip_Quitar.Click += (s, e) => RaiseRemoved();
            if (Panel_Chip != null) Panel_Chip.Click += (s, e) => RaiseRemoved();

            this.Click += (s, e) => { /* no-op */ };
        }

        private void ApplyBaseStyle()
        {
            try
            {
                if (Panel_Chip != null)
                {
                    Panel_Chip.BackColor = Color.Transparent;
                }
            }
            catch {  }
        }

        public void Bind(FacturaItemVM vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            Item = vm;

            _suppress = true;

            lbl_Nom_Componente.Text = Item.Nombre ?? "—";
            lbl_CodigoTipo.Text = string.Format("{0} • {1}", Item.Codigo ?? "—", Item.TipoTexto);
            lbl_PrecioUnit.Text = Item.PrecioUnitario.ToString("C2");

            decimal min = 1m;
            decimal max = 9999m;

            if (Item.EsProducto)
            {
                int stock = Item.Stock ?? 0;
                if (stock < 1) stock = 1;
                max = (decimal)stock;
            }

            nud_Cantidad.Minimum = min;
            nud_Cantidad.Maximum = max;

            int cant = Item.Cantidad < 1 ? 1 : Item.Cantidad;
            if (cant > (int)max) cant = (int)max;

            nud_Cantidad.Value = (decimal)cant;
            Item.Cantidad = cant;

            UpdateSubtotalLabel();

            _suppress = false;
        }

        public void IncrementCantidad(int delta)
        {
            if (Item == null) return;

            int actual = (int)nud_Cantidad.Value;
            int nueva = actual + delta;
            if (nueva < 1) nueva = 1;

            int max = (int)nud_Cantidad.Maximum;
            if (nueva > max) nueva = max;

            SetCantidad(nueva);
        }

        public void SetCantidad(int cantidad)
        {
            if (Item == null) return;

            if (cantidad < 1) cantidad = 1;

            int max = (int)nud_Cantidad.Maximum;
            if (cantidad > max) cantidad = max;

            _suppress = true;
            nud_Cantidad.Value = (decimal)cantidad;
            Item.Cantidad = cantidad;
            UpdateSubtotalLabel();
            _suppress = false;

            QuantityChanged?.Invoke(this, Item);
        }

        private void Nud_Cantidad_ValueChanged(object sender, EventArgs e)
        {
            if (_suppress) return;
            if (Item == null) return;

            int cantidad = (int)nud_Cantidad.Value;
            if (cantidad < 1) cantidad = 1;

            if (Item.EsProducto)
            {
                int stock = Item.Stock ?? 0;
                if (stock < 1) stock = 1;
                if (cantidad > stock) cantidad = stock;

                if ((int)nud_Cantidad.Maximum != stock)
                    nud_Cantidad.Maximum = (decimal)stock;

                if ((int)nud_Cantidad.Value != cantidad)
                {
                    _suppress = true;
                    nud_Cantidad.Value = (decimal)cantidad;
                    _suppress = false;
                }
            }

            Item.Cantidad = cantidad;
            UpdateSubtotalLabel();
            QuantityChanged?.Invoke(this, Item);
        }

        private void UpdateSubtotalLabel()
        {
            if (Item == null) return;
            lbl_Subtotal.Text = Item.Subtotal.ToString("C2");
        }

        private void RaiseRemoved()
        {
            if (Item == null) return;
            ItemRemoved?.Invoke(this, Item);
        }
    }
}
