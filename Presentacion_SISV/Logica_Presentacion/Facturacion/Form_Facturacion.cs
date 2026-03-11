using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs;
using Dominio_SISV.DTOs.Facturacion;
using Dominio_SISV.Services.Facturacion;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Ventas;

namespace Union_Formularios_SISV.Forms.Ventas
{
    public partial class Form_Facturacion : Form
    {
        private const decimal IVA_RATE = 0.15m;

        private bool _initDone;

        private CatalogItemCard _catalogSelectedCard;
        private CatalogItemVM _catalogSelectedItem;
        private List<CatalogItemVM> _catalogAll = new List<CatalogItemVM>();

        private int? _clienteIdActual = null;
        private decimal _descuentoPct = 0m;

        private readonly Dictionary<string, FacturaItemCard> _detalleByKey =
            new Dictionary<string, FacturaItemCard>(StringComparer.OrdinalIgnoreCase);

        private IFacturacionService _facturacionService;

        public Form_Facturacion() : this(null)
        {
        }

        public Form_Facturacion(IFacturacionService facturacionService)
        {
            InitializeComponent();
            _facturacionService = facturacionService;
            Load += (s, e) => Ventas_RuntimeInit();
        }

        private IFacturacionService GetFacturacionService()
        {
            if (_facturacionService == null)
                _facturacionService = new FacturacionService(GetConnectionString());

            return _facturacionService;
        }

        public void Ventas_RuntimeInit()
        {
            if (_initDone) return;
            _initDone = true;

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

            if (btn_Consultar_View != null)
            {
                btn_Consultar_View.Click -= btn_Consultar_View_Click;
                btn_Consultar_View.Click += btn_Consultar_View_Click;
            }

            if (txt_cedula_VentasFacturas != null)
            {
                txt_cedula_VentasFacturas.KeyDown -= txt_cedula_VentasFacturas_KeyDown;
                txt_cedula_VentasFacturas.KeyDown += txt_cedula_VentasFacturas_KeyDown;

                txt_cedula_VentasFacturas.KeyPress -= txt_cedula_VentasFacturas_KeyPress;
                txt_cedula_VentasFacturas.KeyPress += txt_cedula_VentasFacturas_KeyPress;
            }

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

            if (cmbox_TipoPago_Factura != null)
            {
                cmbox_TipoPago_Factura.DropDownStyle = ComboBoxStyle.DropDownList;
            }

            if (btn_añadir_VentasFacturas != null)
            {
                btn_añadir_VentasFacturas.Click -= btn_añadir_VentasFacturas_Click;
                btn_añadir_VentasFacturas.Click += btn_añadir_VentasFacturas_Click;
            }

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

            LoadCatalogFromProcedures();
            LoadTiposPago();
            ResetFacturaNueva();
        }

