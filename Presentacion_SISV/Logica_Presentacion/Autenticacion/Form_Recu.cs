using Dominio_SISV.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Recu : Form
    {
        public Form_Recu()
        {
            InitializeComponent();
        }
        private void btn_recuperar_Click(object sender, EventArgs e)
        {
            try
            {
                btn_recuperar.Enabled = false;
                lbl_Resultado.Text = "Enviando...";

                var svc = new PasswordRecoveryService();
                string msg = svc.SendTemporaryPassword(txt_recuperar.Text);

                lbl_Resultado.Text = msg;
            }
            catch (Exception ex)
            {
                lbl_Resultado.Text = "Error: " + ex.Message;
            }
            finally
            {
                btn_recuperar.Enabled = true;
            }
        }
    }


}
