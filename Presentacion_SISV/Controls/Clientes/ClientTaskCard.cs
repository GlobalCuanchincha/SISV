using Dominio_SISV.DTOs.Clientes;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms.Clientes
{
    public partial class ClientTaskCard : UserControl
    {
        private ClienteCardVM _vm;
        private bool _isSelected;
        private bool _isHover;

        public event EventHandler<ClienteCardSelectedEventArgs> ClientSelected;

        private static readonly Color BorderNormal = Color.FromArgb(230, 232, 239);
        private static readonly Color BorderHover = Color.FromArgb(210, 218, 235);
        private static readonly Color BorderSelect = Color.FromArgb(37, 99, 235);

        private static readonly Color FillNormal1 = Color.White;
        private static readonly Color FillNormal2 = Color.White;

        private static readonly Color FillHover1 = Color.FromArgb(248, 250, 255);
        private static readonly Color FillHover2 = Color.FromArgb(248, 250, 255);

        private static readonly Color FillSelect1 = Color.FromArgb(240, 246, 255);
        private static readonly Color FillSelect2 = Color.FromArgb(240, 246, 255);

        private static readonly Color ChipOn1 = Color.FromArgb(232, 250, 244);
        private static readonly Color ChipOn2 = Color.FromArgb(244, 253, 249);
        private static readonly Color ChipOnBorder = Color.FromArgb(190, 238, 220);
        private static readonly Color ChipOnDot = Color.FromArgb(16, 185, 129);
        private static readonly Color ChipOnText = Color.FromArgb(15, 118, 110);

        private static readonly Color ChipOff1 = Color.FromArgb(255, 235, 235);
        private static readonly Color ChipOff2 = Color.FromArgb(255, 245, 245);
        private static readonly Color ChipOffBorder = Color.FromArgb(255, 199, 199);
        private static readonly Color ChipOffDot = Color.FromArgb(239, 68, 68);
        private static readonly Color ChipOffText = Color.FromArgb(153, 27, 27);

        public ClientTaskCard()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            ApplyCardBaseStyle();
            ApplyChipBaseStyle();

            lbl_Cedula_UserControl.Font = new Font("Consolas", 9.5f, FontStyle.Regular);
            lbl_Telefono_UserControl.Font = new Font("Consolas", 9.5f, FontStyle.Regular);
            lbl_Nom_UserControl.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            lbl_Correo_UserControl.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            lbl_Correo_UserControl.AutoEllipsis = true;

            lbl_Point_UserControl.Text = "●";
            lbl_Point_UserControl.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lbl_Estado_UserControl.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            WireClickRecursive(this);
            WireHover(Panel_Carta_UserControl);

            SetSelected(false);
        }

        public string Cedula { get { return _vm != null ? _vm.Cedula : null; } }

        public void Bind(ClienteCardVM vm)
        {
            if (vm == null) throw new ArgumentNullException("vm");
            _vm = vm;

            lbl_Cedula_UserControl.Text = vm.Cedula ?? "";
            lbl_Nom_UserControl.Text = vm.Cliente ?? "";
            lbl_Correo_UserControl.Text = string.IsNullOrWhiteSpace(vm.Correo) ? "-" : vm.Correo;
            lbl_Telefono_UserControl.Text = string.IsNullOrWhiteSpace(vm.Telefono) ? "-" : vm.Telefono;

            bool activo = vm.EsActivo == true ||
                          ((vm.EstadoNombre ?? "").ToLower().Contains("activo") &&
                           !(vm.EstadoNombre ?? "").ToLower().Contains("inactivo"));

            lbl_Estado_UserControl.Text = activo ? "Activo" : "Inactivo";

            ApplyChipState(activo);
            ApplyCardVisualState();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            ApplyCardVisualState();
        }

        private void ApplyCardBaseStyle()
        {
            if (Panel_Carta_UserControl == null) return;

            Panel_Carta_UserControl.BackColor = Color.Transparent;
            Panel_Carta_UserControl.Padding = new Padding(14, 8, 14, 8);

            SetIfExists(Panel_Carta_UserControl, "BorderRadius", 14);
            SetIfExists(Panel_Carta_UserControl, "BorderThickness", 1);
            SetIfExists(Panel_Carta_UserControl, "BorderColor", BorderNormal);

            SetFill(Panel_Carta_UserControl, FillNormal1, FillNormal2);

            ApplyShadowIfExists(Panel_Carta_UserControl, true, 12,
                new Padding(0, 0, 0, 3),
                Color.FromArgb(20, 17, 24, 39));
        }

        private void ApplyChipBaseStyle()
        {
            if (Panel_Estado_UserControl == null) return;

            Panel_Estado_UserControl.BackColor = Color.Transparent;
            Panel_Estado_UserControl.Padding = new Padding(10, 6, 10, 6);

            SetIfExists(Panel_Estado_UserControl, "BorderRadius", 999);
            SetIfExists(Panel_Estado_UserControl, "BorderThickness", 1);

            SetIfExists(Panel_Estado_UserControl, "GradientMode",
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal);

            Panel_Estado_UserControl.Height = 30;
            Panel_Estado_UserControl.Width = 120;
        }

        private void ApplyCardVisualState()
        {
            if (Panel_Carta_UserControl == null) return;

            if (_isSelected)
            {
                SetIfExists(Panel_Carta_UserControl, "BorderColor", BorderSelect);
                SetFill(Panel_Carta_UserControl, FillSelect1, FillSelect2);
            }
            else if (_isHover)
            {
                SetIfExists(Panel_Carta_UserControl, "BorderColor", BorderHover);
                SetFill(Panel_Carta_UserControl, FillHover1, FillHover2);
            }
            else
            {
                SetIfExists(Panel_Carta_UserControl, "BorderColor", BorderNormal);
                SetFill(Panel_Carta_UserControl, FillNormal1, FillNormal2);
            }
        }

        private void ApplyChipState(bool activo)
        {
            if (Panel_Estado_UserControl == null) return;

            if (activo)
            {
                SetFill(Panel_Estado_UserControl, ChipOn1, ChipOn2);
                SetIfExists(Panel_Estado_UserControl, "BorderColor", ChipOnBorder);

                lbl_Point_UserControl.ForeColor = ChipOnDot;
                lbl_Estado_UserControl.ForeColor = ChipOnText;
            }
            else
            {
                SetFill(Panel_Estado_UserControl, ChipOff1, ChipOff2);
                SetIfExists(Panel_Estado_UserControl, "BorderColor", ChipOffBorder);

                lbl_Point_UserControl.ForeColor = ChipOffDot;
                lbl_Estado_UserControl.ForeColor = ChipOffText;
            }
        }

        private static void SetFill(Control ctrl, Color fill1, Color fill2)
        {
            SetIfExists(ctrl, "FillColor", fill1);
            SetIfExists(ctrl, "FillColor2", fill2);
        }

        private static void ApplyShadowIfExists(Control ctrl, bool enabled, int depth, Padding shadowPadding, Color shadowColor)
        {
            var sd = GetProp(ctrl, "ShadowDecoration");
            if (sd == null) return;

            SetIfExists(sd, "Enabled", enabled);
            SetIfExists(sd, "Depth", depth);
            SetIfExists(sd, "Shadow", shadowPadding);
            SetIfExists(sd, "Color", shadowColor);
        }

        private void WireHover(Control root)
        {
            root.MouseEnter += delegate { _isHover = true; if (!_isSelected) ApplyCardVisualState(); };
            root.MouseLeave += delegate { _isHover = false; if (!_isSelected) ApplyCardVisualState(); };

            foreach (Control c in root.Controls) WireHover(c);
        }

        private void WireClickRecursive(Control root)
        {
            root.Click += delegate { RaiseSelected(); };
            foreach (Control c in root.Controls) WireClickRecursive(c);
        }

        private void RaiseSelected()
        {
            if (_vm == null) return;
            var handler = ClientSelected;
            if (handler != null) handler(this, new ClienteCardSelectedEventArgs(_vm));
        }

        private static object GetProp(object obj, string name)
        {
            if (obj == null) return null;
            var p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            return p != null && p.CanRead ? p.GetValue(obj, null) : null;
        }

        private static void SetIfExists(object obj, string name, object value)
        {
            if (obj == null) return;

            var p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (p == null || !p.CanWrite) return;

            try
            {
                if (value != null && !p.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    if (p.PropertyType.IsEnum && !(value is Enum))
                        value = Enum.ToObject(p.PropertyType, value);
                    else
                        value = Convert.ChangeType(value, p.PropertyType);
                }
                p.SetValue(obj, value, null);
            }
            catch { }
        }
    }
}