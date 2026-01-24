using Capa_Corte_Transversal.Security;
using Datos_Acceso.Common;
using Dominio_SISV.Services;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms;
using Union_Formularios_SISV.Forms.Autenticacion;

namespace Union_Formularios_SISV
{
    public partial class Form_Login : Form
    {
        private Timer _errAnim;
        private int _errTargetHeight = 36;
        private int _errStep = 4;
        private int _alphaStep = 18;
        private int _errAlpha = 0;
        private bool _errShowing = false;

        private Timer _errHideTimer;
        private int _errDisplayMs = 2000;

        public event EventHandler<LoginSession> LoginSucceeded;

        public Form_Login()
        {
            InitializeComponent();
            InitErrorUi(); 
        }

        private void Form_Login_Load(object sender, EventArgs e)
        {
            try
            {
                btn_Close.Parent = guna2PictureBox1;
                btn_Close.BackColor = Color.Transparent;
                btn_Minus.Parent = guna2PictureBox1;
                btn_Minus.BackColor = Color.Transparent;

                label10.Parent = guna2PictureBox1;
                label10.BackColor = Color.Transparent;
                label10.Location = new Point(
                    (guna2PictureBox1.Width - label10.Width) / 30,
                    guna2PictureBox1.Height - label10.Height - 25
                );

                txt_pass.UseSystemPasswordChar = true;
                RepositionError();
                LoadRememberMe();
            }
            catch
            {
            }
            try
            {
                if (!SecurityDbBootstrapper.AnyUserExists())
                {
                    using (var f = new Form_CreatePK())
                    {
                        var r = f.ShowDialog(this);
                        if (r != DialogResult.OK)
                        {
                            Application.Exit();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo verificar/crear el primer usuario:\n" + ex.Message,
                    "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
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

        private void btnlogin_Click(object sender, EventArgs e)
        {
            try
            {
                string username = (txt_user.Text ?? "").Trim();
                string password = txt_pass.Text ?? "";

                if (string.IsNullOrWhiteSpace(username))
                    throw new Exception("Ingrese el usuario.");

                if (string.IsNullOrWhiteSpace(password))
                    throw new Exception("Ingrese la contraseña.");

                var user = GetUserByUsername(username);
                if (user == null)
                    throw new Exception("Usuario o contraseña incorrectos.");

                if (!user.Activo)
                    throw new Exception("El usuario está desactivado.");

                bool ok = PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt, user.PasswordIterations);
                if (!ok)
                {
                    ShowError("Usuario o contraseña incorrectos.");
                    return;
                }

                LoginSucceeded?.Invoke(this, new LoginSession
                {
                    UsuarioId = user.UsuarioId,
                    Username = user.Username,
                    RoleId = user.RoleId
                });

                SaveRememberMe(user.Username, password);
                MessageBox.Show("Login correcto", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private UsuarioLoginDto GetUserByUsername(string username)
        {
            string cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new Exception("No se encontró el connectionString 'SISV' en App.config.");

            using (var cn = new SqlConnection(cs))
            using (var cmd = new SqlCommand("dbo.sp_Usuario_GetByUsername", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Username", SqlDbType.VarChar, 100).Value = username;

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;

                    return new UsuarioLoginDto
                    {
                        UsuarioId = rd.GetInt32(rd.GetOrdinal("UsuarioID_Usuarios")),
                        Username = rd.GetString(rd.GetOrdinal("Username_Usuarios")),
                        PasswordHash = (byte[])rd["PasswordHash_Usuarios"],
                        PasswordSalt = rd["PasswordSalt_Usuarios"] == DBNull.Value ? null : (byte[])rd["PasswordSalt_Usuarios"],
                        PasswordIterations = Convert.ToInt32(rd["PasswordIterations_Usuarios"]),
                        Activo = Convert.ToBoolean(rd["Activo_Usuarios"]),
                        RoleId = Convert.ToByte(rd["RoleID_Usuarios"])
                    };
                }
            }
        }

        private class UsuarioLoginDto
        {
            public int UsuarioId { get; set; }
            public string Username { get; set; }
            public byte[] PasswordHash { get; set; }
            public byte[] PasswordSalt { get; set; }
            public int PasswordIterations { get; set; }
            public bool Activo { get; set; }
            public byte RoleId { get; set; }
        }

        private void LoadRememberMe()
        {
            try
            {
                if (RememberMeStore.TryLoad(out string user, out _))
                {
                    Chbox_Recuerdame.Checked = true;
                    txt_user.Text = (user ?? "").Trim();
                    txt_pass.Text = ""; 
                }
                else
                {
                    Chbox_Recuerdame.Checked = false;
                    txt_pass.Text = "";
                }
            }
            catch
            {
                Chbox_Recuerdame.Checked = false;
                txt_pass.Text = "";
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
