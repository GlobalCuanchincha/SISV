using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Equipos
{
    public partial class Pnl_SeleccionClientes : UserControl
    {
        public class ClienteSeleccionadoEventArgs : EventArgs
        {
            public int ClienteID { get; }
            public string NombreCompleto { get; }
            public ClienteSeleccionadoEventArgs(int id, string nombre)
            {
                ClienteID = id;
                NombreCompleto = nombre;
            }
        }

        public event EventHandler<ClienteSeleccionadoEventArgs> ClienteSeleccionado;

        public int ClienteID { get; private set; }
        public bool Activo { get; private set; }

        // Root visual para pintar selección (si existe panel en designer, lo usa; si no, usa el UserControl)
        private Control _root;

        public Pnl_SeleccionClientes()
        {
            InitializeComponent();

            // Intenta encontrar un panel contenedor por nombre (si tu diseñador lo tiene).
            // Si no existe, usa el propio UserControl.
            _root = FindControlByName(this, "Pnl_SeleccionClientes")
                 ?? FindControlByName(this, "panel1")
                 ?? FindControlByName(this, "Panel_Principal")
                 ?? this;

            // Hook click a todo el root y sus hijos (para que cualquier click seleccione)
            HookClickDeep(_root);
        }

        private void RaiseSelected()
        {
            ClienteSeleccionado?.Invoke(
                this,
                new ClienteSeleccionadoEventArgs(ClienteID, lbl_Nom_Clientes_UC?.Text ?? "")
            );
        }

        public void Bind(int clienteId, string cedula, string nombre, string correo, string telefono, bool activo)
        {
            ClienteID = clienteId;
            Activo = activo;

            if (lbl_Cedula_Clientes_UC != null) lbl_Cedula_Clientes_UC.Text = cedula ?? "";
            if (lbl_Nom_Clientes_UC != null) lbl_Nom_Clientes_UC.Text = nombre ?? "";
            if (lbl_Correo_Clientes_UC != null) lbl_Correo_Clientes_UC.Text = correo ?? "";
            if (lbl_Telefono_Clientes_UC != null) lbl_Telefono_Clientes_UC.Text = telefono ?? "";

            if (lbl_Estado_Clientes_UC != null) lbl_Estado_Clientes_UC.Text = activo ? "Activo" : "Inactivo";
            if (lbl_Point_Clientes_UC != null) lbl_Point_Clientes_UC.Text = "•";

            if (Panel_Estado_Clientes_UC != null)
            {
                Panel_Estado_Clientes_UC.BackColor = activo
                    ? Color.FromArgb(220, 248, 235)
                    : Color.FromArgb(255, 228, 228);
            }

            if (lbl_Estado_Clientes_UC != null)
            {
                lbl_Estado_Clientes_UC.ForeColor = activo
                    ? Color.FromArgb(16, 122, 70)
                    : Color.FromArgb(190, 30, 30);
            }
        }

        public void SetSelected(bool selected)
        {
            if (_root == null) _root = this;

            // Si el root es el UserControl y está en Transparent, a veces no se nota; usamos un color suave.
            _root.BackColor = selected ? Color.FromArgb(235, 245, 255) : Color.Transparent;
        }

        // =========================
        // Helpers
        // =========================
        private void HookClickDeep(Control parent)
        {
            if (parent == null) return;

            parent.Cursor = Cursors.Hand;
            parent.Click += (s, e) => RaiseSelected();

            foreach (Control child in parent.Controls)
            {
                HookClickDeep(child);
            }
        }

        private static Control FindControlByName(Control parent, string name)
        {
            if (parent == null || string.IsNullOrWhiteSpace(name)) return null;

            foreach (Control c in parent.Controls)
            {
                if (string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                    return c;

                var inner = FindControlByName(c, name);
                if (inner != null) return inner;
            }

            return null;
        }
    }
}
