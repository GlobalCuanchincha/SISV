using Dominio_SISV.Services.Permisos;
using Dominio_SISV.Services.Usuarios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Controls.Usuarios.Permisos
{
    public partial class UC_GestionPermisos : UserControl
    {
        public event EventHandler VolverSolicitado;

        private readonly IPermisosService _permService = new PermisosService();
        private readonly IUsuarioService _usuarioService = new UsuarioService();

        private int _actorUsuarioId;
        private bool _suppressAfterCheck;

        public UC_GestionPermisos()
        {
            InitializeComponent();

            WireUi();
            AssignPermissionTags(); // mapea nodos -> códigos
        }

        public void SetActor(int actorUsuarioId)
        {
            _actorUsuarioId = actorUsuarioId;
            LoadCombosInicial();
        }

        // =========================
        // UI WIRING (SIN asumir Button vs Guna2Button)
        // =========================
        private void WireUi()
        {
            var tv = FindTree();
            if (tv != null)
            {
                tv.AfterCheck -= Tv_AfterCheck;
                tv.AfterCheck += Tv_AfterCheck;
            }

            var btnAll = FindControlByName("btn_MarcarTodo") ?? FindFirstControlByTextContains("Marcar todo");
            if (btnAll != null) btnAll.Click += (s, e) => SetAllNodesChecked(true);

            var btnNone = FindControlByName("btn_LimpiarTodo") ?? FindFirstControlByTextContains("Limpiar todo");
            if (btnNone != null) btnNone.Click += (s, e) => SetAllNodesChecked(false);

            var btnSave = FindControlByName("btn_GuardarCambios") ?? FindFirstControlByTextContains("Guardar cambios");
            if (btnSave != null) btnSave.Click += (s, e) => Guardar();

            // ✅ SOLO por Name (sin fallback)
            var cmbAplicarA = FindComboStrict("cmb_AplicarA");
            var cmbSel = FindComboStrict("cmb_Seleccionar");

            if (cmbAplicarA != null)
                cmbAplicarA.SelectedIndexChanged += (s, e) => LoadSeleccionList();

            if (cmbSel != null)
                cmbSel.SelectedIndexChanged += (s, e) => CargarPermisosSeleccion();

            // Botón volver
            var btnVolver = FindControlByName("btn_VolverPermisos") ?? FindFirstControlByTextContains("Volver");
            if (btnVolver != null)
            {
                btnVolver.Click -= btn_VolverPermisos_Click;
                btnVolver.Click += btn_VolverPermisos_Click;
            }
        }
        private void btn_VolverPermisos_Click(object sender, EventArgs e)
        {
            VolverSolicitado?.Invoke(this, EventArgs.Empty);
        }

        private void BtnMarcarTodo_Click(object sender, EventArgs e) => SetAllNodesChecked(true);
        private void BtnLimpiarTodo_Click(object sender, EventArgs e) => SetAllNodesChecked(false);
        private void BtnGuardar_Click(object sender, EventArgs e) => Guardar();

        private void CmbAplicarA_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSeleccionList();
        }

        private void CmbSeleccionar_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarPermisosSeleccion();
        }

        // =========================
        // 1) CARGA INICIAL
        // =========================
        private void LoadCombosInicial()
        {
            try { _permService.SeedCatalogo(); } catch { }

            var cmbAplicarA = FindComboStrict("cmb_AplicarA");
            if (cmbAplicarA == null) return;

            cmbAplicarA.DataSource = null;
            cmbAplicarA.Items.Clear();
            cmbAplicarA.Items.Add("Usuario");
            cmbAplicarA.Items.Add("Rol");
            cmbAplicarA.SelectedIndex = 0; 

            LoadSeleccionList();
        }
        private void LoadSeleccionList()
        {
            var cmbAplicarA = FindComboStrict("cmb_AplicarA");
            var cmbSel = FindComboStrict("cmb_Seleccionar");
            if (cmbAplicarA == null || cmbSel == null) return;

            bool esRol = (cmbAplicarA.SelectedIndex == 1); 

            cmbSel.DataSource = null;

            if (esRol)
            {
                var dt = _usuarioService.ListarRolesAsync(true).GetAwaiter().GetResult();

                BindCombo(
                    cmbSel,
                    dt,
                    preferId: new[] { "RoleID_Roles", "RolID_Roles", "RolID", "RoleID", "IdRol", "ID" },
                    preferText: new[] { "NombreRol_Roles", "RolNombre", "RoleName", "NombreRol", "Nombre", "Name" }
                );
            }
            else
            {
                // Usuarios
                var dt = _permService.ListarUsuariosBasico();
                cmbSel.DisplayMember = "Texto";
                cmbSel.ValueMember = "UsuarioID";
                cmbSel.DataSource = dt;
            }

            // Cargar permisos del seleccionado (si existe)
            CargarPermisosSeleccion();
        }


        // =========================
        // 2) CARGAR PERMISOS EN TREE
        // =========================
        private void CargarPermisosSeleccion()
        {
            var cmbAplicarA = FindCombo("cmb_AplicarA") ?? FindCombos().ElementAtOrDefault(0);
            var cmbSel = FindCombo("cmb_Seleccionar") ?? FindCombos().ElementAtOrDefault(1);
            var tv = FindTree();
            if (cmbAplicarA == null || cmbSel == null || tv == null) return;
            if (cmbSel.SelectedValue == null) return;

            int id = 0;
            int.TryParse(Convert.ToString(cmbSel.SelectedValue), out id);
            if (id <= 0) return;

            bool esRol = (cmbAplicarA.SelectedIndex == 1);

            HashSet<string> allowed =
                esRol
                ? _permService.GetCodigosByRol(id)
                : _permService.GetCodigosByUsuario(id);
            
            _suppressAfterCheck = true;
            try
            {
                foreach (TreeNode n in tv.Nodes)
                    SetNodeCheckedRecursive(n, false);

                foreach (TreeNode n in tv.Nodes)
                    MarkByTagRecursive(n, allowed);

            }
            finally { _suppressAfterCheck = false; }
        }

        private void MarkByTagRecursive(TreeNode node, HashSet<string> allowed)
        {
            if (node == null) return;

            var code = node.Tag as string;
            if (!string.IsNullOrWhiteSpace(code) && allowed.Contains(code))
                node.Checked = true;

            foreach (TreeNode c in node.Nodes)
                MarkByTagRecursive(c, allowed);

            // padre marcado si cualquier hijo marcado
            if (node.Nodes.Count > 0)
                node.Checked = node.Nodes.Cast<TreeNode>().Any(x => x.Checked);
        }

        // =========================
        // 3) GUARDAR
        // =========================
        private void Guardar()
        {
            if (_actorUsuarioId <= 0)
            {
                MessageBox.Show("No se pudo obtener UsuarioID de sesión (actor).", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var cmbAplicarA = FindCombo("cmb_AplicarA") ?? FindCombos().ElementAtOrDefault(0);
            var cmbSel = FindCombo("cmb_Seleccionar") ?? FindCombos().ElementAtOrDefault(1);
            var tv = FindTree();
            if (cmbAplicarA == null || cmbSel == null || tv == null) return;

            if (cmbSel.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un usuario o rol.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int targetId = 0;
            int.TryParse(Convert.ToString(cmbSel.SelectedValue), out targetId);
            if (targetId <= 0) return;

            var codigos = new List<string>();
            foreach (TreeNode n in tv.Nodes)
                CollectCheckedTags(n, codigos);

            string modo = (cmbAplicarA.Text ?? "Usuario").Trim().ToLowerInvariant();

            try
            {
                if (cmbAplicarA.SelectedIndex == 1)
                    _permService.SaveByRol(_actorUsuarioId, targetId, codigos);
                else
                    _permService.SaveByUsuario(_actorUsuarioId, targetId, codigos);

                MessageBox.Show("Permisos guardados.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectCheckedTags(TreeNode node, List<string> outList)
        {
            if (node == null) return;

            var code = node.Tag as string;
            if (!string.IsNullOrWhiteSpace(code) && node.Checked)
                outList.Add(code);

            foreach (TreeNode c in node.Nodes)
                CollectCheckedTags(c, outList);
        }

        // =========================
        // 4) CHECK PADRE -> HIJOS
        // =========================
        private void Tv_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_suppressAfterCheck) return;

            _suppressAfterCheck = true;
            try
            {
                foreach (TreeNode child in e.Node.Nodes)
                    SetNodeCheckedRecursive(child, e.Node.Checked);

                UpdateParentChecks(e.Node);
            }
            finally { _suppressAfterCheck = false; }
        }

        private void UpdateParentChecks(TreeNode node)
        {
            if (node == null || node.Parent == null) return;

            var p = node.Parent;
            p.Checked = p.Nodes.Cast<TreeNode>().Any(n => n.Checked);
            UpdateParentChecks(p);
        }

        private void SetNodeCheckedRecursive(TreeNode node, bool checkedState)
        {
            node.Checked = checkedState;
            foreach (TreeNode c in node.Nodes)
                SetNodeCheckedRecursive(c, checkedState);
        }

        private void SetAllNodesChecked(bool state)
        {
            var tv = FindTree();
            if (tv == null) return;

            _suppressAfterCheck = true;
            try
            {
                foreach (TreeNode n in tv.Nodes)
                    SetNodeCheckedRecursive(n, state);
            }
            finally { _suppressAfterCheck = false; }
        }

        // =========================
        // 5) MAPEO NODO -> CÓDIGO
        // =========================
        private void AssignPermissionTags()
        {
            var tv = FindTree();
            if (tv == null) return;

            // Facturación
            SetTag(tv, "Facturación", "BILL_FACTURA_ACCESO");
            SetTag(tv, "Facturación\\Consultar factura", "BILL_FACTURA_CONSULTAR", fallbackText: "Consultar factura");
            SetTag(tv, "Facturación\\Emitir factura", "BILL_FACTURA_EMITIR", fallbackText: "Emitir factura");
            SetTag(tv, "Facturación\\Anular factura", "BILL_FACTURA_ANULAR", fallbackText: "Anular factura");

            // Órdenes de servicio - Equipos
            SetTag(tv, "Órdenes de servicio\\Equipos\\Acceso a Equipos", "OPS_EQUIPOS_ACCESO", "Acceso a Equipos");
            SetTag(tv, "Órdenes de servicio\\Equipos\\Registrar equipo", "OPS_EQUIPOS_REGISTRAR", "Registrar equipo");
            SetTag(tv, "Órdenes de servicio\\Equipos\\Actualizar equipo", "OPS_EQUIPOS_ACTUALIZAR", "Actualizar equipo");
            SetTag(tv, "Órdenes de servicio\\Equipos\\Desactivar equipo", "OPS_EQUIPOS_DESACTIVAR", "Desactivar equipo");

            // Órdenes de servicio - Recepción
            SetTag(tv, "Órdenes de servicio\\Recepción / Solicitud\\Acceso a Recepción", "OPS_RECEPCION_ACCESO", "Acceso a Recepción");
            SetTag(tv, "Órdenes de servicio\\Recepción / Solicitud\\Crear orden", "OPS_RECEPCION_CREAR_ORDEN", "Crear orden");
            SetTag(tv, "Órdenes de servicio\\Recepción / Solicitud\\Asignar técnico", "OPS_RECEPCION_ASIGNAR_TECNICO", "Asignar técnico");
            SetTag(tv, "Órdenes de servicio\\Recepción / Solicitud\\Editar solicitud", "OPS_RECEPCION_EDITAR", "Editar solicitud");

            // Órdenes de servicio - Notificación
            SetTag(tv, "Órdenes de servicio\\Notificación\\Acceso a Notificación", "OPS_NOTIF_ACCESO", "Acceso a Notificación");
            SetTag(tv, "Órdenes de servicio\\Notificación\\Guardar diagnóstico", "OPS_NOTIF_GUARDAR_DIAG", "Guardar diagnóstico");
            SetTag(tv, "Órdenes de servicio\\Notificación\\Cambiar estado", "OPS_NOTIF_CAMBIAR_ESTADO", "Cambiar estado");
            SetTag(tv, "Órdenes de servicio\\Notificación\\Enviar correo", "OPS_NOTIF_ENVIAR_CORREO", "Enviar correo");

            // Productos
            SetTag(tv, "Productos\\Acceder a productos", "INV_PRODUCTOS_ACCESO", "Acceder a productos");
            SetTag(tv, "Productos\\Registrar productos", "INV_PRODUCTOS_REGISTRAR", "Registrar productos");
            SetTag(tv, "Productos\\Actualizar productos", "INV_PRODUCTOS_ACTUALIZAR", "Actualizar productos");
            SetTag(tv, "Productos\\Desactivar productos", "INV_PRODUCTOS_DESACTIVAR", "Desactivar productos");

            // Servicios
            SetTag(tv, "Servicios\\Acceder a servicios", "OPS_SERVICIOS_ACCESO", "Acceder a servicios");
            SetTag(tv, "Servicios\\Registrar un servicio", "OPS_SERVICIOS_REGISTRAR", "Registrar un servicio");
            SetTag(tv, "Servicios\\Actualizar un servicio", "OPS_SERVICIOS_ACTUALIZAR", "Actualizar un servicio");
            SetTag(tv, "Servicios\\Desactivar un servicio", "OPS_SERVICIOS_DESACTIVAR", "Desactivar un servicio");

            // Proveedores
            SetTag(tv, "Proveedores\\Acceder a Proveedores", "INV_PROVEEDORES_ACCESO", "Acceder a Proveedores");
            SetTag(tv, "Proveedores\\Registrar un proveedor", "INV_PROVEEDORES_REGISTRAR", "Registrar un proveedor");
            SetTag(tv, "Proveedores\\Actualizar un proveedor", "INV_PROVEEDORES_ACTUALIZAR", "Actualizar un proveedor");
            SetTag(tv, "Proveedores\\Desactivar un proveedor", "INV_PROVEEDORES_DESACTIVAR", "Desactivar un proveedor");

            // Usuarios
            SetTag(tv, "Usuarios\\Acceder a usuarios", "SEC_USUARIOS_ACCESO", "Acceder a usuarios");
            SetTag(tv, "Usuarios\\Registrar un usuario", "SEC_USUARIOS_REGISTRAR", "Registrar un usuario");
            SetTag(tv, "Usuarios\\Actualizar un usuario", "SEC_USUARIOS_ACTUALIZAR", "Actualizar un usuario");
            SetTag(tv, "Usuarios\\Gestionar permisos de usuario", "SEC_USUARIOS_GESTIONAR_PERM", "Gestionar permisos de usuario");

            // Clientes
            SetTag(tv, "Clientes\\Acceder a clientes", "CRM_CLIENTES_ACCESO", "Acceder a clientes");
            SetTag(tv, "Clientes\\Registrar un cliente", "CRM_CLIENTES_REGISTRAR", "Registrar un cliente");
            SetTag(tv, "Clientes\\Actualizar un cliente", "CRM_CLIENTES_ACTUALIZAR", "Actualizar un cliente");

            //Reportes
            SetTag(tv, "Reportes", "REP_REPORTES_ACCESO", "Acceder a reportes");
        }

        private void SetTag(TreeView tv, string fullPath, string tag, string fallbackText = null)
        {
            var node = FindNodeByFullPath(tv, fullPath);

            if (node == null && !string.IsNullOrWhiteSpace(fallbackText))
                node = FindFirstNodeByText(tv, fallbackText);

            if (node != null) node.Tag = tag;
        }

        private TreeNode FindNodeByFullPath(TreeView tv, string path)
        {
            foreach (TreeNode n in tv.Nodes)
            {
                var found = FindNodeRecursive(n, path);
                if (found != null) return found;
            }
            return null;
        }

        private TreeNode FindNodeRecursive(TreeNode node, string path)
        {
            if (node == null) return null;

            if (string.Equals(node.FullPath, path, StringComparison.OrdinalIgnoreCase))
                return node;

            foreach (TreeNode c in node.Nodes)
            {
                var f = FindNodeRecursive(c, path);
                if (f != null) return f;
            }
            return null;
        }

        private TreeNode FindFirstNodeByText(TreeView tv, string text)
        {
            foreach (TreeNode n in tv.Nodes)
            {
                var found = FindFirstNodeByTextRecursive(n, text);
                if (found != null) return found;
            }
            return null;
        }

        private TreeNode FindFirstNodeByTextRecursive(TreeNode node, string text)
        {
            if (node == null) return null;

            if (string.Equals(node.Text ?? "", text ?? "", StringComparison.OrdinalIgnoreCase))
                return node;

            foreach (TreeNode c in node.Nodes)
            {
                var f = FindFirstNodeByTextRecursive(c, text);
                if (f != null) return f;
            }
            return null;
        }

        // =========================
        // Helpers robustos (SIN Controls.Find("", true))
        // =========================
        private TreeView FindTree()
            => this.Controls.Find("tvPermisos", true).OfType<TreeView>().FirstOrDefault()
            ?? GetAllControls(this).OfType<TreeView>().FirstOrDefault();

        private ComboBox FindCombo(string name)
            => string.IsNullOrWhiteSpace(name) ? null
            : this.Controls.Find(name, true).OfType<ComboBox>().FirstOrDefault();

        private IEnumerable<ComboBox> FindCombos()
            => GetAllControls(this).OfType<ComboBox>().Distinct();

        private Control FindControlByName(string name)
            => string.IsNullOrWhiteSpace(name) ? null
            : this.Controls.Find(name, true).OfType<Control>().FirstOrDefault();

        private Control FindFirstControlByTextContains(string contains)
        {
            if (string.IsNullOrWhiteSpace(contains)) return null;

            return GetAllControls(this)
                .FirstOrDefault(c =>
                    c != null &&
                    !string.IsNullOrWhiteSpace(c.Text) &&
                    c.Text.IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static IEnumerable<Control> GetAllControls(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;

                foreach (var child in GetAllControls(c))
                    yield return child;
            }
        }

        private void BindCombo(ComboBox cmb, DataTable dt, string[] preferId, string[] preferText)
        {
            if (cmb == null || dt == null || dt.Columns.Count == 0) return;

            string idCol = dt.Columns[0].ColumnName;
            foreach (var c in preferId)
                if (dt.Columns.Contains(c)) { idCol = c; break; }

            string txtCol = dt.Columns.Count > 1 ? dt.Columns[1].ColumnName : dt.Columns[0].ColumnName;
            foreach (var c in preferText)
                if (dt.Columns.Contains(c)) { txtCol = c; break; }

            cmb.DisplayMember = txtCol;
            cmb.ValueMember = idCol;
            cmb.DataSource = dt;
        }

        private ComboBox FindComboStrict(string name)
        {
            // Busca por Name en todo el árbol
            var ctl = this.Controls.Find(name, true).FirstOrDefault();
            var cmb = ctl as ComboBox;

            if (cmb == null)
            {
                MessageBox.Show(
                    $"No encontré el ComboBox '{name}'. Revisa que la propiedad Name sea EXACTAMENTE '{name}'.",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            return cmb;
        }
    }
}