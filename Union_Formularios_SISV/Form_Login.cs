using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Recursos_SISV;

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

            Panel_Derecho_Info.UseTransparentBackground = true;
            Panel_Derecho_Info.BackColor = Color.Transparent;
            Panel_Derecho_Info.FillColor = Color.FromArgb(90, 10, 10, 10);

            Panel_Derecho_Info.BorderRadius = 18;
            Panel_Derecho_Info.BorderThickness = 1;
            Panel_Derecho_Info.BorderColor = Color.FromArgb(55, 255, 255, 255);

            label6.Parent = Panel_Derecho_Info;
            label6.BackColor = Color.Transparent;

            label7.Parent = Panel_Derecho_Info;
            label7.BackColor = Color.Transparent;


            label10.Parent = guna2PictureBox1;
            label10.Location = new Point((guna2PictureBox1.Width - label10.Width) / 30, guna2PictureBox1.Height - label10.Height - 25);

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            BeginInvoke((Action)(() =>
            {
                var tintForm = Color.FromArgb(140, 18, 18, 18);

                bool ok = AcrylicHelper.EnableAcrylic(this.Handle, tintForm);
                if (!ok) AcrylicHelper.EnableBlur(this.Handle);

                this.Refresh();
                Panel_Derecho_Info.Refresh();
            }));
        }

    }
}
