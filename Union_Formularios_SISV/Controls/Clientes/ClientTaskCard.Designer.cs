namespace Union_Formularios_SISV.Forms.Clientes
{
    partial class ClientTaskCard
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.Panel_Carta_UserControl = new Guna.UI2.WinForms.Guna2Panel();
            this.Panel_Estado_UserControl = new Guna.UI2.WinForms.Guna2GradientPanel();
            this.lbl_Point_UserControl = new System.Windows.Forms.Label();
            this.lbl_Estado_UserControl = new System.Windows.Forms.Label();
            this.lbl_Telefono_UserControl = new System.Windows.Forms.Label();
            this.lbl_Correo_UserControl = new System.Windows.Forms.Label();
            this.lbl_Nom_UserControl = new System.Windows.Forms.Label();
            this.lbl_Cedula_UserControl = new System.Windows.Forms.Label();
            this.Panel_Carta_UserControl.SuspendLayout();
            this.Panel_Estado_UserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Carta_UserControl
            // 
            this.Panel_Carta_UserControl.Controls.Add(this.Panel_Estado_UserControl);
            this.Panel_Carta_UserControl.Controls.Add(this.lbl_Telefono_UserControl);
            this.Panel_Carta_UserControl.Controls.Add(this.lbl_Correo_UserControl);
            this.Panel_Carta_UserControl.Controls.Add(this.lbl_Nom_UserControl);
            this.Panel_Carta_UserControl.Controls.Add(this.lbl_Cedula_UserControl);
            this.Panel_Carta_UserControl.Location = new System.Drawing.Point(0, 0);
            this.Panel_Carta_UserControl.Name = "Panel_Carta_UserControl";
            this.Panel_Carta_UserControl.Size = new System.Drawing.Size(729, 66);
            this.Panel_Carta_UserControl.TabIndex = 0;
            // 
            // Panel_Estado_UserControl
            // 
            this.Panel_Estado_UserControl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Panel_Estado_UserControl.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Estado_UserControl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Panel_Estado_UserControl.BorderColor = System.Drawing.Color.Silver;
            this.Panel_Estado_UserControl.BorderRadius = 10;
            this.Panel_Estado_UserControl.BorderStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            this.Panel_Estado_UserControl.BorderThickness = 1;
            this.Panel_Estado_UserControl.Controls.Add(this.lbl_Point_UserControl);
            this.Panel_Estado_UserControl.Controls.Add(this.lbl_Estado_UserControl);
            this.Panel_Estado_UserControl.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(244)))), ((int)(((byte)(238)))));
            this.Panel_Estado_UserControl.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(244)))), ((int)(((byte)(238)))));
            this.Panel_Estado_UserControl.Location = new System.Drawing.Point(596, 17);
            this.Panel_Estado_UserControl.Name = "Panel_Estado_UserControl";
            this.Panel_Estado_UserControl.ShadowDecoration.BorderRadius = 20;
            this.Panel_Estado_UserControl.ShadowDecoration.Color = System.Drawing.Color.Gray;
            this.Panel_Estado_UserControl.ShadowDecoration.Shadow = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.Panel_Estado_UserControl.Size = new System.Drawing.Size(85, 30);
            this.Panel_Estado_UserControl.TabIndex = 68;
            // 
            // lbl_Point_UserControl
            // 
            this.lbl_Point_UserControl.AutoSize = true;
            this.lbl_Point_UserControl.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Point_UserControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Point_UserControl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(187)))), ((int)(((byte)(132)))));
            this.lbl_Point_UserControl.Location = new System.Drawing.Point(8, 5);
            this.lbl_Point_UserControl.Name = "lbl_Point_UserControl";
            this.lbl_Point_UserControl.Size = new System.Drawing.Size(17, 18);
            this.lbl_Point_UserControl.TabIndex = 66;
            this.lbl_Point_UserControl.Text = "●";
            this.lbl_Point_UserControl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_Estado_UserControl
            // 
            this.lbl_Estado_UserControl.AutoSize = true;
            this.lbl_Estado_UserControl.BackColor = System.Drawing.Color.Transparent;
            this.lbl_Estado_UserControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Estado_UserControl.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(187)))), ((int)(((byte)(132)))));
            this.lbl_Estado_UserControl.Location = new System.Drawing.Point(26, 6);
            this.lbl_Estado_UserControl.Name = "lbl_Estado_UserControl";
            this.lbl_Estado_UserControl.Size = new System.Drawing.Size(48, 18);
            this.lbl_Estado_UserControl.TabIndex = 65;
            this.lbl_Estado_UserControl.Text = "Activo";
            this.lbl_Estado_UserControl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_Telefono_UserControl
            // 
            this.lbl_Telefono_UserControl.AutoSize = true;
            this.lbl_Telefono_UserControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.lbl_Telefono_UserControl.Location = new System.Drawing.Point(466, 24);
            this.lbl_Telefono_UserControl.Name = "lbl_Telefono_UserControl";
            this.lbl_Telefono_UserControl.Size = new System.Drawing.Size(88, 18);
            this.lbl_Telefono_UserControl.TabIndex = 3;
            this.lbl_Telefono_UserControl.Text = "1234567890";
            // 
            // lbl_Correo_UserControl
            // 
            this.lbl_Correo_UserControl.AutoSize = true;
            this.lbl_Correo_UserControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.lbl_Correo_UserControl.Location = new System.Drawing.Point(275, 24);
            this.lbl_Correo_UserControl.Name = "lbl_Correo_UserControl";
            this.lbl_Correo_UserControl.Size = new System.Drawing.Size(144, 18);
            this.lbl_Correo_UserControl.TabIndex = 2;
            this.lbl_Correo_UserControl.Text = "correo@gmail.copm";
            // 
            // lbl_Nom_UserControl
            // 
            this.lbl_Nom_UserControl.AutoSize = true;
            this.lbl_Nom_UserControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.lbl_Nom_UserControl.Location = new System.Drawing.Point(126, 23);
            this.lbl_Nom_UserControl.Name = "lbl_Nom_UserControl";
            this.lbl_Nom_UserControl.Size = new System.Drawing.Size(94, 18);
            this.lbl_Nom_UserControl.TabIndex = 1;
            this.lbl_Nom_UserControl.Text = "Nom_Cliente";
            // 
            // lbl_Cedula_UserControl
            // 
            this.lbl_Cedula_UserControl.AutoSize = true;
            this.lbl_Cedula_UserControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.lbl_Cedula_UserControl.Location = new System.Drawing.Point(14, 24);
            this.lbl_Cedula_UserControl.Name = "lbl_Cedula_UserControl";
            this.lbl_Cedula_UserControl.Size = new System.Drawing.Size(88, 18);
            this.lbl_Cedula_UserControl.TabIndex = 0;
            this.lbl_Cedula_UserControl.Text = "1209387251";
            // 
            // ClientTaskCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Panel_Carta_UserControl);
            this.Name = "ClientTaskCard";
            this.Size = new System.Drawing.Size(729, 66);
            this.Panel_Carta_UserControl.ResumeLayout(false);
            this.Panel_Carta_UserControl.PerformLayout();
            this.Panel_Estado_UserControl.ResumeLayout(false);
            this.Panel_Estado_UserControl.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel Panel_Carta_UserControl;
        private System.Windows.Forms.Label lbl_Cedula_UserControl;
        private System.Windows.Forms.Label lbl_Telefono_UserControl;
        private System.Windows.Forms.Label lbl_Correo_UserControl;
        private System.Windows.Forms.Label lbl_Nom_UserControl;
        private Guna.UI2.WinForms.Guna2GradientPanel Panel_Estado_UserControl;
        private System.Windows.Forms.Label lbl_Point_UserControl;
        private System.Windows.Forms.Label lbl_Estado_UserControl;
    }
}
