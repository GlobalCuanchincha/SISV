namespace Union_Formularios_SISV.Forms
{
    partial class Form_Recu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Recu));
            this.btn_Minus = new Guna.UI2.WinForms.Guna2ControlBox();
            this.btn_Close = new Guna.UI2.WinForms.Guna2ControlBox();
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.txt_recuperar = new Guna.UI2.WinForms.Guna2TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_recuperar = new Guna.UI2.WinForms.Guna2Button();
            this.lbl_Resultado = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_Minus
            // 
            this.btn_Minus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Minus.BackColor = System.Drawing.Color.Transparent;
            this.btn_Minus.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btn_Minus.BorderColor = System.Drawing.Color.Transparent;
            this.btn_Minus.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            this.btn_Minus.FillColor = System.Drawing.Color.Transparent;
            this.btn_Minus.IconColor = System.Drawing.Color.Black;
            this.btn_Minus.Location = new System.Drawing.Point(696, 12);
            this.btn_Minus.Name = "btn_Minus";
            this.btn_Minus.Size = new System.Drawing.Size(43, 31);
            this.btn_Minus.TabIndex = 37;
            // 
            // btn_Close
            // 
            this.btn_Close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_Close.BackColor = System.Drawing.Color.Transparent;
            this.btn_Close.FillColor = System.Drawing.Color.Transparent;
            this.btn_Close.IconColor = System.Drawing.Color.Black;
            this.btn_Close.Location = new System.Drawing.Point(745, 12);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(43, 31);
            this.btn_Close.TabIndex = 36;
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.TargetControl = this;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.White;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(204, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(365, 45);
            this.label5.TabIndex = 44;
            this.label5.Text = "Recuerar su contraseña";
            // 
            // txt_recuperar
            // 
            this.txt_recuperar.Animated = true;
            this.txt_recuperar.BorderRadius = 14;
            this.txt_recuperar.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txt_recuperar.DefaultText = "";
            this.txt_recuperar.DisabledState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(208)))), ((int)(((byte)(208)))));
            this.txt_recuperar.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(226)))), ((int)(((byte)(226)))));
            this.txt_recuperar.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txt_recuperar.DisabledState.PlaceholderForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(138)))), ((int)(((byte)(138)))), ((int)(((byte)(138)))));
            this.txt_recuperar.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
            this.txt_recuperar.FocusedState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.txt_recuperar.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txt_recuperar.ForeColor = System.Drawing.Color.Black;
            this.txt_recuperar.HoverState.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.txt_recuperar.IconLeftOffset = new System.Drawing.Point(5, 0);
            this.txt_recuperar.IconLeftSize = new System.Drawing.Size(28, 28);
            this.txt_recuperar.Location = new System.Drawing.Point(193, 150);
            this.txt_recuperar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txt_recuperar.Name = "txt_recuperar";
            this.txt_recuperar.PlaceholderForeColor = System.Drawing.Color.Gray;
            this.txt_recuperar.PlaceholderText = "Ingrese su correo o usuario";
            this.txt_recuperar.SelectedText = "";
            this.txt_recuperar.Size = new System.Drawing.Size(397, 51);
            this.txt_recuperar.TabIndex = 45;
            this.txt_recuperar.TextOffset = new System.Drawing.Point(8, 0);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Location = new System.Drawing.Point(46, 100);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(696, 20);
            this.label1.TabIndex = 46;
            this.label1.Text = "Dentro de este formulario se necesitara su nombre de usuario o correo para recupe" +
    "rar su contraseña";
            // 
            // btn_recuperar
            // 
            this.btn_recuperar.Animated = true;
            this.btn_recuperar.BorderRadius = 4;
            this.btn_recuperar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_recuperar.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.btn_recuperar.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.btn_recuperar.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.btn_recuperar.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.btn_recuperar.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(47)))), ((int)(((byte)(169)))), ((int)(((byte)(90)))));
            this.btn_recuperar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_recuperar.ForeColor = System.Drawing.Color.White;
            this.btn_recuperar.Location = new System.Drawing.Point(500, 208);
            this.btn_recuperar.Name = "btn_recuperar";
            this.btn_recuperar.Size = new System.Drawing.Size(90, 29);
            this.btn_recuperar.TabIndex = 47;
            this.btn_recuperar.Text = "Enviar";
            this.btn_recuperar.Click += new System.EventHandler(this.btn_recuperar_Click);
            // 
            // lbl_Resultado
            // 
            this.lbl_Resultado.AutoSize = true;
            this.lbl_Resultado.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold);
            this.lbl_Resultado.Location = new System.Drawing.Point(46, 268);
            this.lbl_Resultado.Name = "lbl_Resultado";
            this.lbl_Resultado.Size = new System.Drawing.Size(76, 20);
            this.lbl_Resultado.TabIndex = 48;
            this.lbl_Resultado.Text = "Resultado";
            // 
            // Form_Recu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 419);
            this.Controls.Add(this.lbl_Resultado);
            this.Controls.Add(this.btn_recuperar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_recuperar);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btn_Minus);
            this.Controls.Add(this.btn_Close);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_Recu";
            this.Text = "Recuperar contraseña";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Guna.UI2.WinForms.Guna2ControlBox btn_Minus;
        private Guna.UI2.WinForms.Guna2ControlBox btn_Close;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private System.Windows.Forms.Label label5;
        private Guna.UI2.WinForms.Guna2TextBox txt_recuperar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_Resultado;
        private Guna.UI2.WinForms.Guna2Button btn_recuperar;
    }
}