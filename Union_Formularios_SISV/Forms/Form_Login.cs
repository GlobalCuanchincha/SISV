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

            try
            {
                var dt = SqlExecutor.ExecuteDataTable("dbo.sp_TestConnection");
                MessageBox.Show($"Conexión: {dt.Rows[0]["Estado"]}\nFecha: {dt.Rows[0]["Fecha"]}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error conectando a la BD:\n" + ex.Message);
            }

        }
    }
}
