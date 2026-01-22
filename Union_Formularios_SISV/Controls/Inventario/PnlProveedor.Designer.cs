namespace Union_Formularios_SISV.Controls.Clientes
{
    partial class PnlProveedor
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
            this.Panel_DatProveedores = new Guna.UI2.WinForms.Guna2Panel();
            this.lbl_ProveedorMuestra_UC = new System.Windows.Forms.Label();
            this.lbl_DatosProveedor_UC = new System.Windows.Forms.Label();
            this.Panel_DatProveedores.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_DatProveedores
            // 
            this.Panel_DatProveedores.BackColor = System.Drawing.Color.Transparent;
            this.Panel_DatProveedores.BorderRadius = 14;
            this.Panel_DatProveedores.BorderThickness = 2;
            this.Panel_DatProveedores.Controls.Add(this.lbl_DatosProveedor_UC);
            this.Panel_DatProveedores.Controls.Add(this.lbl_ProveedorMuestra_UC);
            this.Panel_DatProveedores.FillColor = System.Drawing.Color.WhiteSmoke;
            this.Panel_DatProveedores.Location = new System.Drawing.Point(0, 0);
            this.Panel_DatProveedores.Name = "Panel_DatProveedores";
            this.Panel_DatProveedores.Size = new System.Drawing.Size(576, 66);
            this.Panel_DatProveedores.TabIndex = 54;
            // 
            // lbl_ProveedorMuestra_UC
            // 
            this.lbl_ProveedorMuestra_UC.AutoSize = true;
            this.lbl_ProveedorMuestra_UC.BackColor = System.Drawing.Color.Transparent;
            this.lbl_ProveedorMuestra_UC.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_ProveedorMuestra_UC.ForeColor = System.Drawing.Color.Black;
            this.lbl_ProveedorMuestra_UC.Location = new System.Drawing.Point(16, 8);
            this.lbl_ProveedorMuestra_UC.Name = "lbl_ProveedorMuestra_UC";
            this.lbl_ProveedorMuestra_UC.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lbl_ProveedorMuestra_UC.Size = new System.Drawing.Size(123, 25);
            this.lbl_ProveedorMuestra_UC.TabIndex = 72;
            this.lbl_ProveedorMuestra_UC.Text = "Proveedor X";
            this.lbl_ProveedorMuestra_UC.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_DatosProveedor_UC
            // 
            this.lbl_DatosProveedor_UC.AutoSize = true;
            this.lbl_DatosProveedor_UC.BackColor = System.Drawing.Color.Transparent;
            this.lbl_DatosProveedor_UC.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_DatosProveedor_UC.Location = new System.Drawing.Point(17, 35);
            this.lbl_DatosProveedor_UC.Name = "lbl_DatosProveedor_UC";
            this.lbl_DatosProveedor_UC.Size = new System.Drawing.Size(295, 21);
            this.lbl_DatosProveedor_UC.TabIndex = 73;
            this.lbl_DatosProveedor_UC.Text = "RUC: 1790012345001 • Tel: 0999999999";
            // 
            // PnlProveedor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.Panel_DatProveedores);
            this.Name = "PnlProveedor";
            this.Size = new System.Drawing.Size(576, 66);
            this.Panel_DatProveedores.ResumeLayout(false);
            this.Panel_DatProveedores.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Guna.UI2.WinForms.Guna2Panel Panel_DatProveedores;
        private System.Windows.Forms.Label lbl_ProveedorMuestra_UC;
        private System.Windows.Forms.Label lbl_DatosProveedor_UC;
    }
}
