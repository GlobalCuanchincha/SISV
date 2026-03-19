using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Union_Formularios_SISV.Logica_Presentacion;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Resumen : Form, IResumenView
    {
        private readonly ResumenPresenter _presenter;

        public Form_Resumen()
        {
            InitializeComponent();
            _presenter = new ResumenPresenter(this);

            Load += async (_, __) =>
            {
                PrepararGrids();
                await _presenter.CargarAsync();
            };
        }

        public void SetVentasHoy(decimal value)
        {
            lbl_VentasH.Text = value.ToString("C2");
        }

        public void SetTotalFacturasHoy(int value)
        {
            lbl_Total_Facturas.Text = value.ToString();
        }

        public void SetOrdenesPendientes(int value)
        {
            lbl_Ordenes_Pendientes.Text = value.ToString();
        }

        public void SetOrdenesHoy(int value)
        {
            lbl_Ordenes_Hoy.Text = value.ToString();
        }

        public void SetStockBajo(int value)
        {
            lbl_StockBajo.Text = value.ToString();
        }

        public void SetClientesNuevos7(int value)
        {
            lbl_Clientes_Nuevos7.Text = value.ToString();
        }

        public void SetPromedioIngresos7(decimal value)
        {
            lbl_Promedio_Ingresos7.Text = "Promedio: " + value.ToString("C2");
        }

        public void BindIngresos(DataTable dt)
        {
            dgv_Ingresos_Promedio.DataSource = dt;

            if (dgv_Ingresos_Promedio.Columns.Contains("Fecha"))
                dgv_Ingresos_Promedio.Columns["Fecha"].HeaderText = "Fecha";

            if (dgv_Ingresos_Promedio.Columns.Contains("Ingreso"))
            {
                dgv_Ingresos_Promedio.Columns["Ingreso"].HeaderText = "Ingreso";
                dgv_Ingresos_Promedio.Columns["Ingreso"].DefaultCellStyle.Format = "C2";
            }
        }

        public void BindStockBajo(DataTable dt)
        {
            dgv_Stock_Bajo.DataSource = dt;

            if (dgv_Stock_Bajo.Columns.Contains("Codigo"))
                dgv_Stock_Bajo.Columns["Codigo"].HeaderText = "Código";

            if (dgv_Stock_Bajo.Columns.Contains("Nombre"))
                dgv_Stock_Bajo.Columns["Nombre"].HeaderText = "Producto";

            if (dgv_Stock_Bajo.Columns.Contains("Stock"))
                dgv_Stock_Bajo.Columns["Stock"].HeaderText = "Stock";

            if (dgv_Stock_Bajo.Columns.Contains("StockMinimo"))
                dgv_Stock_Bajo.Columns["StockMinimo"].HeaderText = "Stock mínimo";

            if (dgv_Stock_Bajo.Columns.Contains("Faltante"))
                dgv_Stock_Bajo.Columns["Faltante"].HeaderText = "Faltante";
        }

        public void BindActividadReciente(DataTable dt)
        {
            dgv_Actividadreciente.DataSource = dt;

            if (dgv_Actividadreciente.Columns.Contains("Fecha"))
                dgv_Actividadreciente.Columns["Fecha"].HeaderText = "Fecha";

            if (dgv_Actividadreciente.Columns.Contains("Tipo"))
                dgv_Actividadreciente.Columns["Tipo"].HeaderText = "Tipo";

            if (dgv_Actividadreciente.Columns.Contains("Descripcion"))
                dgv_Actividadreciente.Columns["Descripcion"].HeaderText = "Descripción";
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void PrepararGrids()
        {
            PrepararGrid(dgv_Ingresos_Promedio);
            PrepararGrid(dgv_Stock_Bajo);
            PrepararGrid(dgv_Actividadreciente);
        }

        private static void PrepararGrid(DataGridView dgv)
        {
            if (dgv == null) return;

            dgv.AutoGenerateColumns = true;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
        }
    }
}