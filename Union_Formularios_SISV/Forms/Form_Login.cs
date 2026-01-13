using Datos_Acceso.Common;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos_Acceso.Common;
using Dominio_SISV.Services;
using System.Windows.Forms;

namespace Union_Formularios_SISV
{
    public partial class Form_Login : Form
    {
        public Form_Login()
        {
            InitializeComponent();
        }

        private void Form_Login_Load(object sender, EventArgs e)
        {
            txt_pass.UseSystemPasswordChar = true;

            label10.Parent = guna2PictureBox1;
            label10.BackColor = Color.Transparent;
            label10.Location = new Point((guna2PictureBox1.Width - label10.Width) / 30, guna2PictureBox1.Height - label10.Height - 25);

        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            var auth = new AuthService();
            var res = auth.Login(txt_user.Text, txt_pass.Text);

            if (!res.Ok)
            {
                MessageBox.Show(res.Error, "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Login correcto. Rol: " + res.RoleID);
        }
    }
}
