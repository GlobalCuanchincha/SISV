using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Servicios
{
    public partial class ServicioTaskCard : UserControl
    {
        public int ServicioID { get; private set; }

        public event EventHandler CardClicked;

        public ServicioTaskCard()
        {
            InitializeComponent();

            // Hacer clic en cualquier parte de la tarjeta
            WireClick(this);
        }

        private void WireClick(Control root)
        {
            root.Click += (_, __) => CardClicked?.Invoke(this, EventArgs.Empty);
            foreach (Control c in root.Controls)
                WireClick(c);
        }

        public void Bind(DataRow r)
        {
            if (r == null) return;

            ServicioID = SafeInt(r, "ServicioID");

            // Labels (usa exactamente los nombres que me diste)
            if (lbl_Codigo_Servicio_USC != null) lbl_Codigo_Servicio_USC.Text = SafeString(r, "Codigo");
            if (lbl_Nom_Componente_Servicio_USC != null) lbl_Nom_Componente_Servicio_USC.Text = SafeString(r, "Nombre");
            if (lbl_Categoria_Servicio_USC != null) lbl_Categoria_Servicio_USC.Text = SafeString(r, "Categoria");

            decimal precio = SafeDecimal(r, "Precio");
            if (lbl_Precio_Servicio != null) lbl_Precio_Servicio.Text = precio.ToString("0.00");

            bool activo = SafeBool(r, "Activo");
            if (lbl_Estado_USC != null) lbl_Estado_USC.Text = activo ? "Activo" : "Inactivo";

            if (lbl_Point_USC != null) lbl_Point_USC.Text = "●";

            if (Panel_Estado_Servicio_USC != null)
                Panel_Estado_Servicio_USC.BackColor = activo ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);
        }

        public void SetSelected(bool selected)
        {
            this.BackColor = selected ? Color.FromArgb(235, 245, 255) : Color.White;
            this.BorderStyle = selected ? BorderStyle.FixedSingle : BorderStyle.None;
        }

        private static int SafeInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) ? x : 0;
        }

        private static decimal SafeDecimal(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0m;
            decimal d; return decimal.TryParse(Convert.ToString(r[col]), out d) ? d : 0m;
        }

        private static bool SafeBool(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return false;
            bool b;
            if (bool.TryParse(Convert.ToString(r[col]), out b)) return b;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) && x != 0;
        }

        private static string SafeString(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return "";
            return Convert.ToString(r[col]) ?? "";
        }
    }
}