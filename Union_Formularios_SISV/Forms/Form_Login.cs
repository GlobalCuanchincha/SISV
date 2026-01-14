using Capa_Corte_Transversal.Security;
using Datos_Acceso.Common;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms;

namespace Union_Formularios_SISV
{
    public partial class Form_Login : Form
    {
        // Animación error
        private Timer _errAnim;
        private int _errTargetHeight = 36;
        private int _errStep = 4;
        private int _alphaStep = 18;
        private int _errAlpha = 0;
        private bool _errShowing = false;

        // Auto-hide error
        private Timer _errHideTimer;
        private int _errDisplayMs = 2000;

        // Evento para AppContext
        public event EventHandler<LoginSession> LoginSucceeded;

        public Form_Login()
        {
            InitializeComponent();
            InitErrorUi(); // timers + estilo base (para que nunca sea null)
        }

        private void Form_Login_Load(object sender, EventArgs e)
        {
            try
            {
                // Label sobre la imagen
                label10.Parent = guna2PictureBox1;
                label10.BackColor = Color.Transparent;
                label10.Location = new Point(
                    (guna2PictureBox1.Width - label10.Width) / 30,
                    guna2PictureBox1.Height - label10.Height - 25
                );

                // Posición del mensaje debajo del botón
                RepositionError();

                // Password UI
                txt_pass.UseSystemPasswordChar = true;

                // Cargar "Recuérdame"
                LoadRememberMe();
            }
            catch
            {
                // nunca se cae
            }
        }

        private void Form_Login_Resize(object sender, EventArgs e)
        {
            try { RepositionError(); } catch { }
        }

        private void RepositionError()
        {
            msg_error.Width = btnLogin.Width;
            msg_error.Left = btnLogin.Left;
            msg_error.Top = btnLogin.Bottom + 12;
        }

        private void InitErrorUi()
        {
            // Estado base del msg_error
            msg_error.AutoSize = false;
            msg_error.TextAlign = ContentAlignment.MiddleCenter;
            msg_error.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            msg_error.Height = 0;
            msg_error.Text = "";

            _errAlpha = 0;
            msg_error.BackColor = Color.FromArgb(0, 255, 235, 238);
            msg_error.ForeColor = Color.FromArgb(0, 183, 28, 28);

            // Timer animación
            _errAnim = new Timer();
            _errAnim.Interval = 15;
            _errAnim.Tick += ErrAnim_Tick;

            // Timer auto-hide
            _errHideTimer = new Timer();
            _errHideTimer.Interval = _errDisplayMs;
            _errHideTimer.Tick += (s, e) =>
            {
                try
                {
                    _errHideTimer.Stop();
                    ClearError();
                }
                catch { }
            };
        }

        private void ShowError(string message)
        {
            if (msg_error == null) return;

            msg_error.Text = "⚠  " + message;
            _errShowing = true;

            if (_errAnim != null) _errAnim.Start();

            if (_errHideTimer != null)
            {
                _errHideTimer.Stop();
                _errHideTimer.Start();
            }
        }

        private void ClearError()
        {
            _errShowing = false;
            if (_errAnim != null) _errAnim.Start();
        }

