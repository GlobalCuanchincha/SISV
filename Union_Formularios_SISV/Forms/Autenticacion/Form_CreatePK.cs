using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms.Autenticacion
{
    public partial class Form_CreatePK : Form
    {
        public Form_CreatePK()
        {
            InitializeComponent();
        }
        private void Form_CreatePK_Load(object sender, EventArgs e)
        {
            txt_pass.UseSystemPasswordChar = true;
            txt_confpass.UseSystemPasswordChar = true;
        }
        private void btn_createuser_Click(object sender, EventArgs e)
        {
            try
            {
                string user = (txt_user.Text ?? "").Trim();
                string email = (txt_email.Text ?? "").Trim();
                string name = (txt_name.Text ?? "").Trim();
                string lastname = (txt_lastname.Text ?? "").Trim();
                string pass = txt_pass.Text ?? "";
                string conf = txt_confpass.Text ?? "";

                if (string.IsNullOrWhiteSpace(user)) throw new Exception("El usuario es obligatorio.");
                if (user.Length < 4) throw new Exception("El usuario debe tener mínimo 4 caracteres.");

                if (string.IsNullOrWhiteSpace(email)) throw new Exception("El email es obligatorio.");
                if (!IsValidEmail(email)) throw new Exception("El email no tiene un formato válido.");

                if (string.IsNullOrWhiteSpace(name)) throw new Exception("El nombre es obligatorio.");
                if (string.IsNullOrWhiteSpace(lastname)) throw new Exception("El apellido es obligatorio.");

                if (string.IsNullOrEmpty(pass) || pass.Length < 6)
                    throw new Exception("La contraseña debe tener mínimo 6 caracteres.");

                if (pass != conf) throw new Exception("La confirmación de contraseña no coincide.");

                SecurityDbBootstrapper.CreateFirstUser_SuperAdmin(
                    username: user,
                    email: email,
                    names: name,
                    lastnames: lastname,
                    plainPassword: pass
                );

                MessageBox.Show(
                    "Usuario SuperAdministrador creado correctamente.\nYa puedes iniciar sesión.",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }
    }
}
