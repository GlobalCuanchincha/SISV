using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Recepcion
{
    public partial class RecepcionTaskCard : UserControl
    {
        public int OrdenServicioID { get; private set; }

        public event EventHandler<OrdenSeleccionadaEventArgs> OrdenSeleccionada;

        public RecepcionTaskCard()
        {
            InitializeComponent();
            HookClicks(this);
        }

        private void HookClicks(Control root)
        {
            root.Click += Card_Click;
            foreach (Control c in root.Controls)
                HookClicks(c);
        }

        private void Card_Click(object sender, EventArgs e)
        {
            if (OrdenServicioID > 0)
                OrdenSeleccionada?.Invoke(this, new OrdenSeleccionadaEventArgs(OrdenServicioID));
        }

        // 6 args (como te pide el compilador)
        public void Bind(int ordenServicioId, string codigo, string cliente, string equipo, string tecnico, string estado)
        {
            OrdenServicioID = ordenServicioId;

            lbl_codigoequipo_Recepcion_UC.Text = codigo ?? "";
            lbl_Cliente_Recepcion_UC.Text = cliente ?? "";
            lbl_Equipo_Recepcion_UC.Text = equipo ?? "";
            lbl_Tecnico_Recepcion_UC.Text = tecnico ?? "";
            lbl_Estado_Recepcion_UC.Text = estado ?? "";

            ApplyEstadoColor(estado);
        }

        public void SetSelected(bool selected)
        {
            // usa tu panel contenedor si existe
            if (Panel_Carta_Recepcion_UC != null)
            {
                Panel_Carta_Recepcion_UC.BackColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;
            }
            else
            {
                BackColor = selected ? Color.FromArgb(230, 242, 255) : Color.White;
            }
        }

        private void ApplyEstadoColor(string estado)
        {
            if (Panel_Estado_Recepcion_UC == null) return;

            string v = (estado ?? "").Trim().ToUpperInvariant();

            if (v.Contains("PEND"))
                Panel_Estado_Recepcion_UC.BackColor = Color.FromArgb(213, 245, 227);
            else if (v.Contains("PROCE") || v.Contains("PROC"))
                Panel_Estado_Recepcion_UC.BackColor = Color.FromArgb(252, 243, 207);
            else if (v.Contains("FINAL") || v.Contains("CERR"))
                Panel_Estado_Recepcion_UC.BackColor = Color.FromArgb(235, 237, 239);
            else
                Panel_Estado_Recepcion_UC.BackColor = Color.FromArgb(230, 230, 230);
        }
    }

    public class OrdenSeleccionadaEventArgs : EventArgs
    {
        public int OrdenServicioID { get; private set; }
        public OrdenSeleccionadaEventArgs(int id) { OrdenServicioID = id; }
    }
}
