using System;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Clientes
{
    public partial class PnlProveedor : UserControl
    {
        public class ProveedorSeleccionadoEventArgs : EventArgs
        {
            public int ProveedorID { get; }
            public ProveedorSeleccionadoEventArgs(int id) { ProveedorID = id; }
        }

        public event EventHandler<ProveedorSeleccionadoEventArgs> ProveedorSeleccionado;

        public int ProveedorID { get; private set; }

        public PnlProveedor()
        {
            InitializeComponent();
            HookClickRecursive(this);
        }

        private void HookClickRecursive(Control root)
        {
            if (root == null) return;

            root.Cursor = Cursors.Hand;
            root.Click += (s, e) => ProveedorSeleccionado?.Invoke(this, new ProveedorSeleccionadoEventArgs(ProveedorID));

            foreach (Control c in root.Controls)
                HookClickRecursive(c);
        }

        public void Bind(int proveedorId, string nombre, string ruc, string telefono)
        {
            ProveedorID = proveedorId;

            lbl_ProveedorMuestra_UC.Text = nombre ?? "";
            lbl_DatosProveedor_UC.Text = $"RUC: {ruc ?? ""}  •  Tel: {telefono ?? ""}";
        }

        public void SetSelected(bool selected)
        {
            // para un highlight simple sin depender de Guna
            BackColor = selected ? System.Drawing.Color.FromArgb(240, 247, 255) : System.Drawing.Color.White;
        }
    }
}
