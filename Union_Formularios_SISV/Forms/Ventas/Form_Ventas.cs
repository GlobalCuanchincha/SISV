using Dominio_SISV.DTOs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Ventas;

namespace Union_Formularios_SISV.Forms.Ventas
{
    public partial class Form_Ventas : Form
    {
        private const decimal IVA_RATE = 0.15m;

        private bool _initDone;

        private CatalogItemCard _catalogSelectedCard;
        private CatalogItemVM _catalogSelectedItem;
        private List<CatalogItemVM> _catalogAll = new List<CatalogItemVM>();

        private int? _clienteIdActual = null;
        private decimal _descuentoPct = 0m; // 0..100

        private readonly Dictionary<string, FacturaItemCard> _detalleByKey =
            new Dictionary<string, FacturaItemCard>(StringComparer.OrdinalIgnoreCase);

        public Form_Ventas()
        {
            InitializeComponent();
            Load += (s, e) => Ventas_RuntimeInit();
        }

        public void Ventas_RuntimeInit()
        {
            if (_initDone) return;
            _initDone = true;

            // Flows
            if (flowCatalog != null)
            {
                flowCatalog.FlowDirection = FlowDirection.TopDown;
                flowCatalog.WrapContents = false;
                flowCatalog.AutoScroll = true;
                flowCatalog.SizeChanged += (s, e) => FixAllCatalogCardsWidth();
            }

            if (flowDetalleItems != null)
            {
                flowDetalleItems.FlowDirection = FlowDirection.TopDown;
                flowDetalleItems.WrapContents = false;
                flowDetalleItems.AutoScroll = true;
                flowDetalleItems.SizeChanged += (s, e) => FixAllDetalleCardsWidth();
            }

            // Navegación a consulta
            if (btn_Consultar_View != null)
            {
                btn_Consultar_View.Click -= btn_Consultar_View_Click;
                btn_Consultar_View.Click += btn_Consultar_View_Click;
            }

            // Cédula Enter (KeyDown + KeyPress para asegurar)
            if (txt_cedula_VentasFacturas != null)
            {
                txt_cedula_VentasFacturas.KeyDown -= txt_cedula_VentasFacturas_KeyDown;
                txt_cedula_VentasFacturas.KeyDown += txt_cedula_VentasFacturas_KeyDown;

                txt_cedula_VentasFacturas.KeyPress -= txt_cedula_VentasFacturas_KeyPress;
                txt_cedula_VentasFacturas.KeyPress += txt_cedula_VentasFacturas_KeyPress;
            }

            // Buscar / Tipo filtros
            if (txt_buscar_VentasFacturas != null)
            {
                txt_buscar_VentasFacturas.TextChanged -= filtros_Changed;
                txt_buscar_VentasFacturas.TextChanged += filtros_Changed;
            }

            if (cmbox_tipo_VentasFacturas != null)
            {
                cmbox_tipo_VentasFacturas.SelectedIndexChanged -= filtros_Changed;
                cmbox_tipo_VentasFacturas.SelectedIndexChanged += filtros_Changed;

                if (cmbox_tipo_VentasFacturas.Items.Count == 0)
                {
                    cmbox_tipo_VentasFacturas.Items.Add("Todos");
                    cmbox_tipo_VentasFacturas.Items.Add("Productos");
                    cmbox_tipo_VentasFacturas.Items.Add("Servicios");
                    cmbox_tipo_VentasFacturas.SelectedIndex = 0;
                }
            }

            // Botón añadir (valida stock <= 0)
            if (btn_añadir_VentasFacturas != null)
            {
                btn_añadir_VentasFacturas.Click -= btn_añadir_VentasFacturas_Click;
                btn_añadir_VentasFacturas.Click += btn_añadir_VentasFacturas_Click;
            }

            // Descuento: valores [x%]
            InitComboDescuento_Brackets();

            if (btn_aplicar_descuento_VentasFacturas != null)
            {
                btn_aplicar_descuento_VentasFacturas.Click -= btn_aplicar_descuento_VentasFacturas_Click;
                btn_aplicar_descuento_VentasFacturas.Click += btn_aplicar_descuento_VentasFacturas_Click;
            }

            if (btn_Nueva_Factura_VentasFacturas != null)
            {
                btn_Nueva_Factura_VentasFacturas.Click -= btn_Nueva_Factura_VentasFacturas_Click;
                btn_Nueva_Factura_VentasFacturas.Click += btn_Nueva_Factura_VentasFacturas_Click;
            }

            if (btn_Guardar_Factura_VentasFacturas != null)
            {
                btn_Guardar_Factura_VentasFacturas.Click -= btn_Guardar_Factura_VentasFacturas_Click;
                btn_Guardar_Factura_VentasFacturas.Click += btn_Guardar_Factura_VentasFacturas_Click;
            }

            // Cargar catálogo desde procedures (con precio real)
            LoadCatalogFromProcedures();

            // Nuevo estado inicial
            ResetFacturaNueva();
        }

