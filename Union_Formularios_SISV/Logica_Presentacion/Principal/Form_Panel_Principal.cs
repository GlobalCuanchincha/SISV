using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls;
using Union_Formularios_SISV.Forms;
using Union_Formularios_SISV.Forms.Proveedores;
using Union_Formularios_SISV.Forms.Ventas;

namespace Union_Formularios_SISV
{
    public partial class Form_Panel_Principal : Form
    {
        private readonly LoginSession _session;
        private FormHost _host;

        public Form_Panel_Principal() : this(null) { }

        public Form_Panel_Principal(LoginSession session)
        {
            InitializeComponent();
            _session = session;
            Load += Form_Panel_Principal_Load;
        }

        private void Form_Panel_Principal_Load(object sender, EventArgs e)
        {
            _host = new FormHost(Panel_Escritorio, lbl_Titulo, lbl_Descripcion_Titulo);

            Nom_Usu.Text = _session?.Username ?? "Usuario";
            lbl_Cargo.Text = GetCargo(_session?.RoleId ?? (byte)0);

            AbrirPanelPrincipal();
        }

        public void OpenChild(Form form, string titulo, string descripcion)
        {
            if (form == null) return;

            if (_host == null)
                _host = new FormHost(Panel_Escritorio, lbl_Titulo, lbl_Descripcion_Titulo);

            _host.Open(form, titulo, descripcion);
        }

        private void AbrirPanelPrincipal()
        {
            _host.Open(new Form_Resumen(), "Panel principal", "Resumen rápido de ventas, servicios e inventario");
            ActivateButton(btn_Resumen, RGBColors.color1);
        }

        private void btn_Resumen_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color1);
            AbrirPanelPrincipal();
        }

        private void btn_Ventas_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color2);

            try
            {
                var ventas = new Form_Facturacion();

                InvokeIfExists(ventas, "Ventas_RuntimeInit");

                _host.Open(ventas, "Facturación", "Emitir factura");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Ordenes_Servicio_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color3);
            _host.Open(new Form_Ordenes_Servicio(), "Órdenes de servicio",
                "Ingreso de equipo, seguimiento, estados y asignación de técnico");
        }

        private void btn_Clientes_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);
            _host.Open(new Form_Clientes(), "Clientes", "Registrar y consultar clientes");
        }

        private void btn_Proveedores_Click(object sender, EventArgs e)
        {
            var role = _session?.RoleId ?? (byte)0;
            if (role != 1 && role != 2)
            {
                MessageBox.Show("Acceso denegado. Solo Administrador puede ver Proveedores.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateButton(sender, RGBColors.color5);
            _host.Open(new Form_Proveedores(_session), "Proveedores",
                "Catálogo de proveedores (registrar, consultar, actualizar, desactivar)");
        }

        private void btn_Productos_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color6);
        }

        private void btn_Usuarios_Click(object sender, EventArgs e)
        {
            var role = _session?.RoleId ?? (byte)0;
            if (role != 1 && role != 2)
            {
                MessageBox.Show("Acceso denegado. Solo Administrador puede gestionar Usuarios.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateButton(sender, RGBColors.color7);
            _host.Open(new Form_Usuarios(_session), "Gestion de usuarios", "Crear • Actualizar • Desactivar");
        }

        private void btn_Configuracion_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color8);
            _host.Open(new Form_Config(), "Configuracion", "Configuración de la aplicación");
        }

        private Control _btnActivo;

        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn == null) return;

            if (_btnActivo != null)
            {
                _btnActivo.BackColor = Color.Transparent;
                _btnActivo.ForeColor = Color.FromArgb(45, 45, 45);
            }

            _btnActivo = (Control)senderBtn;
            _btnActivo.BackColor = Color.Transparent; // si quieres highlight real, quita esta línea
            _btnActivo.ForeColor = color;
        }

        private static void InvokeIfExists(object target, string methodName)
        {
            if (target == null) return;

            var mi = target.GetType().GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (mi != null && mi.GetParameters().Length == 0)
                mi.Invoke(target, null);
        }

        private static class RGBColors
        {
            public static Color color1 = Color.FromArgb(30, 90, 180);
            public static Color color2 = Color.FromArgb(14, 165, 233);
            public static Color color3 = Color.FromArgb(28, 188, 135);
            public static Color color4 = Color.FromArgb(243, 140, 16);
            public static Color color5 = Color.FromArgb(160, 97, 55);
            public static Color color6 = Color.FromArgb(255, 45, 77);
            public static Color color7 = Color.FromArgb(29, 150, 226);
            public static Color color8 = Color.FromArgb(110, 57, 152);
        }

        private string GetCargo(byte roleId)
        {
            switch (roleId)
            {
                case 1: return "SuperAdministrador";
                case 2: return "Administrador";
                case 3: return "Cajero";
                case 4: return "Técnico";
                default: return "Sin rol";
            }
        }
    }
}
