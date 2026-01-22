using Dominio_SISV.DTOs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Clientes
{
    public partial class PnlProveedor : UserControl
    {
        public event Action<ProveedorPickVM> ProveedorSelected;

        private ProveedorPickVM _vm;
        public ProveedorPickVM VM
        {
            get => _vm;
            set
            {
                _vm = value;
                Render();
            }
        }

        public PnlProveedor()
        {
            InitializeComponent();

            // Click en toda la tarjeta
            Panel_DatProveedores.Click += (_, __) => RaiseSelected();
            lbl_ProveedorMuestra_UC.Click += (_, __) => RaiseSelected();
            lbl_DatosProveedor_UC.Click += (_, __) => RaiseSelected();
        }

        private void Render()
        {
            if (_vm == null)
            {
                lbl_ProveedorMuestra_UC.Text = "—";
                lbl_DatosProveedor_UC.Text = "";
                return;
            }


            var ruc = string.IsNullOrWhiteSpace(_vm.Ruc) ? "S/RUC" : _vm.Ruc;
            var tel = string.IsNullOrWhiteSpace(_vm.Telefono) ? "S/Teléfono" : _vm.Telefono;
            lbl_DatosProveedor_UC.Text = $"{ruc} | {tel}";
        }

        private void RaiseSelected()
        {
            if (_vm == null) return;
            ProveedorSelected?.Invoke(_vm);
        }

        // Este método lo pide el Designer (Paint event)
        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {
            // No necesitas nada aquí; solo evita el error de compilación.
        }
    }
}