        private void btn_Consultar_View_Click(object sender, EventArgs e)
        {
            var main = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
            if (main == null)
            {
                MessageBox.Show("No se encontró el Panel Principal.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            main.OpenChild(new Form_Facturacion_Consulta(), "Ventas / Facturación", "Consultar / Anular");
        }

        private void txt_cedula_VentasFacturas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            BuscarClientePorCedula_UI();
        }

        private void txt_cedula_VentasFacturas_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
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
                var cli = GetFacturacionService().BuscarClientePorCedula(ced);

                if (cli == null)
                {
                    _clienteIdActual = null;

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

                _clienteIdActual = cli.ClienteID > 0 ? (int?)cli.ClienteID : null;

                if (!_clienteIdActual.HasValue)
                {
                    MessageBox.Show(
                        "Cliente encontrado, pero no se pudo resolver ClienteID.",
                        "SISV",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

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

        private void LoadCatalogFromProcedures()
        {
            try
            {
                _catalogAll = GetFacturacionService().ObtenerCatalogo() ?? new List<CatalogItemVM>();
                RenderCatalogo(ApplyCatalogFilters(_catalogAll));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el catálogo desde la base de datos.\n\n" + ex.Message,
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadTiposPago()
        {
            if (cmbox_TipoPago_Factura == null) return;

            try
            {
                var lista = new List<TipoPagoDto>
                {
                    new TipoPagoDto { TipoPagoID = 0, Nombre = "-- Seleccione --" }
                };

                var tipos = GetFacturacionService().ListarTiposPago();
                if (tipos != null && tipos.Count > 0)
                    lista.AddRange(tipos);

                cmbox_TipoPago_Factura.DataSource = null;
                cmbox_TipoPago_Factura.DisplayMember = "Nombre";
                cmbox_TipoPago_Factura.ValueMember = "TipoPagoID";
                cmbox_TipoPago_Factura.DataSource = lista;
                cmbox_TipoPago_Factura.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo cargar el tipo de pago.\n\n" + ex.Message,
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private int GetTipoPagoSeleccionado()
        {
            if (cmbox_TipoPago_Factura == null || cmbox_TipoPago_Factura.SelectedValue == null)
                return 0;

            if (cmbox_TipoPago_Factura.SelectedValue is int id)
                return id;

            int.TryParse(cmbox_TipoPago_Factura.SelectedValue.ToString(), out id);
            return id;
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

            if (cmbox_TipoPago_Factura != null && cmbox_TipoPago_Factura.Items.Count > 0)
                cmbox_TipoPago_Factura.SelectedIndex = 0;

            UpdateTotales();

            try
            {
                string codigo = GetFacturacionService().ObtenerSiguienteCodigoFactura();

                if (lbl_Codigo_VentasFacturas != null)
                    lbl_Codigo_VentasFacturas.Text = string.IsNullOrWhiteSpace(codigo) ? "" : codigo.Trim();
            }
            catch (Exception ex)
            {
                if (lbl_Codigo_VentasFacturas != null)
                    lbl_Codigo_VentasFacturas.Text = "";

                MessageBox.Show(
                    "No se pudo generar el siguiente número de factura.\n\n" + ex.Message,
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
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

            int usuarioId = Session.UsuarioId;
            if (usuarioId <= 0)
            {
                MessageBox.Show("No se pudo identificar el usuario de la sesión actual.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int tipoPagoId = GetTipoPagoSeleccionado();
            if (tipoPagoId <= 0)
            {
                MessageBox.Show("Debes seleccionar un tipo de pago válido.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string codigo = GetCodigoFacturaActual();
            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show("No se pudo obtener un número de factura válido.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal subtotal = items.Sum(i => i.Subtotal);
            decimal descuento = Math.Round(subtotal * (_descuentoPct / 100m), 2, MidpointRounding.AwayFromZero);
            if (descuento > subtotal) descuento = subtotal;

            decimal baseImponible = subtotal - descuento;
            decimal iva = Math.Round(baseImponible * IVA_RATE, 2, MidpointRounding.AwayFromZero);
            decimal total = Math.Round(baseImponible + iva, 2, MidpointRounding.AwayFromZero);

            try
            {
                var request = new CrearFacturaRequestDto
                {
                    UsuarioID = usuarioId,
                    ClienteID = _clienteIdActual.Value,
                    NumeroFactura = codigo,
                    Subtotal = subtotal,
                    Descuento = descuento,
                    IVA = iva,
                    Total = total,
                    TipoPagoID = tipoPagoId,
                    Items = items
                };

                var res = GetFacturacionService().CrearFactura(request);

                MessageBox.Show(
                    $"Factura guardada.\nCódigo: {res.NumeroFactura}\nID: {res.FacturaID}",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                ResetFacturaNueva();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo guardar la factura:\n\n" + ex.ToString(), "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs) && ConfigurationManager.ConnectionStrings.Count > 0)
                cs = ConfigurationManager.ConnectionStrings[0]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("No existe connectionString en App.config. Agrega una con nombre 'SISV'.");

            return cs;
        }
        private string GetCodigoFacturaActual()
        {
            return (lbl_Codigo_VentasFacturas?.Text ?? "").Trim();
        }
    }
}