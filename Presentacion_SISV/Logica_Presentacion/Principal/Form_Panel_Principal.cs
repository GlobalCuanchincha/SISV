using Capa_Corte_Transversal.Helpers;
using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Services.Permisos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Dominio_SISV.Permisos;
using Union_Formularios_SISV.Controls;
using Union_Formularios_SISV.Controls.Usuarios.Permisos; 
using Union_Formularios_SISV.Forms;
using Union_Formularios_SISV.Forms.Proveedores;
using Union_Formularios_SISV.Forms.Ventas;
using Union_Formularios_SISV.Logica_Presentacion.Reportes;
using Union_Formularios_SISV.Logica_Presentacion.Servicio;

namespace Union_Formularios_SISV
{
    public partial class Form_Panel_Principal : Form
    {
        private LoginSession _session;
        private FormHost _host;

        public Form_Panel_Principal() : this(null) { }

        public Form_Panel_Principal(LoginSession session)
        {
            InitializeComponent();
            _session = session;
            Load += Form_Panel_Principal_Load;
            btn_CerrarSesion.Click += btn_CerrarSesion_Click;
        }

        private void Form_Panel_Principal_Load(object sender, EventArgs e)
        {
            _host = new FormHost(Panel_Escritorio, lbl_Titulo, lbl_Descripcion_Titulo);

            Nom_Usu.Text = _session?.Username ?? "Usuario";
            lbl_Cargo.Text = GetCargo(_session?.RoleId ?? (byte)0);

            CargarPermisosSesionYAplicarMenu();  

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
            var p = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            if (!PuedeVerOrdenesServicio(p))
            {
                MessageBox.Show("Acceso denegado. No tiene permisos para Órdenes de servicio.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateButton(sender, RGBColors.color3);
            AbrirPrimerSubmoduloOrdenesServicio(p);
        }

        private void btn_Clientes_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color4);
            _host.Open(new Form_Clientes(), "Clientes", "Registrar y consultar clientes");
        }

        private void btn_Proveedores_Click(object sender, EventArgs e)
        {
            var p = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            if (!p.HasAny("INV_PROVEEDORES_ACCESO", "INV_PROVEEDORES_REGISTRAR", "INV_PROVEEDORES_ACTUALIZAR", "INV_PROVEEDORES_DESACTIVAR"))
            {
                MessageBox.Show("Acceso denegado. No tiene permisos para Proveedores.", "SISV",
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
            _host.Open(new Union_Formularios_SISV.Forms.Inventario.Form_Inventario(_session),
                "Inventario", "Productos (registrar, consultar, actualizar)");
        }

        private void btn_Usuarios_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color7);
            _host.Open(new Form_Usuarios(_session), "Gestión de usuarios", "Crear • Actualizar • Desactivar");
        }

