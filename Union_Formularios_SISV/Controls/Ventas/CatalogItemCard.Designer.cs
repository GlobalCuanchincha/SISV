namespace Union_Formularios_SISV.Controls.Ventas
{
    partial class CatalogItemCard
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
            this.Panel_Chip = new Guna.UI2.WinForms.Guna2Panel();
            this.chip_Estado = new Guna.UI2.WinForms.Guna2Chip();
            this.lbl_Stock = new System.Windows.Forms.Label();
            this.lbl_Precio = new System.Windows.Forms.Label();
            this.lbl_CodigoTipo = new System.Windows.Forms.Label();
            this.lbl_Nom_Componente = new System.Windows.Forms.Label();
            this.Panel_Carta.SuspendLayout();
            this.Panel_Chip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Carta
            // 
            this.Panel_Carta.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Carta.BorderRadius = 10;
            this.Panel_Carta.BorderThickness = 1;
            this.Panel_Carta.Controls.Add(this.Panel_Chip);
            this.Panel_Carta.Controls.Add(this.lbl_Stock);
            this.Panel_Carta.Controls.Add(this.lbl_Precio);
            this.Panel_Carta.Controls.Add(this.lbl_CodigoTipo);
            this.Panel_Carta.Controls.Add(this.lbl_Nom_Componente);
            this.Panel_Carta.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Panel_Carta.Location = new System.Drawing.Point(1, 1);
            this.Panel_Carta.Name = "Panel_Carta";
            this.Panel_Carta.Size = new System.Drawing.Size(708, 66);
            this.Panel_Carta.TabIndex = 2;
            // 
            // Panel_Chip
            // 
            this.Panel_Chip.Controls.Add(this.chip_Estado);
            this.Panel_Chip.Location = new System.Drawing.Point(547, 11);
            this.Panel_Chip.Name = "Panel_Chip";
            this.Panel_Chip.Size = new System.Drawing.Size(158, 48);
            this.Panel_Chip.TabIndex = 53;
            // 
            // chip_Estado
            // 
            this.chip_Estado.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(240)))), ((int)(((byte)(246)))));
            this.chip_Estado.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(250)))), ((int)(((byte)(251)))));
            this.chip_Estado.Font = new System.Drawing.Font("Segoe UI Semibold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chip_Estado.ForeColor = System.Drawing.Color.DarkGray;
            this.chip_Estado.Location = new System.Drawing.Point(20, 6);
            this.chip_Estado.Name = "chip_Estado";
            this.chip_Estado.Size = new System.Drawing.Size(125, 31);
            this.chip_Estado.TabIndex = 51;
            // 
            // lbl_Stock
            // 
            this.lbl_Stock.AutoSize = true;
            this.lbl_Stock.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Stock.Location = new System.Drawing.Point(480, 24);
            this.lbl_Stock.Name = "lbl_Stock";
            this.lbl_Stock.Size = new System.Drawing.Size(31, 21);
            this.lbl_Stock.TabIndex = 50;
            this.lbl_Stock.Text = "(0)";
            // 
            // lbl_Precio
            // 
            this.lbl_Precio.AutoSize = true;
            this.lbl_Precio.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_Precio.Location = new System.Drawing.Point(366, 24);
            this.lbl_Precio.Name = "lbl_Precio";
            this.lbl_Precio.Size = new System.Drawing.Size(59, 21);
            this.lbl_Precio.TabIndex = 49;
            this.lbl_Precio.Text = "$00.00";
            // 
            // lbl_CodigoTipo
            // 
            this.lbl_CodigoTipo.AutoSize = true;
            this.lbl_CodigoTipo.BackColor = System.Drawing.Color.Transparent;
            this.lbl_CodigoTipo.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_CodigoTipo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbl_CodigoTipo.Location = new System.Drawing.Point(13, 37);
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
            this.lbl_Nom_Componente.Location = new System.Drawing.Point(13, 14);
            this.lbl_Nom_Componente.Name = "lbl_Nom_Componente";
            this.lbl_Nom_Componente.Size = new System.Drawing.Size(154, 21);
            this.lbl_Nom_Componente.TabIndex = 0;
            this.lbl_Nom_Componente.Text = "Nom_Componente";
            // 
            // CatalogItemCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Panel_Carta);
            this.Name = "CatalogItemCard";
            this.Size = new System.Drawing.Size(710, 67);
            this.Panel_Carta.ResumeLayout(false);
            this.Panel_Carta.PerformLayout();
            this.Panel_Chip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel Panel_Carta;
        private Guna.UI2.WinForms.Guna2Panel Panel_Chip;
        private Guna.UI2.WinForms.Guna2Chip chip_Estado;
        private System.Windows.Forms.Label lbl_Stock;
        private System.Windows.Forms.Label lbl_Precio;
        private System.Windows.Forms.Label lbl_CodigoTipo;
        private System.Windows.Forms.Label lbl_Nom_Componente;
    }
}
