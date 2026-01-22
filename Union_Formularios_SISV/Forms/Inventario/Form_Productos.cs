using Datos_Acceso.Repositories;
using Dominio_SISV.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Clientes;
using Union_Formularios_SISV.Controls.Inventario;

namespace Union_Formularios_SISV.Forms.Inventario
{
    public partial class Form_Productos : Form
    {
        private readonly SISVInventarioRepository _repo;
        private readonly int _usuarioId;

        private List<CategoriaInventarioVM> _categorias = new List<CategoriaInventarioVM>();
        private ProductoCardVM _selected;
        private ProveedorPickVM _selectedProveedor;

        private readonly Timer _debounce = new Timer { Interval = 250 };

        private sealed class EstadoFiltroItem
        {
            public string Text { get; set; }
            public bool? Value { get; set; }
            public override string ToString() => Text;
        }

        public Form_Productos(int usuarioId = 1)
        {
            InitializeComponent();

            _usuarioId = usuarioId;
            _repo = new SISVInventarioRepository();

            _debounce.Tick += (_, __) => { _debounce.Stop(); Buscar(); };

            // Filtros
            txt_Buscador_Productos.TextChanged += (_, __) => { _debounce.Stop(); _debounce.Start(); };
            cmbox_Categoria_Productos.SelectedIndexChanged += (_, __) => Buscar();
            cmbox_Estado_Producto.SelectedIndexChanged += (_, __) => Buscar();

            // Acciones
            btn_ElegirProveedor_Producto.Click += (_, __) => ElegirProveedor();

            // Init
            Load += (_, __) => Inicializar();
        }

        private void Inicializar()
        {
            try
            {
                // Estado filtro (arriba)
                cmbox_Estado_Producto.Items.Clear();
                cmbox_Estado_Producto.Items.Add(new EstadoFiltroItem { Text = "Todos", Value = null });
                cmbox_Estado_Producto.Items.Add(new EstadoFiltroItem { Text = "Activos", Value = true });
                cmbox_Estado_Producto.Items.Add(new EstadoFiltroItem { Text = "Inactivos", Value = false });
                cmbox_Estado_Producto.SelectedIndex = 1; // por defecto Activos

                // Estado registro (derecha)
                cmbox_EstadoReg_Producto.Items.Clear();
                cmbox_EstadoReg_Producto.Items.Add("Activo");
                cmbox_EstadoReg_Producto.Items.Add("Inactivo");
                cmbox_EstadoReg_Producto.SelectedIndex = 0;


                var catFiltro = new List<CategoriaInventarioVM> { new CategoriaInventarioVM { CategoriaId = 0, Nombre = "Todas" } };
                catFiltro.AddRange(_categorias);

                cmbox_Categoria_Productos.DataSource = catFiltro;
                cmbox_Categoria_Productos.DisplayMember = "Nombre";
                cmbox_Categoria_Productos.ValueMember = "CategoriaId";
                cmbox_Categoria_Productos.SelectedIndex = 0;

                // Combo de categoría del registro (en tu designer se llama guna2ComboBox1)
                guna2ComboBox1.DataSource = _categorias.ToList();
                guna2ComboBox1.DisplayMember = "Nombre";
                guna2ComboBox1.ValueMember = "CategoriaId";
                if (_categorias.Count > 0) guna2ComboBox1.SelectedIndex = 0;

                Nuevo();
                Buscar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool? GetSoloActivos()
        {
            if (cmbox_Estado_Producto.SelectedItem is EstadoFiltroItem it)
                return it.Value;
            return true;
        }

        private int? GetCategoriaFiltro()
        {
            if (cmbox_Categoria_Productos.SelectedItem is CategoriaInventarioVM c)
                return c.CategoriaId <= 0 ? (int?)null : c.CategoriaId;
            return null;
        }

        private void Buscar()
        {
            try
            {
                var buscar = txt_Buscador_Productos.Text?.Trim();
                var catId = GetCategoriaFiltro();
                var soloActivos = GetSoloActivos();


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderProductos(List<ProductoCardVM> data)
        {
            flowProductCard.SuspendLayout();
            flowProductCard.Controls.Clear();

            foreach (var p in data)
            {
                var card = new ProductTaskCard();
                card.Bind(p);

                flowProductCard.Controls.Add(card);
            }

            flowProductCard.ResumeLayout();
        }

        private void Seleccionar(ProductoCardVM p)
        {
            _selected = p;
            _selectedProveedor = null;

            txt_Codigo_Producto.Text = p.Codigo ?? "";
            txt_Nombre_Producto.Text = p.Nombre ?? "";
            txt_Proveedor_Producto.Text = p.Proveedor ?? "";
            txt_Descripcion_Producto.Text = p.Descripcion ?? "";

            nuc_Stock_Producto.Value = p.Stock;
            nuc_StockMinimo_Producto.Value = p.StockMinimo;
            nuc_Precio_Producto.Value = p.Precio;
            nuc_Costo_Producto.Value = p.Costo ?? 0;

            // categoría (registro)
            var idx = _categorias.FindIndex(x => x.CategoriaId == p.CategoriaId);
            if (idx >= 0) guna2ComboBox1.SelectedIndex = idx;

            // estado (registro)
            cmbox_EstadoReg_Producto.SelectedIndex = p.Activo ? 0 : 1;

            btn_Desactivar_Producto.Text = p.Activo ? "Desactivar" : "Activar";
        }

        private void Nuevo()
        {
            _selected = null;
            _selectedProveedor = null;

            txt_Codigo_Producto.Text = "";
            txt_Nombre_Producto.Text = "";
            txt_Proveedor_Producto.Text = "";
            txt_Descripcion_Producto.Text = "";

            nuc_Stock_Producto.Value = 0;
            nuc_StockMinimo_Producto.Value = 0;
            nuc_Precio_Producto.Value = 0;
            nuc_Costo_Producto.Value = 0;

            if (_categorias.Count > 0) guna2ComboBox1.SelectedIndex = 0;
            cmbox_EstadoReg_Producto.SelectedIndex = 0;

            btn_Desactivar_Producto.Text = "Desactivar";
        }

        private void ElegirProveedor()
        {
            try
            {
                using (var dlg = new Control_Proveedores_UC(_repo))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedProveedor != null)
                    {
                        _selectedProveedor = dlg.SelectedProveedor;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
    }
}
