namespace Union_Formularios_SISV.Controls.Ventas
{
    partial class FacturaItemCard
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
            this.Panel_Carta = new Guna.UI2.WinForms.Guna2Panel();
            this.nud_Cantidad = new Guna.UI2.WinForms.Guna2NumericUpDown();
            this.lbl_Subtotal = new System.Windows.Forms.Label();
            this.Panel_Chip = new Guna.UI2.WinForms.Guna2Panel();
            this.chip_Quitar = new Guna.UI2.WinForms.Guna2Chip();
            this.lbl_PrecioUnit = new System.Windows.Forms.Label();
            this.lbl_CodigoTipo = new System.Windows.Forms.Label();
            this.lbl_Nom_Componente = new System.Windows.Forms.Label();
            this.Panel_Carta.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Cantidad)).BeginInit();
            this.Panel_Chip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Carta
            // 
            this.Panel_Carta.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Carta.BorderRadius = 10;
            this.Panel_Carta.BorderThickness = 1;
            this.Panel_Carta.Controls.Add(this.nud_Cantidad);
            this.Panel_Carta.Controls.Add(this.lbl_Subtotal);
            this.Panel_Carta.Controls.Add(this.Panel_Chip);
            this.Panel_Carta.Controls.Add(this.lbl_PrecioUnit);
            this.Panel_Carta.Controls.Add(this.lbl_CodigoTipo);
            this.Panel_Carta.Controls.Add(this.lbl_Nom_Componente);
            this.Panel_Carta.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Panel_Carta.Location = new System.Drawing.Point(0, -1);
            this.Panel_Carta.Name = "Panel_Carta";
            this.Panel_Carta.Size = new System.Drawing.Size(708, 68);
            this.Panel_Carta.TabIndex = 55;
            // 
            // nud_Cantidad
            // 
            this.nud_Cantidad.BackColor = System.Drawing.Color.Transparent;
            this.nud_Cantidad.BorderRadius = 15;
            this.nud_Cantidad.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.nud_Cantidad.Font = new System.Drawing.Font("Segoe UI", 11.25F);
            this.nud_Cantidad.Location = new System.Drawing.Point(324, 16);
            this.nud_Cantidad.Name = "nud_Cantidad";
            this.nud_Cantidad.Size = new System.Drawing.Size(76, 34);
            this.nud_Cantidad.TabIndex = 79;
            this.nud_Cantidad.TextOffset = new System.Drawing.Point(5, 0);
            this.nud_Cantidad.UpDownButtonFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(228)))));
            // 
            // lbl_Subtotal
            // 
            this.lbl_Subtotal.AutoSize = true;
            this.lbl_Subtotal.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Subtotal.Location = new System.Drawing.Point(554, 23);
            this.lbl_Subtotal.Name = "lbl_Subtotal";
            this.lbl_Subtotal.Size = new System.Drawing.Size(59, 21);
            this.lbl_Subtotal.TabIndex = 52;
            this.lbl_Subtotal.Text = "$00.00";
            // 
            // Panel_Chip
            // 
            this.Panel_Chip.Controls.Add(this.chip_Quitar);
            this.Panel_Chip.Location = new System.Drawing.Point(649, 13);
            this.Panel_Chip.Name = "Panel_Chip";
            this.Panel_Chip.Size = new System.Drawing.Size(56, 48);
            this.Panel_Chip.TabIndex = 53;
            // 
            // chip_Quitar
            // 
            this.chip_Quitar.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(246)))));
            this.chip_Quitar.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.chip_Quitar.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chip_Quitar.ForeColor = System.Drawing.Color.DarkGray;
            this.chip_Quitar.Location = new System.Drawing.Point(16, 10);
            this.chip_Quitar.Name = "chip_Quitar";
            this.chip_Quitar.Size = new System.Drawing.Size(23, 27);
            this.chip_Quitar.TabIndex = 51;
            // 
            // lbl_PrecioUnit
            // 
            this.lbl_PrecioUnit.AutoSize = true;
            this.lbl_PrecioUnit.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_PrecioUnit.Location = new System.Drawing.Point(445, 23);
            this.lbl_PrecioUnit.Name = "lbl_PrecioUnit";
            this.lbl_PrecioUnit.Size = new System.Drawing.Size(59, 21);
            this.lbl_PrecioUnit.TabIndex = 49;
            this.lbl_PrecioUnit.Text = "$00.00";
            // 
            // lbl_CodigoTipo
            // 
            this.lbl_CodigoTipo.AutoSize = true;
            this.lbl_CodigoTipo.BackColor = System.Drawing.Color.Transparent;
            this.lbl_CodigoTipo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_CodigoTipo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbl_CodigoTipo.Location = new System.Drawing.Point(13, 36);
            this.lbl_CodigoTipo.Name = "lbl_CodigoTipo";
            this.lbl_CodigoTipo.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.lbl_CodigoTipo.Size = new System.Drawing.Size(143, 18);
            this.lbl_CodigoTipo.TabIndex = 48;
            this.lbl_CodigoTipo.Text = "P001 • PRODUCTO";
            this.lbl_CodigoTipo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbl_Nom_Componente
            // 
            this.lbl_Nom_Componente.AutoSize = true;
            this.lbl_Nom_Componente.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Nom_Componente.Location = new System.Drawing.Point(13, 13);
            this.lbl_Nom_Componente.Name = "lbl_Nom_Componente";
            this.lbl_Nom_Componente.Size = new System.Drawing.Size(154, 21);
            this.lbl_Nom_Componente.TabIndex = 0;
            this.lbl_Nom_Componente.Text = "Nom_Componente";
            // 
            // FacturaItemCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Panel_Carta);
            this.Name = "FacturaItemCard";
            this.Size = new System.Drawing.Size(708, 68);
            this.Panel_Carta.ResumeLayout(false);
            this.Panel_Carta.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_Cantidad)).EndInit();
            this.Panel_Chip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel Panel_Carta;
        private Guna.UI2.WinForms.Guna2NumericUpDown nud_Cantidad;
        private System.Windows.Forms.Label lbl_Subtotal;
        private Guna.UI2.WinForms.Guna2Panel Panel_Chip;
        private Guna.UI2.WinForms.Guna2Chip chip_Quitar;
        private System.Windows.Forms.Label lbl_PrecioUnit;
        private System.Windows.Forms.Label lbl_CodigoTipo;
        private System.Windows.Forms.Label lbl_Nom_Componente;
    }
}
