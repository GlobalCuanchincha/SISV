using System;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Equipos
{
    public partial class EquiposTaskCard : UserControl
    {
        public class EquipoSeleccionadoEventArgs : EventArgs
        {
            public int EquipoID { get; }
            public EquipoSeleccionadoEventArgs(int id) { EquipoID = id; }
        }

        public event EventHandler<EquipoSeleccionadoEventArgs> EquipoSeleccionado;

        public int EquipoID { get; private set; }

        public EquiposTaskCard()
        {
            InitializeComponent();

            HookClick(this);
            HookClick(Panel_ProductTask);
            HookClick(lbl_CodigoInterno_Equipos_UC);
            HookClick(lbl_NombreEquipo_Equipos_UC);
            HookClick(lbl_Cliente_Equipos_UC);
            HookClick(lbl_NumSerie_Equipos_UC);
        }

        private void HookClick(Control c)
        {
            if (c == null) return;
            c.Cursor = Cursors.Hand;
            c.Click += (s, e) => RaiseSelected();
        }

        private void RaiseSelected()
        {
            EquipoSeleccionado?.Invoke(this, new EquipoSeleccionadoEventArgs(EquipoID));
        }

        public void Bind(int equipoId, string codigoInterno, string nombreEquipo, string cliente, string serie)
        {
            EquipoID = equipoId;
            lbl_CodigoInterno_Equipos_UC.Text = codigoInterno ?? "";
            lbl_NombreEquipo_Equipos_UC.Text = nombreEquipo ?? "";
            lbl_Cliente_Equipos_UC.Text = cliente ?? "";
            lbl_NumSerie_Equipos_UC.Text = serie ?? "";
        }

        public void SetSelected(bool selected)
        {
            // Si tu panel principal se llama distinto, cambia Panel_ProductTask por el tuyo
            if (Panel_ProductTask != null)
                Panel_ProductTask.BackColor = selected ? System.Drawing.Color.FromArgb(235, 245, 255) : System.Drawing.Color.Transparent;
        }
    }
}
