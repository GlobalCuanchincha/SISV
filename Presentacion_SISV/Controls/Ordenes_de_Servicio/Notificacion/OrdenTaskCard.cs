using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ordenes_de_Servicio
{
    public partial class OrdenTaskCard : UserControl
    {
        public event EventHandler CardClicked;
        public int OrdenServicioID { get; private set; }

        public OrdenTaskCard()
        {
            InitializeComponent();

            HookClick(this);
        }

        private void HookClick(Control parent)
        {
            parent.Click += (s, e) => CardClicked?.Invoke(this, EventArgs.Empty);
            foreach (Control c in parent.Controls)
                HookClick(c);
        }

        public void Bind(int ordenServicioId, string codigo, string cliente, string correo, string equipo, string estado)
        {
            OrdenServicioID = ordenServicioId;

            lbl_Orden_Ordenes_UC.Text = codigo ?? "";
            lbl_Nom_Ordenes_UC.Text = cliente ?? "";
            lbl_Correo_Ordenes_UC.Text = correo ?? "";
            lbl_Equipo_Ordenes_UC.Text = equipo ?? "";
            lbl_Estado_Ordenes_UC.Text = estado ?? "";

            // Punto/Color por estado (simple)
            var est = (estado ?? "").ToUpperInvariant();
            if (est.Contains("ENTREG") || est.Contains("CERR"))
                Panel_Estado_Ordenes_UC.BackColor = Color.FromArgb(16, 185, 129);
            else if (est.Contains("PEND") || est.Contains("ESPER"))
                Panel_Estado_Ordenes_UC.BackColor = Color.FromArgb(245, 158, 11);
            else
                Panel_Estado_Ordenes_UC.BackColor = Color.FromArgb(59, 130, 246);
        }

        public void SetSelected(bool selected)
        {
            Pnl_SeleccionOrdenes.BackColor = selected ? Color.FromArgb(235, 245, 255) : Color.White;
        }
    }
}