        private void ErrAnim_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_errShowing)
                {
                    if (msg_error.Height < _errTargetHeight)
                        msg_error.Height = Math.Min(_errTargetHeight, msg_error.Height + _errStep);

                    if (_errAlpha < 255)
                        _errAlpha = Math.Min(255, _errAlpha + _alphaStep);

                    msg_error.BackColor = Color.FromArgb(_errAlpha, 255, 235, 238);
                    msg_error.ForeColor = Color.FromArgb(_errAlpha, 183, 28, 28);

                    if (msg_error.Height == _errTargetHeight && _errAlpha == 255)
                        _errAnim.Stop();
                }
                else
                {
                    if (msg_error.Height > 0)
                        msg_error.Height = Math.Max(0, msg_error.Height - _errStep);

                    if (_errAlpha > 0)
                        _errAlpha = Math.Max(0, _errAlpha - _alphaStep);

                    msg_error.BackColor = Color.FromArgb(_errAlpha, 255, 235, 238);
                    msg_error.ForeColor = Color.FromArgb(_errAlpha, 183, 28, 28);

                    if (msg_error.Height == 0 && _errAlpha == 0)
                    {
                        msg_error.Text = "";
                        _errAnim.Stop();
                    }
                }
            }
            catch
            {
                // nunca se cae
                try { _errAnim.Stop(); } catch { }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            try
            {
                ClearError();

                string user = (txt_user.Text ?? "").Trim();
                string pass = txt_pass.Text ?? "";

                if (string.IsNullOrWhiteSpace(user))
                {
                    ShowError("Ingrese el usuario.");
                    txt_user.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(pass))
                {
                    ShowError("Ingrese la contraseña.");
                    txt_pass.Focus();
                    return;
                }

                var dt = SqlExecutor.ExecuteDataTable(
                    "dbo.sp_Usuario_GetByUsername",
                    new SqlParameter("@Username", user)
                );

                if (dt == null || dt.Rows.Count == 0)
                {
                    ShowError("Usuario no existe.");
                    return;
                }

                var row = dt.Rows[0];

                bool activo = row["Activo_Usuarios"] != DBNull.Value && (bool)row["Activo_Usuarios"];
                if (!activo)
                {
                    ShowError("Usuario inactivo.");
                    return;
                }

                if (row["PasswordSalt_Usuarios"] == DBNull.Value)
                {
                    ShowError("Contraseña no configurada. Use '¿Olvidaste tu contraseña?' para restablecer.");
                    return;
                }

                byte[] hash = (byte[])row["PasswordHash_Usuarios"];
                byte[] salt = (byte[])row["PasswordSalt_Usuarios"];
                int iterations = row["PasswordIterations_Usuarios"] == DBNull.Value ? 10000 : (int)row["PasswordIterations_Usuarios"];

                bool ok = PasswordHasher.Verify(pass, hash, salt, iterations);
                if (!ok)
                {
                    ShowError("Contraseña incorrecta.");
                    return;
                }

                // RememberMe (no debe tumbar el login si falla)
                SaveRememberMe(user, pass);

                // Crear sesión y notificar al AppContext
                byte roleId = (byte)row["RoleID_Usuarios"];
                int usuarioId = (int)row["UsuarioID_Usuarios"];

                LoginSucceeded?.Invoke(this, new LoginSession
                {
                    UsuarioId = usuarioId,
                    Username = user,
                    RoleId = roleId
                });

                // IMPORTANTÍSIMO: no sigas ejecutando lógica aquí, el AppContext cerrará el form
                return;
            }
            catch
            {
                ShowError("Error al iniciar sesión. Verifique conexión e intente nuevamente.");
            }
        }

        private void LoadRememberMe()
        {
            try
            {
                if (RememberMeStore.TryLoad(out string user, out string passProtected))
                {
                    Chbox_Recuerdame.Checked = true;
                    txt_user.Text = user;
                    txt_pass.Text = Dpapi.Unprotect(passProtected);
                }
                else
                {
                    Chbox_Recuerdame.Checked = false;
                }
            }
            catch
            {
                Chbox_Recuerdame.Checked = false;
            }
        }

        private void SaveRememberMe(string user, string pass)
        {
            try
            {
                if (Chbox_Recuerdame.Checked)
                    RememberMeStore.Save(user, Dpapi.Protect(pass));
                else
                    RememberMeStore.Clear();
            }
            catch
            {
                // no crash
            }
        }

        private void Lnk_Olvidaste_pass_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                using (var frm = new Form_Recu())
                {
                    frm.StartPosition = FormStartPosition.CenterParent;
                    frm.ShowDialog(this); // modal
                }
            }
            catch
            {
                ShowError("No se pudo abrir la recuperación.");
            }
        }
    }
}
