using Capa_Corte_Transversal.Helpers;
using Dominio_SISV.Services.Usuarios;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Usuarios;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;
using Union_Formularios_SISV.Logica_Presentacion.Administracion;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Usuarios : Form, IUsuariosView
    {
        private readonly object _session;
        private readonly UsuariosPresenter _presenter;

        private Timer _debounceTimer;

        private byte[] _fotoPendienteBytes;
        private string _fotoPendienteNombre;

        private UC_GestionPermisos _ucPermisos;

        public Form_Usuarios() : this(null) { }

        public Form_Usuarios(object session)
        {
            InitializeComponent();
            _session = session;
            _presenter = new UsuariosPresenter(this);

            InitPermisosOverlay();
            CerrarPermisos();

            _debounceTimer = new Timer();
            _debounceTimer.Interval = 350;
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop();
                await _presenter.BuscarAsync();
            };

            WireEvents();
            ApplyFormStyle();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await _presenter.InitializeAsync();
        }

        public int UsuarioSesionId
        {
            get
            {
                try { return SessionHelper.GetUsuarioID(_session); }
                catch { return 0; }
            }
        }

        public string TextoBusqueda => txt_Buscador_Usuarios.Text?.Trim() ?? "";
        public string FiltroTexto => cmbox_Filtro_Usuarios.Text ?? "Usuario";
        public string RolFiltroTexto => cmbox_RolFiltro_Usuarios.Text ?? "Todos";
        public string EstadoFiltroTexto => cmbox_EstadoFiltro_Usuarios.Text ?? "Todos";

        private void ApplyFormStyle()
        {
            BackColor = Color.FromArgb(246, 247, 251);

            flowUsuarios.BackColor = Color.Transparent;
            flowUsuarios.AutoScroll = true;
            flowUsuarios.FlowDirection = FlowDirection.TopDown;
            flowUsuarios.WrapContents = false;
        }

        private void WireEvents()
        {
            txt_Buscador_Usuarios.TextChanged += (s, e) => RestartDebounce();
            cmbox_Filtro_Usuarios.SelectedIndexChanged += (s, e) => RestartDebounce();
            cmbox_RolFiltro_Usuarios.SelectedIndexChanged += (s, e) => RestartDebounce();
            cmbox_EstadoFiltro_Usuarios.SelectedIndexChanged += (s, e) => RestartDebounce();

            btn_Registrar_Usuarios.Click += async (s, e) => { await _presenter.GuardarAsync(); };
            btn_Limpiar_Usuarios.Click += (s, e) => _presenter.Limpiar();

            btn_SubirFoto_Usuarios.Click += (s, e) => SeleccionarFoto();

            flowUsuarios.SizeChanged += (s, e) =>
            {
                foreach (var c in flowUsuarios.Controls.OfType<UsuariosTaskCard>())
                    c.Width = flowUsuarios.ClientSize.Width - 22;
            };

            if (btn_GestionarPermisos_Usuarios != null)
            {
                btn_GestionarPermisos_Usuarios.Click -= btn_GestionarPermisos_Usuarios_Click;
                btn_GestionarPermisos_Usuarios.Click += btn_GestionarPermisos_Usuarios_Click;
            }
        }

        private void RestartDebounce()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        public void EnsureFiltroCombo()
        {
            if (cmbox_Filtro_Usuarios.Items.Count == 0)
            {
                cmbox_Filtro_Usuarios.Items.AddRange(new object[]
                {
                    "Usuario",
                    "Nombres",
                    "Correo"
                });
                cmbox_Filtro_Usuarios.SelectedIndex = 0;
            }
        }

        public void BindRoles(DataTable dt)
        {
            if (dt == null || dt.Columns.Count == 0)
                throw new Exception("El listado de roles no devolvió columnas.");

            string idCol =
                dt.Columns.Contains("RolID") ? "RolID" :
                dt.Columns.Contains("RoleID") ? "RoleID" :
                dt.Columns.Contains("IdRol") ? "IdRol" :
                dt.Columns.Contains("ID") ? "ID" :
                dt.Columns[0].ColumnName;

            string nameCol =
                dt.Columns.Contains("RolNombre") ? "RolNombre" :
                dt.Columns.Contains("RoleName") ? "RoleName" :
                dt.Columns.Contains("NombreRol") ? "NombreRol" :
                dt.Columns.Contains("Nombre") ? "Nombre" :
                dt.Columns.Contains("Name") ? "Name" :
                (dt.Columns.Count > 1 ? dt.Columns[1].ColumnName : dt.Columns[0].ColumnName);

            cmbox_Rol_Usuarios.DisplayMember = nameCol;
            cmbox_Rol_Usuarios.ValueMember = idCol;
            cmbox_Rol_Usuarios.DataSource = dt;

            var dtFiltro = dt.Copy();
            var rowTodos = dtFiltro.NewRow();
            rowTodos[idCol] = DBNull.Value;
            rowTodos[nameCol] = "Todos";
            dtFiltro.Rows.InsertAt(rowTodos, 0);

            cmbox_RolFiltro_Usuarios.DisplayMember = nameCol;
            cmbox_RolFiltro_Usuarios.ValueMember = idCol;
            cmbox_RolFiltro_Usuarios.DataSource = dtFiltro;
        }

        public void BindEstados(DataTable dtFiltro, DataTable dtForm)
        {
            cmbox_EstadoFiltro_Usuarios.DisplayMember = "EstadoTexto";
            cmbox_EstadoFiltro_Usuarios.ValueMember = "Activo";
            cmbox_EstadoFiltro_Usuarios.DataSource = dtFiltro;

            cmbox_Estado_Usuarios.DisplayMember = "EstadoTexto";
            cmbox_Estado_Usuarios.ValueMember = "Activo";
            cmbox_Estado_Usuarios.DataSource = dtForm;
        }

        public void RenderUsuarios(DataTable dt, int? selectedUsuarioId)
        {
            flowUsuarios.SuspendLayout();
            flowUsuarios.Controls.Clear();

            int count = 0;

            if (dt == null) dt = new DataTable();

            foreach (DataRow r in dt.Rows)
            {
                int id = I(r, 0, "UsuarioID", "UsuarioID_Usuarios", "IdUsuario", "Id");
                if (id <= 0) continue;

                string username = S(r, "LoginName", "Username", "UserName", "NombreUsuario", "Usuario");
                string nombres = S(r, "Nombres", "Nombre", "FirstName");
                string apellidos = S(r, "Apellidos", "Apellido", "LastName");
                string correo = S(r, "Correo", "Email", "CorreoElectronico");
                string rol = S(r, "RolNombre", "RoleName", "Rol", "NombreRol", "Nombre_Roles", "Role");

                bool activo = B(r, true, "Activo", "Activo_Usuarios", "Estado", "IsActive");
                bool hasFoto = B(r, false, "HasFoto", "TieneFoto", "ConFoto");

                var card = new UsuariosTaskCard();
                card.Width = flowUsuarios.ClientSize.Width - 22;

                card.Bind(id, username, nombres, apellidos, correo, rol, activo, hasFoto);
                card.SetSelected(selectedUsuarioId.HasValue && id == selectedUsuarioId.Value);

                card.UsuarioSeleccionado += async (s, e) => { await _presenter.SeleccionarAsync(e.UsuarioID); };

                flowUsuarios.Controls.Add(card);
                count++;
            }

            lbl_CantResultados_Usuarios.Text = $"{count} resultados";
            flowUsuarios.ResumeLayout();
        }

        public void ShowUsuarioDetalle(DataRow row)
        {
            txt_LoginName_Usuarios.Text = Convert.ToString(row["LoginName"]);
            txt_Nombre_Usuarios.Text = Convert.ToString(row["Nombres"]);
            txt_Apellido_Usuarios.Text = Convert.ToString(row["Apellidos"]);
            txt_Correo_Usuarios.Text = Convert.ToString(row["Correo"]);
            txt_Telefono_Usuarios.Text = Convert.ToString(row["Telefono"]);

            if (row.Table.Columns.Contains("RolID") && row["RolID"] != DBNull.Value)
                cmbox_Rol_Usuarios.SelectedValue = Convert.ToInt32(row["RolID"]);

            if (row.Table.Columns.Contains("Activo") && row["Activo"] != DBNull.Value)
                cmbox_Estado_Usuarios.SelectedValue = Convert.ToBoolean(row["Activo"]);

            txt_Pass_Usuarios.Text = "";
        }

        public GuardarUsuarioRequest BuildGuardarRequest(int? usuarioTargetId)
        {
            return new GuardarUsuarioRequest
            {
                UsuarioSesionId = UsuarioSesionId,
                UsuarioTargetId = usuarioTargetId,
                Username = (txt_LoginName_Usuarios.Text ?? "").Trim(),
                Nombres = (txt_Nombre_Usuarios.Text ?? "").Trim(),
                Apellidos = (txt_Apellido_Usuarios.Text ?? "").Trim(),
                Email = string.IsNullOrWhiteSpace(txt_Correo_Usuarios.Text) ? null : txt_Correo_Usuarios.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(txt_Telefono_Usuarios.Text) ? null : txt_Telefono_Usuarios.Text.Trim(),
                RolId = cmbox_Rol_Usuarios.SelectedValue == null ? 0 : Convert.ToInt32(cmbox_Rol_Usuarios.SelectedValue),
                Activo = cmbox_Estado_Usuarios.SelectedValue == null || Convert.ToBoolean(cmbox_Estado_Usuarios.SelectedValue),
                PasswordPlain = txt_Pass_Usuarios.Text,
                FotoBytes = _fotoPendienteBytes
            };
        }

        public void ClearPendingFoto()
        {
            _fotoPendienteBytes = null;
            _fotoPendienteNombre = null;
        }

        public void SetFotoFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                SetDefaultFoto();
                return;
            }

            using (var ms = new MemoryStream(bytes))
            using (var img = Image.FromStream(ms))
            {
                var bmp = new Bitmap(img);
                var old = pictruebox_Usuarios.Image;
                pictruebox_Usuarios.Image = bmp;
                old?.Dispose();
            }

            lbl_SeleccionFoto_Usuarios.Text = "Foto cargada";
        }

        public void SetDefaultFoto()
        {
            var old = pictruebox_Usuarios.Image;
            pictruebox_Usuarios.Image = null;
            old?.Dispose();

            lbl_SeleccionFoto_Usuarios.Text = "Sin selección";
        }

        public void ResetForm()
        {
            txt_LoginName_Usuarios.Text = "";
            txt_Pass_Usuarios.Text = "";
            txt_Nombre_Usuarios.Text = "";
            txt_Apellido_Usuarios.Text = "";
            txt_Correo_Usuarios.Text = "";
            txt_Telefono_Usuarios.Text = "";

            if (cmbox_Rol_Usuarios.Items.Count > 0) cmbox_Rol_Usuarios.SelectedIndex = 0;
            if (cmbox_Estado_Usuarios.Items.Count > 0) cmbox_Estado_Usuarios.SelectedIndex = 0;

            SetDefaultFoto();
            ClearPendingFoto();

            SetModeActualizar(false);

            foreach (var c in flowUsuarios.Controls.OfType<UsuariosTaskCard>())
                c.SetSelected(false);
        }

        public void SetModeActualizar(bool actualizar)
        {
            btn_Registrar_Usuarios.Text = actualizar ? "Actualizar" : "Registrar";
            btn_Limpiar_Usuarios.Text = "Limpiar";
        }

        public void SetEditingEnabled(bool enabled)
        {
            txt_LoginName_Usuarios.Enabled = enabled;
            txt_Pass_Usuarios.Enabled = enabled;
            txt_Nombre_Usuarios.Enabled = enabled;
            txt_Apellido_Usuarios.Enabled = enabled;
            txt_Correo_Usuarios.Enabled = enabled;
            txt_Telefono_Usuarios.Enabled = enabled;
            cmbox_Rol_Usuarios.Enabled = enabled;
            cmbox_Estado_Usuarios.Enabled = enabled;
            btn_SubirFoto_Usuarios.Enabled = enabled;
        }

        public void SetGuardarEnabled(bool enabled)
        {
            btn_Registrar_Usuarios.Enabled = enabled;
        }

        public void SetGestionarPermisosEnabled(bool enabled)
        {
            if (btn_GestionarPermisos_Usuarios != null)
                btn_GestionarPermisos_Usuarios.Enabled = enabled;
        }

        public void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string msg, Exception ex = null)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(msg) ? "Ocurrió un error al procesar la operación." : msg,
                "SISV",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        public void CloseView()
        {
            BeginInvoke(new Action(() => Close()));
        }

        private void SeleccionarFoto()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.webp";
                ofd.Title = "Seleccionar foto de usuario";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                _fotoPendienteNombre = Path.GetFileName(ofd.FileName);
                _fotoPendienteBytes = File.ReadAllBytes(ofd.FileName);

                lbl_SeleccionFoto_Usuarios.Text = _fotoPendienteNombre;
                SetFotoFromBytes(_fotoPendienteBytes);
            }
        }

        private void btn_GestionarPermisos_Usuarios_Click(object sender, EventArgs e)
        {
            pnlPermisosOverlay.Visible = true;
            pnlPermisosOverlay.BringToFront();
            _ucPermisos.SetActor(UsuarioSesionId);
        }

        public void CerrarPermisos()
        {
            pnlPermisosOverlay.Visible = false;
        }

        private void InitPermisosOverlay()
        {
            if (pnlPermisosOverlay == null)
                throw new InvalidOperationException("No existe el panel pnlPermisosOverlay en el diseñador.");

            _ucPermisos = new UC_GestionPermisos();
            _ucPermisos.Dock = DockStyle.Fill;
            _ucPermisos.VolverSolicitado += (_, __) => CerrarPermisos();

            pnlPermisosOverlay.Controls.Clear();
            pnlPermisosOverlay.Controls.Add(_ucPermisos);
        }

        private static string S(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToString(row[c]);
            return "";
        }

        private static bool B(DataRow row, bool def, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToBoolean(row[c]);
            return def;
        }

        private static int I(DataRow row, int def, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToInt32(row[c]);
            return def;
        }
    }
}