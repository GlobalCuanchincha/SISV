using Dominio_SISV.DTOs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ventas
{
    public partial class CatalogItemCard : UserControl
    {
        public CatalogItemVM Item { get; private set; }

        public event EventHandler<CatalogItemVM> ItemClicked;

        public CatalogItemCard()
        {
            InitializeComponent();
            WireEvents();
            ApplyBaseStyle();
        }

        private void WireEvents()
        {
            // Click en todo
            this.Click += (s, e) => RaiseItemClicked();
            Panel_Carta.Click += (s, e) => RaiseItemClicked();

            lbl_Nom_Componente.Click += (s, e) => RaiseItemClicked();
            lbl_CodigoTipo.Click += (s, e) => RaiseItemClicked();
            lbl_Precio.Click += (s, e) => RaiseItemClicked();
            lbl_Stock.Click += (s, e) => RaiseItemClicked();

            Panel_Chip.Click += (s, e) => RaiseItemClicked();
            chip_Estado.Click += (s, e) => RaiseItemClicked();
        }

        private void RaiseItemClicked()
        {
            if (Item == null) return;
            ItemClicked?.Invoke(this, Item);
        }

        private void ApplyBaseStyle()
        {
            // Tu panel ya existe en el designer, solo le damos estilo base
            Panel_Carta.BorderRadius = 14;
            Panel_Carta.BorderThickness = 1;
            Panel_Carta.BorderColor = Color.FromArgb(230, 232, 239);
        }

        public void Bind(CatalogItemVM vm)
        {
            Item = vm ?? throw new ArgumentNullException(nameof(vm));

            lbl_Nom_Componente.Text = Item.Nombre ?? "—";
            lbl_CodigoTipo.Text = $"{Item.Codigo} • {Item.Tipo?.ToUpper() ?? "—"}";
            lbl_Precio.Text = Item.Precio.ToString("C2");

            // Stock: si es servicio, puedes mostrar "—"
            if ((Item.Tipo ?? "").ToUpper() == "SERVICIO")
                lbl_Stock.Text = "(—)";
            else
                lbl_Stock.Text = $"({(Item.Stock ?? 0)})";

            ApplyEstadoChip();
            SetSelected(false);
        }

        private void ApplyEstadoChip()
        {
            // Disponible / Sin stock (según tu diseño)
            if (Item == null) return;

            bool disponible = Item.Disponible;

            if (disponible)
            {
                chip_Estado.Text = "Disponible";
                chip_Estado.FillColor = Color.FromArgb(245, 245, 245);
                chip_Estado.ForeColor = Color.FromArgb(75, 85, 99);
                chip_Estado.BorderColor = Color.FromArgb(236, 238, 245);
            }
            else
            {
                chip_Estado.Text = "Sin stock";
                chip_Estado.FillColor = Color.FromArgb(255, 235, 235);
                chip_Estado.ForeColor = Color.FromArgb(153, 27, 27);
                chip_Estado.BorderColor = Color.FromArgb(255, 199, 199);
            }
        }

        public void SetSelected(bool selected)
        {
            // Efecto visual de selección (sin MessageBox)
            if (selected)
            {
                Panel_Carta.BorderColor = Color.FromArgb(37, 99, 235);
                Panel_Carta.FillColor = Color.FromArgb(240, 246, 255);
            }
            else
            {
                Panel_Carta.BorderColor = Color.FromArgb(230, 232, 239);
                Panel_Carta.FillColor = Color.White;
            }
        }
    }
}