        private void btn_Configuracion_Click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color8);
            _host.Open(new Form_Servicio(), "Gestión de servicios", "Registrar • Actualizar • Consultar");
        }

        private void btn_Reportes_click(object sender, EventArgs e)
        {
            ActivateButton(sender, RGBColors.color1);
            _host.Open(new Form_Reportes(), "Reportes", "Reportes de ventas, servicios e inventario");
        }

        private Control _btnActivo;

        private void btn_CerrarSesion_Click(object sender, EventArgs e)
        {
            Session.UsuarioId = 0;
            Session.NombreUsuario = "";
            Session.Rol = "";
            Session.Permisos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            _session = null;

            this.Hide();

            LoginSession nuevaSesion = null;

            using (var login = new Form_Login())
            {
                login.LoginSucceeded += (s, sess) =>
                {
                    nuevaSesion = sess;
                    login.DialogResult = DialogResult.OK; 
                };

                var r = login.ShowDialog(this);

                if (r != DialogResult.OK || nuevaSesion == null)
                {
                    Application.Exit();   // ✅ AQUÍ VA
                    return;
                }
            }

            _session = nuevaSesion;

            Nom_Usu.Text = _session?.Username ?? "Usuario";
            lbl_Cargo.Text = GetCargo(_session?.RoleId ?? (byte)0);

            CargarPermisosSesionYAplicarMenu();
            AbrirPanelPrincipal();

            this.Show();
        }
        private void ActivateButton(object senderBtn, Color color)
        {
            if (senderBtn == null) return;

            if (_btnActivo != null)
            {
                _btnActivo.BackColor = Color.Transparent;
                _btnActivo.ForeColor = Color.FromArgb(45, 45, 45);
            }

            _btnActivo = (Control)senderBtn;
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

        private void CargarPermisosSesionYAplicarMenu()
        {
            int usuarioId = 0;

            try
            {
                // Usa tu SessionHelper para leer UsuarioID desde LoginSession
                usuarioId = SessionHelper.GetUsuarioID(_session);
            }
            catch
            {
                usuarioId = 0;
            }

            // Si no hay usuarioId, deja permisos vacíos (todo oculto excepto resumen)
            var permisosSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (usuarioId > 0)
            {
                try
                {
                    var ps = new PermisosService();
                    permisosSet = ps.GetCodigosEfectivosByUsuario(usuarioId);
                }
                catch
                {
                    permisosSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
            }

            // Guardar en sesión global (la estás usando ya como estática)
            Session.UsuarioId = usuarioId;
            Session.NombreUsuario = _session?.Username ?? "";
            Session.Rol = GetCargo(_session?.RoleId ?? (byte)0);
            Session.Permisos = permisosSet;
            AplicarPermisosMenu();
        }

        private void AplicarPermisosMenu()
        {
            var p = new PermissionContext(Session.Permisos ?? new HashSet<string>());

            // Facturación
            btn_Ventas.Visible = p.HasAny("BILL_FACTURA_CONSULTAR", "BILL_FACTURA_EMITIR", "BILL_FACTURA_ANULAR");

            // Órdenes de servicio (cualquiera de sus apartados)
            btn_Ordenes_Servicio.Visible = PuedeVerOrdenesServicio(p);

            // Clientes
            btn_Clientes.Visible = p.HasAny("CRM_CLIENTES_ACCESO", "CRM_CLIENTES_REGISTRAR", "CRM_CLIENTES_ACTUALIZAR");

            // Productos (Inventario)
            btn_Productos.Visible = p.HasAny("INV_PRODUCTOS_ACCESO", "INV_PRODUCTOS_REGISTRAR", "INV_PRODUCTOS_ACTUALIZAR", "INV_PRODUCTOS_DESACTIVAR");

            //Proveedores
            btn_Proveedores.Visible = p.HasAny("INV_PROVEEDORES_ACCESO","INV_PROVEEDORES_REGISTRAR","INV_PROVEEDORES_ACTUALIZAR","INV_PROVEEDORES_DESACTIVAR");

            // Servicios 
            btn_Servicios.Visible = p.HasAny("OPS_SERVICIOS_ACCESO", "OPS_SERVICIOS_REGISTRAR", "OPS_SERVICIOS_ACTUALIZAR", "OPS_SERVICIOS_DESACTIVAR");

            // Usuarios (gestión)
            btn_Usuarios.Visible = p.HasAny("SEC_USUARIOS_ACCESO", "SEC_USUARIOS_REGISTRAR", "SEC_USUARIOS_ACTUALIZAR", "SEC_USUARIOS_GESTIONAR_PERM");

            btn_Reportes.Visible = p.Has("REP_REPORTES_ACCESO");
        }

        private bool PuedeVerOrdenesServicio(PermissionContext p)
        {
            return PuedeAbrirEquipos(p) || PuedeAbrirRecepcion(p) || PuedeAbrirNotificacion(p);
        }

        private bool PuedeAbrirEquipos(PermissionContext p)
        {
            return p.HasAny(
                OpsPermissionCodes.EquiposAcceso,
                OpsPermissionCodes.EquiposRegistrar,
                OpsPermissionCodes.EquiposActualizar,
                OpsPermissionCodes.EquiposDesactivar
            );
        }

        private bool PuedeAbrirRecepcion(PermissionContext p)
        {
            return p.HasAny(
                OpsPermissionCodes.RecepcionAcceso,
                OpsPermissionCodes.RecepcionCrearOrden,
                OpsPermissionCodes.RecepcionAsignarTecnico,
                OpsPermissionCodes.RecepcionEditar
            );
        }

        private bool PuedeAbrirNotificacion(PermissionContext p)
        {
            return p.HasAny(
                OpsPermissionCodes.NotifAcceso,
                OpsPermissionCodes.NotifGuardarDiag,
                OpsPermissionCodes.NotifCambiarEstado,
                OpsPermissionCodes.NotifEnviarCorreo
            );
        }

        private void AbrirPrimerSubmoduloOrdenesServicio(PermissionContext p)
        {
            if (PuedeAbrirEquipos(p))
            {
                _host.Open(new Form_Ordenes_Servicio(_session),
                    "Órdenes de servicio",
                    "Ingreso de equipo, seguimiento, estados y asignación de técnico");
                return;
            }

            if (PuedeAbrirRecepcion(p))
            {
                _host.Open(new Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Recepcion(_session),
                    "Recepción",
                    "Recepción / Solicitud de orden");
                return;
            }

            if (PuedeAbrirNotificacion(p))
            {
                _host.Open(new Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Notificacion(_session),
                    "Notificación",
                    "Actualizar estado / notificaciones del servicio");
                return;
            }

            MessageBox.Show("Acceso denegado. No tiene permisos para Órdenes de servicio.", "SISV",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