        // ------------------------
        // NAVEGACIÓN
        // ------------------------
        private void btn_Consultar_View_Click(object sender, EventArgs e)
        {
            var main = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
            if (main == null)
            {
                MessageBox.Show("No se encontró el Panel Principal.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            main.OpenChild(new Form_Ventas_Consulta(), "Ventas / Facturación", "Consultar / Anular");
        }

        // ------------------------
        // CÉDULA + ENTER
        // ------------------------
        private void txt_cedula_VentasFacturas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            BuscarClientePorCedula_UI();
        }

        private void txt_cedula_VentasFacturas_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter) return;
            e.Handled = true;
            BuscarClientePorCedula_UI();
        }

        private void BuscarClientePorCedula_UI()
        {
            string ced = (txt_cedula_VentasFacturas.Text ?? "").Trim();

            if (ced.Length != 10 || !ced.All(char.IsDigit))
            {
                MessageBox.Show("Cédula inválida. Debe contener exactamente 10 dígitos numéricos.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var cs = GetConnectionString();
                var repo = new ClienteProcRepository(cs);

                var cli = repo.GetByCedula(ced);

                if (cli == null)
                {
                    _clienteIdActual = cli.ClienteID;

                    if (!_clienteIdActual.HasValue || _clienteIdActual.Value <= 0)
                    {
                        _clienteIdActual = null;
                        MessageBox.Show(
                            "Cliente encontrado, pero no se pudo resolver ClienteID.\n" +
                            "Verifica que el SP crm.usp_Cliente_GetByCedula esté siendo usado y devuelva ClienteID.",
                            "SISV",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );
                        return;
                    }


                    txt_telefono_VentasFacturas.Text = "";
                    txt_nombre_VentasFacturas.Text = "";
                    if (txt_apellido_VentasFacturas != null) txt_apellido_VentasFacturas.Text = "";
                    txt_direccion_VentasFacturas.Text = "";
                    txt_email_VentasFacturas.Text = "";

                    MessageBox.Show(
                        $"No existe un cliente registrado con la cédula {ced}.\n\n" +
                        "Verifica el número o registra al cliente en el módulo de Clientes antes de facturar.",
                        "Cliente no encontrado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                _clienteIdActual = cli.ClienteID;

                txt_telefono_VentasFacturas.Text = cli.Telefono ?? "";
                txt_nombre_VentasFacturas.Text = cli.Nombre ?? "";
                if (txt_apellido_VentasFacturas != null) txt_apellido_VentasFacturas.Text = cli.Apellido ?? "";
                txt_direccion_VentasFacturas.Text = cli.Direccion ?? "";
                txt_email_VentasFacturas.Text = cli.Email ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar el cliente:\n\n" + ex.Message, "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------
        // CATÁLOGO (PROCEDURES)
        // ------------------------
        private void LoadCatalogFromProcedures()
        {
            try
            {
                var cs = GetConnectionString();
                var repo = new CatalogoProcRepository(cs);

                _catalogAll = repo.GetCatalogo();
                RenderCatalogo(ApplyCatalogFilters(_catalogAll));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el catálogo desde la base de datos.\n\n" +
                    "Asegúrate de tener los Stored Procedures de catálogo.\n\n" +
                    ex.Message,
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void filtros_Changed(object sender, EventArgs e)
        {
            RenderCatalogo(ApplyCatalogFilters(_catalogAll));
        }

        private List<CatalogItemVM> ApplyCatalogFilters(List<CatalogItemVM> baseList)
        {
            if (baseList == null) return new List<CatalogItemVM>();

            string q = (txt_buscar_VentasFacturas?.Text ?? "").Trim();
            string tipo = (cmbox_tipo_VentasFacturas?.Text ?? "").Trim();

            IEnumerable<CatalogItemVM> query = baseList.Where(x => x != null && x.Activo);

            if (!string.IsNullOrWhiteSpace(tipo) && !tipo.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            {
                if (tipo.StartsWith("Prod", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => (x.Tipo ?? "").Equals("PRODUCTO", StringComparison.OrdinalIgnoreCase));
                else if (tipo.StartsWith("Serv", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => (x.Tipo ?? "").Equals("SERVICIO", StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                string qq = q.ToUpperInvariant();
                query = query.Where(x =>
                    ((x.Nombre ?? "").ToUpperInvariant().Contains(qq)) ||
                    ((x.Codigo ?? "").ToUpperInvariant().Contains(qq)));
            }

            return query
                .OrderByDescending(x => x.Disponible)
                .ThenBy(x => x.Tipo ?? "")
                .ThenBy(x => x.Nombre ?? "")
                .ToList();
        }

        private void RenderCatalogo(List<CatalogItemVM> items)
        {
            if (flowCatalog == null) return;

            flowCatalog.SuspendLayout();
            try
            {
                flowCatalog.Controls.Clear();

                if (items == null || items.Count == 0)
                    return;

                foreach (var vm in items)
                {
                    var card = new CatalogItemCard();
                    card.Bind(vm);

                    card.ItemClicked -= CatalogCard_ItemClicked;
                    card.ItemClicked += CatalogCard_ItemClicked;

                    flowCatalog.Controls.Add(card);
                    FixCatalogCardWidth(card);
                }
            }
            finally
            {
                flowCatalog.ResumeLayout(true);
            }
        }

        private void CatalogCard_ItemClicked(object sender, CatalogItemVM item)
        {
            var card = sender as CatalogItemCard;
            if (card == null || item == null) return;

            if (_catalogSelectedCard != null && _catalogSelectedCard != card)
                _catalogSelectedCard.SetSelected(false);

            _catalogSelectedCard = card;
            _catalogSelectedItem = item;
            _catalogSelectedCard.SetSelected(true);

            if (lbl_Seleccion_Item_VentasFacturas != null)
                lbl_Seleccion_Item_VentasFacturas.Text = item.Nombre ?? "—";

            if (lbl_Stock_selccionado_VentasFacturas != null)
            {
                if (string.Equals(item.Tipo ?? "", "SERVICIO", StringComparison.OrdinalIgnoreCase))
                    lbl_Stock_selccionado_VentasFacturas.Text = "—";
                else
                    lbl_Stock_selccionado_VentasFacturas.Text = (item.Stock ?? 0).ToString();
            }
        }

        private void FixAllCatalogCardsWidth()
        {
            if (flowCatalog == null) return;
            foreach (Control c in flowCatalog.Controls)
                if (c is CatalogItemCard card) FixCatalogCardWidth(card);
        }

        private void FixCatalogCardWidth(CatalogItemCard card)
        {
            if (flowCatalog == null || card == null) return;
            int w = flowCatalog.ClientSize.Width - card.Margin.Horizontal - 8;
            if (w < 80) w = 80;
            card.Width = w;
        }

        // ------------------------
        // AÑADIR A DETALLE
        // ------------------------
        private void btn_añadir_VentasFacturas_Click(object sender, EventArgs e)
        {
            AddSelectedCatalogToDetalle();
        }

        private void AddSelectedCatalogToDetalle()
        {
            if (flowDetalleItems == null) return;

            if (_catalogSelectedItem == null)
            {
                MessageBox.Show("Selecciona un ítem del catálogo antes de añadir.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool esServicio = string.Equals(_catalogSelectedItem.Tipo ?? "", "SERVICIO", StringComparison.OrdinalIgnoreCase);
            int stock = _catalogSelectedItem.Stock ?? 0;

            if (!esServicio && stock <= 0)
            {
                MessageBox.Show(
                    $"No puedes añadir “{_catalogSelectedItem.Nombre}” porque no tiene stock disponible.\n\n" +
                    "Actualiza el inventario o selecciona otro producto.",
                    "Sin stock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (!_catalogSelectedItem.Disponible)
            {
                MessageBox.Show("El ítem seleccionado no está disponible.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FacturaItemVM vm = FacturaItemVM.FromCatalog(_catalogSelectedItem);
            string key = vm.Key;

            if (_detalleByKey.TryGetValue(key, out var existing) && existing != null)
            {
                existing.IncrementCantidad(1);
                UpdateTotales();
                return;
            }

            var card = new FacturaItemCard();
            card.Bind(vm);

            card.ItemRemoved -= Detalle_ItemRemoved;
            card.ItemRemoved += Detalle_ItemRemoved;

            card.QuantityChanged -= Detalle_QuantityChanged;
            card.QuantityChanged += Detalle_QuantityChanged;

            flowDetalleItems.Controls.Add(card);
            _detalleByKey[key] = card;

            FixDetalleCardWidth(card);
            UpdateTotales();
        }

        private void Detalle_ItemRemoved(object sender, FacturaItemVM item)
        {
            var card = sender as FacturaItemCard;
            if (card == null || item == null) return;

            if (flowDetalleItems.Controls.Contains(card))
                flowDetalleItems.Controls.Remove(card);

            if (_detalleByKey.ContainsKey(item.Key))
                _detalleByKey.Remove(item.Key);

            card.ItemRemoved -= Detalle_ItemRemoved;
            card.QuantityChanged -= Detalle_QuantityChanged;
            card.Dispose();

            UpdateTotales();
        }

        private void Detalle_QuantityChanged(object sender, FacturaItemVM item)
        {
            UpdateTotales();
        }

        private void FixAllDetalleCardsWidth()
        {
            if (flowDetalleItems == null) return;
            foreach (Control c in flowDetalleItems.Controls)
                if (c is FacturaItemCard card) FixDetalleCardWidth(card);
        }

        private void FixDetalleCardWidth(FacturaItemCard card)
        {
            if (flowDetalleItems == null || card == null) return;
            int w = flowDetalleItems.ClientSize.Width - card.Margin.Horizontal - 8;
            if (w < 80) w = 80;
            card.Width = w;
        }

        private List<FacturaItemVM> GetDetalleFacturaItems()
        {
            return _detalleByKey.Values
                .Where(c => c != null && c.Item != null)
                .Select(c => c.Item)
                .ToList();
        }

        // ------------------------
        // DESCUENTO [x%]
        // ------------------------
        private void InitComboDescuento_Brackets()
        {
            if (cmbox_descuento_VentasFacturas == null) return;

            if (cmbox_descuento_VentasFacturas.Items.Count == 0)
            {
                cmbox_descuento_VentasFacturas.Items.Add("[0%]");
                cmbox_descuento_VentasFacturas.Items.Add("[5%]");
                cmbox_descuento_VentasFacturas.Items.Add("[10%]");
                cmbox_descuento_VentasFacturas.Items.Add("[15%]");
                cmbox_descuento_VentasFacturas.Items.Add("[20%]");
                cmbox_descuento_VentasFacturas.Items.Add("[25%]");
                cmbox_descuento_VentasFacturas.Items.Add("[30%]");
                cmbox_descuento_VentasFacturas.SelectedIndex = 0;
            }
        }

        private void btn_aplicar_descuento_VentasFacturas_Click(object sender, EventArgs e)
        {
            string t = (cmbox_descuento_VentasFacturas?.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(t))
            {
                _descuentoPct = 0m;
                UpdateTotales();
                return;
            }

            string clean = t.Replace("[", "").Replace("]", "").Replace("%", "").Trim();

            if (!decimal.TryParse(clean, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal pct))
                decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out pct);

            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;

            _descuentoPct = pct;
            UpdateTotales();
        }

        // ------------------------
        // TOTALES
        // ------------------------
        private void UpdateTotales()
        {
            var items = GetDetalleFacturaItems();

            decimal subtotal = 0m;
            for (int i = 0; i < items.Count; i++)
                subtotal += items[i].Subtotal;

            decimal descuento = Math.Round(subtotal * (_descuentoPct / 100m), 2, MidpointRounding.AwayFromZero);
            if (descuento > subtotal) descuento = subtotal;

            decimal baseImponible = subtotal - descuento;
            decimal iva = Math.Round(baseImponible * IVA_RATE, 2, MidpointRounding.AwayFromZero);
            decimal total = Math.Round(baseImponible + iva, 2, MidpointRounding.AwayFromZero);

            if (lbl_subtotal_VentasFacturas != null) lbl_subtotal_VentasFacturas.Text = subtotal.ToString("C2");
            if (lbl_descuento_VentasFacturas != null) lbl_descuento_VentasFacturas.Text = descuento.ToString("C2");
            if (lbl_IVA_VentasFacturas != null) lbl_IVA_VentasFacturas.Text = iva.ToString("C2");
            if (lbl_Total_VentasFacturas != null) lbl_Total_VentasFacturas.Text = total.ToString("C2");
        }

        private void btn_Nueva_Factura_VentasFacturas_Click(object sender, EventArgs e)
        {
            ResetFacturaNueva();
        }

        private void ResetFacturaNueva()
        {
            if (flowDetalleItems != null) flowDetalleItems.Controls.Clear();
            _detalleByKey.Clear();

            _catalogSelectedItem = null;
            if (_catalogSelectedCard != null) _catalogSelectedCard.SetSelected(false);
            _catalogSelectedCard = null;

            if (lbl_Seleccion_Item_VentasFacturas != null) lbl_Seleccion_Item_VentasFacturas.Text = "—";
            if (lbl_Stock_selccionado_VentasFacturas != null) lbl_Stock_selccionado_VentasFacturas.Text = "—";

            _descuentoPct = 0m;
            if (cmbox_descuento_VentasFacturas != null && cmbox_descuento_VentasFacturas.Items.Count > 0)
                cmbox_descuento_VentasFacturas.SelectedIndex = 0;

            UpdateTotales();

            try
            {
                var cs = GetConnectionString();
                var fact = new FacturaProcRepository(cs);
                string codigo = fact.GetNextCodigoFactura();
                if (!string.IsNullOrWhiteSpace(codigo) && lbl_Codigo_VentasFacturas != null)
                    lbl_Codigo_VentasFacturas.Text = codigo;
            }
            catch
            {
                if (lbl_Codigo_VentasFacturas != null && string.IsNullOrWhiteSpace(lbl_Codigo_VentasFacturas.Text))
                    lbl_Codigo_VentasFacturas.Text = "FAC-0001";
            }
        }

        private void btn_Guardar_Factura_VentasFacturas_Click(object sender, EventArgs e)
        {
            var items = GetDetalleFacturaItems();
            if (items.Count == 0)
            {
                MessageBox.Show("No hay ítems en la factura. Añade productos/servicios antes de guardar.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_clienteIdActual == null)
            {
                MessageBox.Show("Debes cargar un cliente válido por cédula antes de guardar la factura.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subtotal = items.Sum(i => i.Subtotal);
            decimal descuento = Math.Round(subtotal * (_descuentoPct / 100m), 2, MidpointRounding.AwayFromZero);
            if (descuento > subtotal) descuento = subtotal;

            decimal baseImponible = subtotal - descuento;
            decimal iva = Math.Round(baseImponible * IVA_RATE, 2, MidpointRounding.AwayFromZero);
            decimal total = Math.Round(baseImponible + iva, 2, MidpointRounding.AwayFromZero);

            string codigo = null;

            try
            {
                var cs = GetConnectionString();
                var fact = new FacturaProcRepository(cs);

                var res = fact.CrearFactura(_clienteIdActual.Value, codigo, subtotal, descuento, iva, total, items);

                MessageBox.Show($"Factura guardada.\nCódigo: {res.NumeroFactura}\nID: {res.FacturaID}", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                ResetFacturaNueva();
            }
            catch (Exception ex)
            {
                // Mostrar TODO (incluye InnerException)
                MessageBox.Show("No se pudo guardar la factura:\n\n" + ex.ToString(), "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------
        // CONEXIÓN
        // ------------------------
        private static string GetConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs) && ConfigurationManager.ConnectionStrings.Count > 0)
                cs = ConfigurationManager.ConnectionStrings[0]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("No existe connectionString en App.config. Agrega una con nombre 'SISV'.");

            return cs;
        }

        // =====================================================================
        // REPOSITORIOS POR STORED PROCEDURES (SIN SQL EMBEBIDO)
        // =====================================================================

        private sealed class ClienteVM
        {
            public int ClienteID { get; set; }
            public string Cedula { get; set; }
            public string Telefono { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Direccion { get; set; }
            public string Email { get; set; }
        }

        private sealed class ClienteProcRepository
        {
            private readonly string _cs;
            public ClienteProcRepository(string cs) { _cs = cs; }

            public ClienteVM GetByCedula(string cedula)
            {
                var procCandidates = new[]
                {
                    "crm.usp_Cliente_GetByCedula",
                    "crm.usp_Clientes_GetByCedula",
                    "crm.usp_Cliente_BuscarPorCedula",
                    "crm.usp_Cliente_ObtenerPorCedula"
                };

                var paramCandidates = new[]
                {
                    "@Cedula",
                    "@Cedula_Clientes",
                    "@Identificacion",
                    "@pCedula"
                };

                using (var con = new SqlConnection(_cs))
                {
                    con.Open();

                    foreach (var proc in procCandidates)
                    {
                        foreach (var pName in paramCandidates)
                        {
                            try
                            {
                                using (var cmd = new SqlCommand(proc, con))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue(pName, cedula);

                                    using (var rd = cmd.ExecuteReader())
                                    {
                                        if (!rd.Read()) continue;

                                        return new ClienteVM
                                        {
                                            ClienteID = ReadInt(rd, "ClienteID", "ClienteID_Clientes", "Id", "ID"),
                                            Cedula = ReadString(rd, "Cedula_Clientes", "Cedula", "Identificacion"),
                                            Telefono = ReadString(rd, "Telefono_Clientes", "Telefono", "Telefono1"),
                                            Nombre = ReadString(rd, "Nombre_Clientes", "Nombre"),
                                            Apellido = ReadString(rd, "Apellido_Clientes", "Apellido"),
                                            Direccion = ReadString(rd, "Direccion_Clientes", "Direccion"),
                                            Email = ReadString(rd, "Email_Clientes", "Email")
                                        };
                                    }
                                }
                            }
                            catch (SqlException)
                            {
                                // intenta siguiente combinación
                            }
                        }
                    }
                }

                return null;
            }
        }

        private sealed class CatalogoProcRepository
        {
            private readonly string _cs;
            public CatalogoProcRepository(string cs) { _cs = cs; }

            public List<CatalogItemVM> GetCatalogo()
            {
                var productos = GetProductos();
                var servicios = GetServicios();

                var all = new List<CatalogItemVM>();
                if (productos != null) all.AddRange(productos);
                if (servicios != null) all.AddRange(servicios);

                return all;
            }

            private List<CatalogItemVM> GetProductos()
            {
                var procCandidates = new[]
                {
                    "inv.usp_ItemsInventario_Listar",
                    "inv.usp_ItemsInventario_ListarActivos",
                    "inv.usp_Productos_Listar",
                    "inv.usp_Producto_Listar"
                };

                return ExecuteCatalogProc(procCandidates, "PRODUCTO");
            }

            private List<CatalogItemVM> GetServicios()
            {
                var procCandidates = new[]
                {
                    "ops.usp_Servicios_Listar",
                    "ops.usp_Servicio_Listar",
                    "ops.usp_Servicios_Obtener"
                };

                return ExecuteCatalogProc(procCandidates, "SERVICIO");
            }

            private List<CatalogItemVM> ExecuteCatalogProc(string[] procCandidates, string tipoForzado)
            {
                var list = new List<CatalogItemVM>();

                using (var con = new SqlConnection(_cs))
                {
                    con.Open();

                    foreach (var proc in procCandidates)
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(proc, con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                using (var rd = cmd.ExecuteReader())
                                {
                                    while (rd.Read())
                                    {
                                        int id = ReadInt(rd,
                                            "ItemInventarioID", "ItemInventarioID_ItemsInventario",
                                            "ServicioID", "ServicioID_Servicios",
                                            "ProductoID", "ID", "Id");

                                        string codigo = ReadString(rd, "SKU", "Codigo", "CodigoItem", "CodigoServicio");
                                        string nombre = ReadString(rd, "Nombre", "NombreItem", "NombreServicio", "Descripcion");
                                        decimal precio = ReadDecimal(rd, "Precio", "PrecioVenta", "PrecioUnitario", "Precio_Servicios", "Precio_ItemsInventario");
                                        int? stock = ReadNullableInt(rd, "Stock", "StockActual", "Stock_ItemsInventario", "Cantidad");

                                        bool activo = ReadBool(rd, "Activo", "Estado", "IsActive");

                                        if (string.IsNullOrWhiteSpace(codigo))
                                            codigo = tipoForzado == "SERVICIO" ? $"S{id:0000}" : $"PRD-{id:0000}";
                                        if (string.IsNullOrWhiteSpace(nombre))
                                            nombre = tipoForzado == "SERVICIO" ? $"Servicio {id}" : $"Producto {id}";

                                        list.Add(new CatalogItemVM
                                        {
                                            Id = id,
                                            Codigo = codigo,
                                            Nombre = nombre,
                                            Tipo = tipoForzado,
                                            Precio = precio,
                                            Stock = tipoForzado == "SERVICIO" ? (int?)null : (stock ?? 0),
                                            Activo = activo
                                        });
                                    }
                                }
                            }

                            if (list.Count > 0) break; // este proc funcionó
                        }
                        catch (SqlException)
                        {
                            // prueba el siguiente proc
                        }
                    }
                }

                return list;
            }
        }

        private sealed class FacturaProcRepository
        {
            private readonly string _cs;
            public FacturaProcRepository(string cs) { _cs = cs; }

            public string GetNextCodigoFactura()
            {
                var procCandidates = new[]
                {
                    "bill.usp_Factura_GetNextCodigo",
                };

                using (var con = new SqlConnection(_cs))
                {
                    con.Open();

                    foreach (var proc in procCandidates)
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(proc, con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                object v = cmd.ExecuteScalar();
                                string s = v == null ? null : v.ToString();
                                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                            }
                        }
                        catch (SqlException)
                        {
                            // intenta siguiente
                        }
                    }
                }

                throw new InvalidOperationException("No se encontró un SP para generar el código de factura.");
            }

            public (int FacturaID, string NumeroFactura) CrearFactura(
                int clienteId,
                string numeroFactura,
                decimal subtotal,
                decimal descuento,
                decimal iva,
                decimal total,
                List<FacturaItemVM> items)
            {
                const string proc = "bill.usp_Factura_Crear";
                const string tvpType = "bill.TVP_FacturaItem"; // ✅ ÚNICO y correcto

                DataTable tvp = BuildFacturaItemsTvp(items);

                using (var con = new SqlConnection(_cs))
                {
                    con.Open();

                    // Validación rápida: confirma que el TYPE existe en esta BD
                    using (var check = new SqlCommand(
                        "SELECT 1 FROM sys.types WHERE is_table_type=1 AND SCHEMA_NAME(schema_id)=@s AND name=@n", con))
                    {
                        check.Parameters.AddWithValue("@s", "bill");
                        check.Parameters.AddWithValue("@n", "TVP_FacturaItem");
                        if (check.ExecuteScalar() == null)
                            throw new InvalidOperationException("No existe el TYPE bill.TVP_FacturaItem. Revisa connectionString o crea el TYPE.");
                    }

                    try
                    {
                        using (var cmd = new SqlCommand(proc, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandTimeout = 30;

                            cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                            cmd.Parameters.AddWithValue("@NumeroFactura",
                                string.IsNullOrWhiteSpace(numeroFactura) ? (object)DBNull.Value : numeroFactura);

                            cmd.Parameters.AddWithValue("@Subtotal", subtotal);
                            cmd.Parameters.AddWithValue("@Descuento", descuento);
                            cmd.Parameters.AddWithValue("@IVA", iva);
                            cmd.Parameters.AddWithValue("@Total", total);

                            var pItems = cmd.Parameters.Add("@Items", SqlDbType.Structured);
                            pItems.TypeName = tvpType;   // ✅ fijo
                            pItems.Value = tvp;

                            var pFacturaID = new SqlParameter("@FacturaID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(pFacturaID);

                            var pNumOut = new SqlParameter("@NumeroFacturaOut", SqlDbType.VarChar, 20)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(pNumOut);

                            cmd.ExecuteNonQuery();

                            int id = (pFacturaID.Value == DBNull.Value) ? 0 : Convert.ToInt32(pFacturaID.Value);
                            string num = (pNumOut.Value == DBNull.Value) ? (numeroFactura ?? "") : pNumOut.Value.ToString();

                            if (id <= 0)
                                throw new InvalidOperationException("El SP ejecutó pero no devolvió @FacturaID (OUTPUT). Revisa bill.usp_Factura_Crear.");

                            if (string.IsNullOrWhiteSpace(num))
                                num = numeroFactura ?? "";

                            return (id, num);
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new InvalidOperationException(
                            "No se pudo ejecutar bill.usp_Factura_Crear.\n" +
                            $"Intento: SP={proc} | TVP={tvpType}\n\n" +
                            "Detalle SQL:\n" + ex.Message,
                            ex
                        );
                    }
                }
            }

            private static DataTable BuildFacturaItemsTvp(List<FacturaItemVM> items)
            {
                // bill.TVP_FacturaItem:
                // ItemInventarioID (int), ServicioID (int), Cantidad (int), PrecioUnitario (decimal), Subtotal (decimal)
                var dt = new DataTable();
                dt.Columns.Add("ItemInventarioID", typeof(int));
                dt.Columns.Add("ServicioID", typeof(int));
                dt.Columns.Add("Cantidad", typeof(int));
                dt.Columns.Add("PrecioUnitario", typeof(decimal));
                dt.Columns.Add("Subtotal", typeof(decimal));

                foreach (var it in items)
                {
                    int? itemInv = it.ItemInventarioID;
                    int? serv = it.ServicioID;

                    var row = dt.NewRow();
                    row["ItemInventarioID"] = itemInv.HasValue ? (object)itemInv.Value : DBNull.Value;
                    row["ServicioID"] = serv.HasValue ? (object)serv.Value : DBNull.Value;
                    row["Cantidad"] = it.Cantidad;
                    row["PrecioUnitario"] = it.PrecioUnitario;
                    row["Subtotal"] = it.Subtotal;
                    dt.Rows.Add(row);
                }

                return dt;
            }
        }

        // ------------------------
        // LECTORES (sin SQL)
        // ------------------------
        private static bool Has(IDataRecord rd, string name)
        {
            for (int i = 0; i < rd.FieldCount; i++)
                if (string.Equals(rd.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        private static int ReadInt(IDataRecord rd, params string[] names)
        {
            foreach (var n in names)
            {
                if (!Has(rd, n)) continue;
                object v = rd[n];
                if (v == null || v == DBNull.Value) continue;
                if (int.TryParse(v.ToString(), out int x)) return x;
            }
            return 0;
        }

        private static int? ReadNullableInt(IDataRecord rd, params string[] names)
        {
            foreach (var n in names)
            {
                if (!Has(rd, n)) continue;
                object v = rd[n];
                if (v == null || v == DBNull.Value) continue;
                if (int.TryParse(v.ToString(), out int x)) return x;
            }
            return null;
        }

        private static decimal ReadDecimal(IDataRecord rd, params string[] names)
        {
            foreach (var n in names)
            {
                if (!Has(rd, n)) continue;
                object v = rd[n];
                if (v == null || v == DBNull.Value) continue;

                if (v is decimal d) return d;

                if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal x))
                    return x;

                if (decimal.TryParse(v.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                    return x;
            }
            return 0m;
        }

        private static string ReadString(IDataRecord rd, params string[] names)
        {
            foreach (var n in names)
            {
                if (!Has(rd, n)) continue;
                object v = rd[n];
                if (v == null || v == DBNull.Value) continue;
                string s = v.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s;
            }
            return "";
        }

        private static bool ReadBool(IDataRecord rd, params string[] names)
        {
            foreach (var n in names)
            {
                if (!Has(rd, n)) continue;
                object v = rd[n];
                if (v == null || v == DBNull.Value) continue;

                if (v is bool b) return b;

                if (int.TryParse(v.ToString(), out int i))
                    return i != 0;

                var s = (v.ToString() ?? "").Trim().ToUpperInvariant();
                if (s == "ACTIVO" || s == "A" || s == "TRUE" || s == "SI" || s == "SÍ") return true;
                if (s == "INACTIVO" || s == "I" || s == "FALSE" || s == "NO") return false;
            }
            return true;
        }
    }
}
