using Capa_Corte_Transversal.Loggin;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Capa_Corte_Transversal.Helpers;
using Dominio_SISV.DTOs;
using Dominio_SISV.Services.Proveedores;
using Union_Formularios_SISV.Controls.Proveedor;
using Union_Formularios_SISV.Logica_Presentacion.Proveedores;

namespace Union_Formularios_SISV.Forms.Proveedores
{
    public partial class Form_Proveedores : Form, IProveedoresView
    {
        private readonly LoginSession _session;
        private readonly Timer _debounce = new Timer();
        private readonly ProveedoresPresenter _presenter;

        public Form_Proveedores() : this(null) { }

        public Form_Proveedores(LoginSession session)
        {
            InitializeComponent();
            _session = session;

            _presenter = new ProveedoresPresenter(this, new ProveedorService());

            Load += Form_Proveedores_Load;
        }

        private void Form_Proveedores_Load(object sender, EventArgs e)
        {
            // combos (UI)
            cmbox_Filtro_Proveedor.Items.Clear();
            cmbox_Filtro_Proveedor.Items.AddRange(new object[] { "Todos", "RUC", "Proveedor", "Telefono", "Correo" });
            cmbox_Filtro_Proveedor.SelectedIndex = 0;

            cmbox_EstadoFiltro_Proveedor.Items.Clear();
            cmbox_EstadoFiltro_Proveedor.Items.AddRange(new object[] { "Todos", "Activo", "Inactivo" });
            cmbox_EstadoFiltro_Proveedor.SelectedIndex = 0;

            cmbox_Estado_Proveedor.Items.Clear();
            cmbox_Estado_Proveedor.Items.AddRange(new object[] { "Activo", "Inactivo" });
            cmbox_Estado_Proveedor.SelectedIndex = 0;

            // Flow layout
            flowProveedor.FlowDirection = FlowDirection.TopDown;
            flowProveedor.WrapContents = false;
            flowProveedor.AutoScroll = true;

            // debounce 350ms
            _debounce.Interval = 350;
            _debounce.Tick += (_, __) =>
            {
                _debounce.Stop();
                _presenter.CargarLista();
            };

            // eventos filtros
            txt_Buscador_Proveedor.TextChanged += (_, __) => DispararBusqueda();
            cmbox_Filtro_Proveedor.SelectionChangeCommitted += (_, __) => DispararBusqueda();
            cmbox_EstadoFiltro_Proveedor.SelectionChangeCommitted += (_, __) => DispararBusqueda();

            // botones
            btn_Registrar_Proveedor.Click += (_, __) => _presenter.Guardar();
            btn_Limpiar_Proveedor.Click += (_, __) => _presenter.Limpiar();

            // estado inicial
            SetSelectedLabel("--");
            SetModoActualizar(false);
            ClearForm();

            _presenter.CargarLista();
        }

        private void DispararBusqueda()
        {
            _debounce.Stop();
            _debounce.Start();
        }

        // ===== IProveedoresView =====
        public int UsuarioId => SessionHelper.GetUsuarioID(_session);

        public string TextoBusqueda => (txt_Buscador_Proveedor.Text ?? "").Trim();
        public string FiltroTexto => (cmbox_Filtro_Proveedor.SelectedItem?.ToString() ?? "Todos");
        public string EstadoFiltroTexto => (cmbox_EstadoFiltro_Proveedor.SelectedItem?.ToString() ?? "Todos");

        public ProveedorDetalleVM ReadForm()
        {
            return new ProveedorDetalleVM
            {
                Nombre = (txt_Nombre_Proveedor.Text ?? "").Trim(),
                Ruc = (txt_RUC_Proveedor.Text ?? "").Trim(),
                Telefono = (txt_Telefono_Proveedor.Text ?? "").Trim(),
                Correo = (txt_Correo_Proveedor.Text ?? "").Trim(),
                Direccion = (txt_Direccion_Proveedor.Text ?? "").Trim(),
                Activo = (cmbox_Estado_Proveedor.SelectedItem?.ToString() ?? "Activo")
                            .Equals("Activo", StringComparison.OrdinalIgnoreCase)
            };
        }

