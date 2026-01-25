using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dominio_SISV.DTOs;

namespace Union_Formularios_SISV.Controls.Proveedor
{
    public partial class ProveedorTaskCard : UserControl
    {
        public event EventHandler<int> ProveedorSeleccionado;

        public int ProveedorId { get; private set; }

        private bool _clickWired;

        private Label _lblRuc, _lblNombre, _lblTelefono, _lblEstado;

        public ProveedorTaskCard()
        {
            InitializeComponent();

            EnsureFallbackUI();

            WireClicksOnce();
        }

        public void Bind(int id, string ruc, string nombre, string telefono, bool? activo, string estadoTexto)
        {
            ProveedorId = id;
            this.Tag = id;

            bool esActivo = activo ?? string.Equals(estadoTexto, "Activo", StringComparison.OrdinalIgnoreCase);

            SetLabelText("lbl_RUC_Proveedor_UC", ruc);
            SetLabelText("lbl_Nombre_Proveedor_UC", nombre);
            SetLabelText("lbl_Telf_Proveedor_UC", telefono);
            SetLabelText("lbl_Estado_Proveedor_UC", esActivo ? "Activo" : "Inactivo");
            SetLabelText("lblRuc", ruc);
            SetLabelText("lblNombre", nombre);
            SetLabelText("lblTelefono", telefono);
            SetLabelText("lblEstado", esActivo ? "Activo" : "Inactivo");

            PintarEstadoUI(esActivo);

            this.Cursor = Cursors.Hand;
        }

        public void Bind(int id, ProveedorCardVM vm)
        {
            if (vm == null) return;
            Bind(id > 0 ? id : vm.ProveedorId, vm.Ruc, vm.Nombre, vm.Telefono, vm.Activo, vm.EstadoTexto);
        }

        private void PintarEstadoUI(bool esActivo)
        {
            var dot = this.Controls.Find("lbl_Point_Proveedor_UC", true).FirstOrDefault();
            if (dot != null)
                dot.ForeColor = esActivo ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);

            var panel = this.Controls.Find("Panel_Estado_Proveedor_UC", true).FirstOrDefault();
            if (panel != null)
                panel.BackColor = esActivo ? Color.FromArgb(230, 245, 240) : Color.FromArgb(250, 235, 235);
        }

        private void RaiseSelect()
        {
            if (ProveedorId <= 0) return;
            ProveedorSeleccionado?.Invoke(this, ProveedorId);
        }

        private void WireClicksOnce()
        {
            if (_clickWired) return;
            _clickWired = true;

            WireClickRecursive(this);
        }

        private void WireClickRecursive(Control c)
        {
            c.Click += (_, __) => RaiseSelect();

            foreach (Control child in c.Controls)
                WireClickRecursive(child);
        }

        private void SetLabelText(string name, string value)
        {
            // Busca un Label dentro del UserControl con ese Name (del diseñador)
            var lbl = this.Controls.Find(name, true).FirstOrDefault() as Label;
            if (lbl != null) lbl.Text = value ?? "";
        }

        private void EnsureFallbackUI()
        {
            // Si ya tienes diseñador con controles, no crear nada adicional
            if (this.Controls.Count > 0) return;

            this.Height = 48;
            this.Dock = DockStyle.Top;
            this.Padding = new Padding(10);
            this.BackColor = Color.White;

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

            _lblRuc = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Name = "lblRuc" };
            _lblNombre = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Name = "lblNombre" };
            _lblTelefono = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Name = "lblTelefono" };
            _lblEstado = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, AutoEllipsis = true, Name = "lblEstado" };

            tbl.Controls.Add(_lblRuc, 0, 0);
            tbl.Controls.Add(_lblNombre, 1, 0);
            tbl.Controls.Add(_lblTelefono, 2, 0);
            tbl.Controls.Add(_lblEstado, 3, 0);

            this.Controls.Add(tbl);
        }
    }
}
