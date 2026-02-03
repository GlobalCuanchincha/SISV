using Capa_Corte_Transversal.Helpers;
using Capa_Corte_Transversal.Security;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Usuarios;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Usuarios : Form
    {
        private readonly object _session;

        private Timer _debounceTimer;

        private int _usuarioSesionId;
        private byte _rolSesionId; // 1=SuperAdmin, 2=Admin, 3=Cajero, 4=Técnico
        private int _usuarioSeleccionadoId;

        private byte[] _fotoPendienteBytes;
        private string _fotoPendienteNombre;

        public Form_Usuarios() : this(null) { }

        public Form_Usuarios(object session)
        {
            InitializeComponent();
            _session = session;

            _debounceTimer = new Timer();
            _debounceTimer.Interval = 350;
            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop();
                await BuscarAsync();
            };

            WireEvents();
            ApplyFormStyle();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _usuarioSesionId = TryGetUsuarioSesionId();
            if (_usuarioSesionId <= 0)
            {
                MessageBox.Show("No se pudo obtener UsuarioID de sesión.\n\nAbre este formulario pasando la sesión:\nnew Form_Usuarios(_session)",
                    "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetEditingEnabled(false);
                return;
            }

            try
            {
                _rolSesionId = await GetRolSesionFromDbAsync(_usuarioSesionId);
            }
            catch
            {
                _rolSesionId = 0;
            }

            // Seguridad: este formulario es solo para Admin/SuperAdmin
            if (_rolSesionId != 1 && _rolSesionId != 2)
            {
                MessageBox.Show("Acceso denegado: solo Admin/SuperAdmin puede gestionar usuarios.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SetEditingEnabled(false);
                return;
            }

            EnsureFiltroCombo();

            await CargarRolesAsync();
            await CargarEstadosAsync();
            await CargarRecientesAsync();

            LimpiarFormulario();
        }

        private void ApplyFormStyle()
        {
            this.BackColor = Color.FromArgb(246, 247, 251);

            // Flow en modo “lista” (vertical)
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

            btn_Registrar_Usuarios.Click += async (s, e) =>
            {
                try { await GuardarAsync(); }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            btn_Limpiar_Usuarios.Click += (s, e) => LimpiarFormulario();

            btn_SubirFoto_Usuarios.Click += (s, e) => SeleccionarFoto();

            // Mantener ancho de cards cuando el panel cambie de tamaño
            flowUsuarios.SizeChanged += (s, e) =>
            {
                foreach (var c in flowUsuarios.Controls.OfType<UsuariosTaskCard>())
                    c.Width = flowUsuarios.ClientSize.Width - 22;
            };
        }

        private void RestartDebounce()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void EnsureFiltroCombo()
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

        private int TryGetUsuarioSesionId()
        {
            try
            {
                // Si tienes tu SessionHelper (como en tus otros formularios)
                int id = SessionHelper.GetUsuarioID(_session);
                return id;
            }
            catch
            {
                return 0;
            }
        }

        private static SqlConnection CreateConn()
        {
            string cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("No existe ConnectionString 'SISV' en App.config");

            return new SqlConnection(cs);
        }

        private static async Task<DataTable> ExecDataTableAsync(string storedProc, Action<SqlCommand> fillParams)
        {
            using (var cn = CreateConn())
            using (var cmd = new SqlCommand(storedProc, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fillParams?.Invoke(cmd);

                var dt = new DataTable();
                await cn.OpenAsync();
                da.Fill(dt);
                return dt;
            }
        }

        private static async Task<DataTable> TryExecDataTableAsync(string[] candidates, Action<SqlCommand> fillParams)
        {
            Exception last = null;

            foreach (var sp in candidates)
            {
                try
                {
                    return await ExecDataTableAsync(sp, fillParams);
                }
                catch (SqlException ex)
                {
                    last = ex;

                    // Si no existe el SP, probamos siguiente
                    if (ex.Number == 2812) // Could not find stored procedure
                        continue;

                    throw;
                }
            }

            throw last ?? new Exception("No se pudo ejecutar ningún procedimiento almacenado.");
        }

        private static async Task<byte> GetRolSesionFromDbAsync(int usuarioId)
        {
            DataTable dt = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_GetById", "dbo.usp_Usuario_GetById" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
                    cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioId);
                });

            if (dt.Rows.Count == 0) return 0;

            if (dt.Columns.Contains("RolID") && dt.Rows[0]["RolID"] != DBNull.Value)
                return Convert.ToByte(dt.Rows[0]["RolID"]);

            var o = dt.Rows[0][0];
            if (o == null || o == DBNull.Value) return 0;
            return Convert.ToByte(o);
        }

        private async Task CargarRolesAsync()
        {
            var dt = await TryExecDataTableAsync(
                new[] { "sec.usp_Rol_Listar", "dbo.usp_Rol_Listar" },
                cmd => { cmd.Parameters.AddWithValue("@SoloActivos", 1); }
            );

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

        private static int FindIndexByValue(ComboBox cbo, object value)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                var drv = cbo.Items[i] as DataRowView;
                if (drv == null) continue;

                if (drv.Row.Table.Columns.Contains(cbo.ValueMember))
                {
                    var v = drv.Row[cbo.ValueMember];
                    if (v != DBNull.Value && Convert.ToString(v) == Convert.ToString(value))
                        return i;
                }
            }
            return -1;
        }

        private async Task CargarEstadosAsync()
        {
            // Filtro
            var dtFiltro = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Estados_Listar", "dbo.usp_Usuario_Estados_Listar" },
                cmd => cmd.Parameters.AddWithValue("@Modo", "Filtro"));

            cmbox_EstadoFiltro_Usuarios.DisplayMember = "EstadoTexto";
            cmbox_EstadoFiltro_Usuarios.ValueMember = "Activo";
            cmbox_EstadoFiltro_Usuarios.DataSource = dtFiltro;

            // Form (sin "Todos")
            var dtForm = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Estados_Listar", "dbo.usp_Usuario_Estados_Listar" },
                cmd => cmd.Parameters.AddWithValue("@Modo", "Form"));

            cmbox_Estado_Usuarios.DisplayMember = "EstadoTexto";
            cmbox_Estado_Usuarios.ValueMember = "Activo";
            cmbox_Estado_Usuarios.DataSource = dtForm;
        }

        private async Task CargarRecientesAsync()
        {
            var dt = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_ListarRecientes", "dbo.usp_Usuario_ListarRecientes" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioSesionId);
                    cmd.Parameters.AddWithValue("@Top", 200);
                });

            RenderFlow(dt);
        }

        private async Task BuscarAsync()
        {
            string texto = txt_Buscador_Usuarios.Text?.Trim() ?? "";
            string filtroRaw = (cmbox_Filtro_Usuarios.Text ?? "Todos").Trim().ToLowerInvariant();

            string rolFiltro = (cmbox_RolFiltro_Usuarios.Text ?? "Todos").Trim();
            string estadoTxt = (cmbox_EstadoFiltro_Usuarios.Text ?? "Todos").Trim();

            bool hasAny = !string.IsNullOrWhiteSpace(texto)
                          || (rolFiltro != "Todos")
                          || (estadoTxt != "Todos");

            if (!hasAny)
            {
                await CargarRecientesAsync();
                return;
            }

            string filtroKey = "todos";
            if (filtroRaw.Contains("usuario")) filtroKey = "login";
            else if (filtroRaw.Contains("nombre")) filtroKey = "nombre";
            else if (filtroRaw.Contains("correo") || filtroRaw.Contains("email")) filtroKey = "email";

            var dt = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Buscar", "dbo.usp_Usuario_Buscar" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioSesionId);
                    cmd.Parameters.AddWithValue("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto);
                    cmd.Parameters.AddWithValue("@Filtro", filtroKey);
                    cmd.Parameters.AddWithValue("@RolFiltro", rolFiltro);
                    cmd.Parameters.AddWithValue("@EstadoFiltro", estadoTxt);
                    cmd.Parameters.AddWithValue("@Top", 200);
                });

            RenderFlow(dt);
        }

        private void RenderFlow(DataTable dt)
        {
            flowUsuarios.SuspendLayout();
            flowUsuarios.Controls.Clear();

            int count = 0;

            // Helpers locales para leer columnas con fallback (evita "columna no pertenece")
            string S(DataRow row, params string[] cols)
            {
                foreach (var c in cols)
                {
                    if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                        return Convert.ToString(row[c]);
                }
                return "";
            }

            bool B(DataRow row, bool def, params string[] cols)
            {
                foreach (var c in cols)
                {
                    if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                        return Convert.ToBoolean(row[c]);
                }
                return def;
            }

            int I(DataRow row, int def, params string[] cols)
            {
                foreach (var c in cols)
                {
                    if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                        return Convert.ToInt32(row[c]);
                }
                return def;
            }

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

                card.Bind(
                    id,
                    username,
                    nombres,
                    apellidos,
                    correo,
                    rol,
                    activo,
                    hasFoto
                );

                card.SetSelected(id == _usuarioSeleccionadoId);

                card.UsuarioSeleccionado += async (s, e) =>
                {
                    await SeleccionarUsuarioAsync(e.UsuarioID);
                };

                flowUsuarios.Controls.Add(card);
                count++;
            }

            lbl_CantResultados_Usuarios.Text = $"{count} resultados";

            flowUsuarios.ResumeLayout();
        }

        private async Task SeleccionarUsuarioAsync(int usuarioId)
        {
            _usuarioSeleccionadoId = usuarioId;

            foreach (var c in flowUsuarios.Controls.OfType<UsuariosTaskCard>())
                c.SetSelected(c.UsuarioID == usuarioId);

            var dt = await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_GetById", "dbo.usp_Usuario_GetById" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioSesionId);
                    cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioId);
                });

            if (dt.Rows.Count == 0) return;
            var row = dt.Rows[0];

            txt_LoginName_Usuarios.Text = Convert.ToString(row["LoginName"]);
            txt_Nombre_Usuarios.Text = Convert.ToString(row["Nombres"]);
            txt_Apellido_Usuarios.Text = Convert.ToString(row["Apellidos"]);
            txt_Correo_Usuarios.Text = Convert.ToString(row["Correo"]);
            txt_Telefono_Usuarios.Text = Convert.ToString(row["Telefono"]);

            if (row.Table.Columns.Contains("RolID") && row["RolID"] != DBNull.Value)
                cmbox_Rol_Usuarios.SelectedValue = Convert.ToInt32(row["RolID"]);

            if (row.Table.Columns.Contains("Activo") && row["Activo"] != DBNull.Value)
                cmbox_Estado_Usuarios.SelectedValue = Convert.ToBoolean(row["Activo"]);

            // Password: NO se carga desde BD
            txt_Pass_Usuarios.Text = "";

            // Foto
            await TryLoadFotoAsync(usuarioId);

            // Botón dice "Actualizar" cuando hay selección
            btn_Registrar_Usuarios.Text = "Actualizar";
            btn_Limpiar_Usuarios.Text = "Limpiar";

            // Permisos de edición por rol
            SetEditingEnabled(_rolSesionId == 1 || _rolSesionId == 2);
        }

        private async Task TryLoadFotoAsync(int usuarioIdTarget)
        {
            try
            {
                var dt = await TryExecDataTableAsync(
                    new[] { "sec.usp_Usuario_Foto_Get", "dbo.usp_Usuario_Foto_Get" },
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", _usuarioSesionId);
                        cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioIdTarget);
                    });

                if (dt.Rows.Count == 0 || dt.Rows[0]["Foto"] == DBNull.Value)
                {
                    SetDefaultFoto();
                    return;
                }

                byte[] bytes = (byte[])dt.Rows[0]["Foto"];
                SetPictureFromBytes(bytes);
                lbl_SeleccionFoto_Usuarios.Text = "Foto cargada";
            }
            catch
            {
                SetDefaultFoto();
            }
        }

        private void SetEditingEnabled(bool enabled)
        {
            // Panel formulario
            txt_LoginName_Usuarios.Enabled = enabled;
            txt_Pass_Usuarios.Enabled = enabled;

            txt_Nombre_Usuarios.Enabled = enabled;
            txt_Apellido_Usuarios.Enabled = enabled;
            txt_Correo_Usuarios.Enabled = enabled;
            txt_Telefono_Usuarios.Enabled = enabled;

            cmbox_Rol_Usuarios.Enabled = enabled;
            cmbox_Estado_Usuarios.Enabled = enabled;

            btn_Registrar_Usuarios.Enabled = enabled;
            btn_SubirFoto_Usuarios.Enabled = true; // siempre se ve, pero solo guarda cuando tenga permiso (en SP también valida)
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

                // preview
                SetPictureFromBytes(_fotoPendienteBytes);
            }
        }

        private void SetDefaultFoto()
        {
            // liberar imagen anterior para evitar fuga de memoria / locks
            var old = pictruebox_Usuarios.Image;
            pictruebox_Usuarios.Image = null;
            old?.Dispose();

            lbl_SeleccionFoto_Usuarios.Text = "Sin selección";
        }

        private void SetPictureFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                SetDefaultFoto();
                return;
            }

            using (var ms = new MemoryStream(bytes))
            using (var img = Image.FromStream(ms))
            {
                var bmp = new Bitmap(img); // clonar para no depender del stream
                var old = pictruebox_Usuarios.Image;
                pictruebox_Usuarios.Image = bmp;
                old?.Dispose();
            }
        }

        private static byte[] ToBytes(object v)
        {
            if (v == null || v == DBNull.Value) return null;

            if (v is byte[] b) return b;

            if (v is string s)
            {
                s = s.Trim();
                if (s.Length == 0) return null;

                try { return Convert.FromBase64String(s); }
                catch { }

                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    s = s.Substring(2);

                if (s.Length % 2 != 0)
                    throw new ArgumentException("Hash/Salt en hex inválido (longitud impar).");

                if (!s.All(Uri.IsHexDigit))
                    throw new ArgumentException("Hash/Salt string no es Base64 ni Hex.");

                byte[] bytes = new byte[s.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                    bytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);

                return bytes;
            }

            throw new ArgumentException("Tipo de Hash/Salt no soportado: " + v.GetType().FullName);
        }


        private async Task GuardarAsync()
        {
            try
            {
                if (_rolSesionId != 1 && _rolSesionId != 2)
                {
                    MessageBox.Show("Acceso denegado.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string username = txt_LoginName_Usuarios.Text.Trim();
                string pass = txt_Pass_Usuarios.Text;
                string nombres = txt_Nombre_Usuarios.Text.Trim();
                string apellidos = txt_Apellido_Usuarios.Text.Trim();
                string correo = txt_Correo_Usuarios.Text.Trim();
                string tel = txt_Telefono_Usuarios.Text.Trim();

                if (string.IsNullOrWhiteSpace(username))
                {
                    MessageBox.Show("Ingrese el nombre de usuario.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(nombres))
                {
                    MessageBox.Show("Ingrese los nombres.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(apellidos))
                {
                    MessageBox.Show("Ingrese los apellidos.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbox_Rol_Usuarios.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione el rol.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int rolId = Convert.ToInt32(cmbox_Rol_Usuarios.SelectedValue);

                bool activo = true;
                if (cmbox_Estado_Usuarios.SelectedValue != null)
                    activo = Convert.ToBoolean(cmbox_Estado_Usuarios.SelectedValue);

                bool esNuevo = (_usuarioSeleccionadoId <= 0);

                object hashObj = null;
                object saltObj = null;
                int iterations = 0;

                if (esNuevo)
                {
                    if (string.IsNullOrWhiteSpace(pass))
                    {
                        MessageBox.Show("Ingrese la contraseña para registrar.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var created = PasswordHasher.Create(pass, 10000);
                    hashObj = created.Hash;  
                    saltObj = created.Salt;
                    iterations = created.Iterations;
                }

                byte[] hashBytes = ToBytes(hashObj);
                byte[] saltBytes = ToBytes(saltObj);

                var dt = await TryExecDataTableAsync(
                    new[] { "sec.usp_Usuario_Guardar", "dbo.usp_Usuario_Guardar" },
                    cmd =>
                    {
                        cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = _usuarioSesionId;

                        var pTarget = cmd.Parameters.Add("@UsuarioIDTarget", SqlDbType.Int);
                        pTarget.Value = esNuevo ? (object)DBNull.Value : _usuarioSeleccionadoId;

                        cmd.Parameters.Add("@Username", SqlDbType.NVarChar, 100).Value = username;
                        cmd.Parameters.Add("@Nombres", SqlDbType.NVarChar, 100).Value = nombres;
                        cmd.Parameters.Add("@Apellidos", SqlDbType.NVarChar, 100).Value = apellidos;

                        var pEmail = cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 200);
                        pEmail.Value = string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo;

                        var pTel = cmd.Parameters.Add("@Telefono", SqlDbType.NVarChar, 30);
                        pTel.Value = string.IsNullOrWhiteSpace(tel) ? (object)DBNull.Value : tel;

                        cmd.Parameters.Add("@RolID", SqlDbType.Int).Value = rolId;
                        cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;

                        var pHash = cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, -1);
                        pHash.Value = esNuevo && hashBytes != null ? (object)hashBytes : DBNull.Value;

                        var pSalt = cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, -1);
                        pSalt.Value = esNuevo && saltBytes != null ? (object)saltBytes : DBNull.Value;

                        var pIter = cmd.Parameters.Add("@PasswordIterations", SqlDbType.Int);
                        pIter.Value = esNuevo && iterations > 0 ? (object)iterations : DBNull.Value;
                    });

                int idGuardado = _usuarioSeleccionadoId;
                if (esNuevo)
                {
                    if (dt.Rows.Count == 0)
                        throw new Exception("El SP no devolvió el UsuarioID creado.");

                    var row = dt.Rows[0];

                    if (dt.Columns.Contains("UsuarioID") && row["UsuarioID"] != DBNull.Value)
                        idGuardado = Convert.ToInt32(row["UsuarioID"]);
                    else if (dt.Columns.Contains("Id") && row["Id"] != DBNull.Value)
                        idGuardado = Convert.ToInt32(row["Id"]);
                    else if (dt.Columns.Contains("UsuarioIDTarget") && row["UsuarioIDTarget"] != DBNull.Value)
                        idGuardado = Convert.ToInt32(row["UsuarioIDTarget"]);
                    else
                        throw new Exception("No se encontró columna de ID devuelto (UsuarioID/Id/UsuarioIDTarget).");
                }

                if (!esNuevo && !string.IsNullOrWhiteSpace(pass))
                {
                    var created2 = PasswordHasher.Create(pass, 10000);

                    byte[] hash2 = ToBytes(created2.Hash);
                    byte[] salt2 = ToBytes(created2.Salt);

                    await TryExecDataTableAsync(
                        new[]
                        {
                    "sec.usp_Usuario_Password_Set",
                    "sec.usp_Usuario_SetPassword",
                    "sec.usp_Usuario_SetPasswordHash",
                    "dbo.usp_Usuario_SetPassword",
                    "dbo.usp_Usuario_SetPasswordHash"
                        },
                        cmd =>
                        {
                            cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = _usuarioSesionId;
                            cmd.Parameters.Add("@UsuarioIDTarget", SqlDbType.Int).Value = idGuardado;

                            var pHash = cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, -1);
                            pHash.Value = (object)hash2 ?? DBNull.Value;

                            var pSalt = cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, -1);
                            pSalt.Value = (object)salt2 ?? DBNull.Value;

                            cmd.Parameters.Add("@PasswordIterations", SqlDbType.Int).Value = created2.Iterations;
                        });

                    // Limpia campo para que no quede visible
                    txt_Pass_Usuarios.Text = "";
                }

                // Guardar foto si hay pendiente
                if (_fotoPendienteBytes != null && _fotoPendienteBytes.Length > 0)
                {
                    await SubirFotoAsync(idGuardado);
                }

                MessageBox.Show(esNuevo ? "Usuario registrado." : "Usuario actualizado.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                _usuarioSeleccionadoId = idGuardado;

                await BuscarAsync(); // refresca
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "Error SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task SubirFotoAsync(int usuarioIdTarget)
        {
            await TryExecDataTableAsync(
                new[] { "sec.usp_Usuario_Foto_Set", "dbo.usp_Usuario_Foto_Set" },
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioSesionId);
                    cmd.Parameters.AddWithValue("@UsuarioIDTarget", usuarioIdTarget);
                    cmd.Parameters.AddWithValue("@Foto", _fotoPendienteBytes);
                    cmd.Parameters.AddWithValue("@MimeType", (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileName", string.IsNullOrWhiteSpace(_fotoPendienteNombre) ? (object)DBNull.Value : _fotoPendienteNombre);
                });

            // refleja en UI
            SetPictureFromBytes(_fotoPendienteBytes);
            lbl_SeleccionFoto_Usuarios.Text = "Foto guardada";

            // limpia pendiente
            _fotoPendienteBytes = null;
            _fotoPendienteNombre = null;
        }

        private void LimpiarFormulario()
        {
            _usuarioSeleccionadoId = 0;

            txt_LoginName_Usuarios.Text = "";
            txt_Pass_Usuarios.Text = "";
            txt_Nombre_Usuarios.Text = "";
            txt_Apellido_Usuarios.Text = "";
            txt_Correo_Usuarios.Text = "";
            txt_Telefono_Usuarios.Text = "";


            // reset combos
            if (cmbox_Rol_Usuarios.Items.Count > 0) cmbox_Rol_Usuarios.SelectedIndex = 0;
            if (cmbox_Estado_Usuarios.Items.Count > 0) cmbox_Estado_Usuarios.SelectedIndex = 0;

            SetDefaultFoto();

            foreach (var c in flowUsuarios.Controls.OfType<UsuariosTaskCard>())
                c.SetSelected(false);

            bool puede = (_rolSesionId == 1 || _rolSesionId == 2);
            SetEditingEnabled(puede);
        }
    }
}