        public void ShowDetalle(ProveedorDetalleVM det)
        {
            txt_Nombre_Proveedor.Text = det.Nombre ?? "";
            txt_RUC_Proveedor.Text = det.Ruc ?? "";
            txt_Telefono_Proveedor.Text = det.Telefono ?? "";
            txt_Correo_Proveedor.Text = det.Correo ?? "";
            txt_Direccion_Proveedor.Text = det.Direccion ?? "";

            cmbox_Estado_Proveedor.SelectedItem = det.Activo ? "Activo" : "Inactivo";
            txt_UltimaAct_Proveedor.Text = det.UltimaActualizacion.HasValue
                ? det.UltimaActualizacion.Value.ToString("yyyy-MM-dd HH:mm")
                : "—";
        }

        public void ClearForm()
        {
            txt_Nombre_Proveedor.Text = "";
            txt_RUC_Proveedor.Text = "";
            txt_Telefono_Proveedor.Text = "";
            txt_Correo_Proveedor.Text = "";
            txt_Direccion_Proveedor.Text = "";
            cmbox_Estado_Proveedor.SelectedIndex = 0;
            txt_UltimaAct_Proveedor.Text = "—";
        }

        public void SetSelectedLabel(string text)
        {
            lbl_Seleccion_Proveedor.Text = string.IsNullOrWhiteSpace(text) ? "--" : text;
        }

        public void SetResultados(int total)
        {
            // el usuario pidió solo la cantidad
            lbl_Resultados_Proveedor.Text = total.ToString();
        }

        public void SetModoActualizar(bool actualizar)
        {
            btn_Registrar_Proveedor.Text = actualizar ? "Actualizar" : "Registrar";
        }

        public void RenderCards(List<ProveedorDetalleVM> proveedores, int? selectedId)
        {
            flowProveedor.SuspendLayout();
            try
            {
                flowProveedor.Controls.Clear();

                int cardW = Math.Max(10, flowProveedor.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6);

                foreach (var vm in proveedores)
                {
                    var card = new ProveedorTaskCard();
                    card.Width = cardW;
                    card.Margin = new Padding(0, 0, 0, 10);

                    card.Bind(vm.ProveedorId, vm);

                    // (opcional) marcar selección visual simple
                    card.BackColor = (selectedId.HasValue && selectedId.Value == vm.ProveedorId)
                        ? Color.FromArgb(235, 245, 255)
                        : Color.White;

                    card.ProveedorSeleccionado += (_, provId) => _presenter.Seleccionar(provId);

                    flowProveedor.Controls.Add(card);
                }
            }
            finally
            {
                flowProveedor.ResumeLayout(true);
            }
        }
        public void SetGuardarHabilitado(bool enabled)
        {
            btn_Registrar_Proveedor.Enabled = enabled;
        }

        public bool PuedeAcceder
        {
            get
            {
                var p = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                return p.HasAny("INV_PROVEEDORES_ACCESO", "INV_PROVEEDORES_REGISTRAR", "INV_PROVEEDORES_ACTUALIZAR", "INV_PROVEEDORES_DESACTIVAR");
            }
        }

        public bool PuedeRegistrar
        {
            get
            {
                var p = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                return p.HasAny("INV_PROVEEDORES_REGISTRAR");
            }
        }

        public bool PuedeActualizar
        {
            get
            {
                var p = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                return p.HasAny("INV_PROVEEDORES_ACTUALIZAR");
            }
        }

        public void ShowInfo(string msg)
            => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void ShowWarning(string msg)
            => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowError(string msg, Exception ex)
            => MessageBox.Show(msg + "\n\n" + ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}