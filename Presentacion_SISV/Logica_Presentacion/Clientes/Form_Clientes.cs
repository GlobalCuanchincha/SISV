using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.DTOs.Clientes;
using Dominio_SISV.Services.Clientes;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;
using Union_Formularios_SISV.Forms.Clientes;
using Union_Formularios_SISV.Logica_Presentacion.Clientes;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Clientes : Form, IClientesView
    {
        private const string P_ACCESO = "CRM_CLIENTES_ACCESO";
        private const string P_REG = "CRM_CLIENTES_REGISTRAR";
        private const string P_UPD = "CRM_CLIENTES_ACTUALIZAR";

        private readonly Timer _searchDebounce;
        private readonly IClienteService _service;
        private readonly ClientesPresenter _presenter;

        private PermissionContext _perm;

        private string _selectedCedula = null;
        private readonly List<ClientTaskCard> _renderedCards = new List<ClientTaskCard>();

        public Form_Clientes()
        {
            InitializeComponent();

            _perm = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            if (_perm == null) _perm = new PermissionContext(new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            _service = new ClienteService();
            _presenter = new ClientesPresenter(this, _service, _perm);

            _searchDebounce = new Timer();
            _searchDebounce.Interval = 350;
            _searchDebounce.Tick += delegate
            {
                _searchDebounce.Stop();
                _presenter.Search();
            };

            Load += Form_Clientes_Load;

            txt_Buscador_Items_Clientes.TextChanged += delegate { DebounceSearch(); };
            cmbox_Filtrarpor_Clientes.SelectedIndexChanged += delegate { _presenter.Search(); };
            cmbox_EstadoFiltro_Clientes.SelectedIndexChanged += delegate { _presenter.Search(); };

            btn_Registrar_Clientes.Click += delegate { _presenter.Registrar(); };
            btn_Actualizar_Clientes.Click += delegate { _presenter.Actualizar(); };
            btn_Limpiar_Clientes.Click += delegate { _presenter.Limpiar(); };

            txt_Buscador_Items_Clientes.KeyDown += Txt_Buscador_Items_Clientes_KeyDown;

            txt_Cedula_Cliente.KeyPress += txt_Cedula_Cliente_KeyPress;
            txt_Telefono_Clientes.KeyPress += txt_Telefono_Clientes_KeyPress;
            txt_Telefono_Clientes.Validating += txt_Telefono_Clientes_Validating;

            if (flowClientCard != null)
                flowClientCard.SizeChanged += delegate { FixAllCardsWidth(); };
        }

        private void Form_Clientes_Load(object sender, EventArgs e)
        {
            if (!_perm.TryEnsure(P_ACCESO, "Acceso denegado: no tiene permiso para Clientes."))
            {
                Close();
                return;
            }

            ApplyPermissionsToUI();
            _presenter.Initialize();
        }

        private void ApplyPermissionsToUI()
        {
            bool canReg = _perm.Has(P_REG);
            bool canUpd = _perm.Has(P_UPD);

            if (btn_Registrar_Clientes != null) btn_Registrar_Clientes.Enabled = canReg;
            if (btn_Actualizar_Clientes != null) btn_Actualizar_Clientes.Enabled = false; // lo habilita el presenter + permiso

            bool canEdit = canReg || canUpd;
            SetFormEditable(canEdit);

            if (!canEdit)
                SetActualizarEnabled(false);
        }

        private void SetFormEditable(bool editable)
        {
            // En modo solo lectura, dejamos el formulario visible, pero no editable.
            SetTextReadOnly(txt_Cedula_Cliente, !editable);
            SetTextReadOnly(txt_Telefono_Clientes, !editable);
            SetTextReadOnly(txt_Nombre_Clientes, !editable);
            SetTextReadOnly(txt_Apellido_Clientes, !editable);
            SetTextReadOnly(txt_Correo_Clientes, !editable);
            SetTextReadOnly(txt_Direccion_Clientes, !editable);

            if (cmbox_Estado_Clientes != null) cmbox_Estado_Clientes.Enabled = editable;
        }

        private static void SetTextReadOnly(Control c, bool readOnly)
        {
            if (c == null) return;

            // Guna2TextBox y TextBox tienen ReadOnly
            var p = c.GetType().GetProperty("ReadOnly");
            if (p != null && p.CanWrite)
            {
                p.SetValue(c, readOnly, null);
                return;
            }

            // fallback
            c.Enabled = !readOnly;
        }

        private void DebounceSearch()
        {
            _searchDebounce.Stop();
            _searchDebounce.Start();
        }

        private void Txt_Buscador_Items_Clientes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            _searchDebounce.Stop();
            _presenter.EnterPressed();
        }

        private void FixAllCardsWidth()
        {
            if (flowClientCard == null) return;
            int w = flowClientCard.ClientSize.Width - 28;
            if (w < 200) w = 200;

            foreach (var c in _renderedCards)
                c.Width = w;
        }

        // =======================
        // IClientesView (lecturas)
        // =======================
        public string BuscarTexto
        {
            get { return (txt_Buscador_Items_Clientes.Text ?? "").Trim(); }
        }

        public string FiltroPorKey
        {
            get
            {
                var val = cmbox_Filtrarpor_Clientes.SelectedValue;
                return val != null ? Convert.ToString(val) : "nombre";
            }
        }

        public int? EstadoFiltroKey
        {
            get
            {
                if (cmbox_EstadoFiltro_Clientes.SelectedValue == null) return null;

                int n;
                if (int.TryParse(Convert.ToString(cmbox_EstadoFiltro_Clientes.SelectedValue), out n))
                    return n;

                return null;
            }
        }

        // =======================
        // IClientesView (binds)
        // =======================
        public void BindFiltroPor(List<KeyValuePair<string, string>> opciones, string defaultKey)
        {
            cmbox_Filtrarpor_Clientes.DataSource = null;
            cmbox_Filtrarpor_Clientes.DisplayMember = "Value";
            cmbox_Filtrarpor_Clientes.ValueMember = "Key";
            cmbox_Filtrarpor_Clientes.DataSource = opciones;
            cmbox_Filtrarpor_Clientes.SelectedValue = defaultKey;
        }

        public void BindEstados(List<ClienteEstadoVM> estados, int? defaultEstadoKey)
        {
            cmbox_Estado_Clientes.DataSource = null;
            cmbox_Estado_Clientes.DisplayMember = "EstadoNombre";
            cmbox_Estado_Clientes.ValueMember = "EstadoKey";
            cmbox_Estado_Clientes.DataSource = estados;

            if (defaultEstadoKey.HasValue)
                cmbox_Estado_Clientes.SelectedValue = defaultEstadoKey.Value;
        }

        public void BindEstadosFiltro(List<ClienteEstadoVM> estadosFiltro)
        {
            cmbox_EstadoFiltro_Clientes.DataSource = null;
            cmbox_EstadoFiltro_Clientes.DisplayMember = "EstadoNombre";
            cmbox_EstadoFiltro_Clientes.ValueMember = "EstadoKey";
            cmbox_EstadoFiltro_Clientes.DataSource = estadosFiltro;
            cmbox_EstadoFiltro_Clientes.SelectedIndex = 0;
        }

        // =======================
        // IClientesView (render)
        // =======================
        public void ShowClientes(List<ClienteCardVM> clientes, string selectedCedula)
        {
            _selectedCedula = selectedCedula;

            flowClientCard.SuspendLayout();
            flowClientCard.Controls.Clear();
            _renderedCards.Clear();

            int w = flowClientCard.ClientSize.Width - 28;
            if (w < 200) w = 200;

            foreach (var vm in clientes)
            {
                var card = new ClientTaskCard();
                card.Margin = new Padding(6);
                card.Width = w;
                card.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                card.Bind(vm);
                card.SetSelected(!string.IsNullOrWhiteSpace(_selectedCedula) &&
                                 string.Equals(_selectedCedula, vm.Cedula, StringComparison.OrdinalIgnoreCase));

                card.ClientSelected += Card_ClientSelected;

                _renderedCards.Add(card);
                flowClientCard.Controls.Add(card);
            }

            flowClientCard.ResumeLayout();
        }

        private void Card_ClientSelected(object sender, ClienteCardSelectedEventArgs e)
        {
            if (e == null || e.Cliente == null) return;
            _selectedCedula = e.Cliente.Cedula;

            foreach (var c in _renderedCards)
                c.SetSelected(string.Equals(c.Cedula, _selectedCedula, StringComparison.OrdinalIgnoreCase));

            _presenter.SelectFromCard(_selectedCedula);
        }

        public void SetResultados(int count)
        {
            lbl_Cantidad_Resultados_Clientes.Text = count.ToString() + " resultados";
        }

        public ClienteDetalleVM ReadForm()
        {
            return new ClienteDetalleVM
            {
                Cedula = (txt_Cedula_Cliente.Text ?? "").Trim(),
                Nombres = (txt_Nombre_Clientes.Text ?? "").Trim(),
                Apellidos = (txt_Apellido_Clientes.Text ?? "").Trim(),
                Correo = (txt_Correo_Clientes.Text ?? "").Trim(),
                Telefono = (txt_Telefono_Clientes.Text ?? "").Trim(),
                Direccion = (txt_Direccion_Clientes.Text ?? "").Trim(),
                EstadoKey = GetEstadoFormKey()
            };
        }

        public void ShowDetalle(ClienteDetalleVM det)
        {
            if (det == null) return;

            lbl_Seleccion_Clientes.Text = "Seleccionado: " + (det.Cedula ?? "");

            txt_Cedula_Cliente.Text = det.Cedula ?? "";
            txt_Nombre_Clientes.Text = det.Nombres ?? "";
            txt_Apellido_Clientes.Text = det.Apellidos ?? "";
            txt_Correo_Clientes.Text = det.Correo ?? "";
            txt_Telefono_Clientes.Text = det.Telefono ?? "";
            txt_Direccion_Clientes.Text = det.Direccion ?? "";

            if (det.EstadoKey.HasValue)
                cmbox_Estado_Clientes.SelectedValue = det.EstadoKey.Value;
        }

        public void ClearSelectionAndForm()
        {
            _selectedCedula = null;

            lbl_Seleccion_Clientes.Text = "Sin seleccionar";
            SetCedulaReadOnly(false);
            SetActualizarEnabled(false);

            txt_Cedula_Cliente.Text = "";
            txt_Telefono_Clientes.Text = "";
            txt_Nombre_Clientes.Text = "";
            txt_Apellido_Clientes.Text = "";
            txt_Correo_Clientes.Text = "";
            txt_Direccion_Clientes.Text = "";

            TrySelectEstadoActivo();
        }

        public void SetCedulaReadOnly(bool readOnly)
        {
            if (txt_Cedula_Cliente != null)
                txt_Cedula_Cliente.ReadOnly = readOnly;
        }

        public void SetActualizarEnabled(bool enabled)
        {
            // Solo habilita si además tiene permiso
            bool can = enabled && _perm.Has(P_UPD);
            btn_Actualizar_Clientes.Enabled = can;
        }

        public void SetSelectedLabel(string text)
        {
            lbl_Seleccion_Clientes.Text = text ?? "Sin seleccionar";
        }

        public void SetBusqueda(string filtroKey, string texto)
        {
            try { cmbox_Filtrarpor_Clientes.SelectedValue = filtroKey; } catch { }
            txt_Buscador_Items_Clientes.Text = texto ?? "";
        }

        public void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string title, Exception ex)
        {
            MessageBox.Show(title + "\n\n" + (ex != null ? ex.Message : ""), "SISV",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // =======================
        // helpers
        // =======================
        private int? GetEstadoFormKey()
        {
            if (cmbox_Estado_Clientes.SelectedValue == null) return null;

            int n;
            if (int.TryParse(Convert.ToString(cmbox_Estado_Clientes.SelectedValue), out n))
                return n;

            return null;
        }

        private void TrySelectEstadoActivo()
        {
            try
            {
                var items = cmbox_Estado_Clientes.DataSource as IEnumerable<ClienteEstadoVM>;
                if (items == null) return;

                var activo = items.FirstOrDefault(x =>
                    (x.EstadoNombre ?? "").ToLower().Contains("activo") &&
                    !(x.EstadoNombre ?? "").ToLower().Contains("inactivo"));

                if (activo != null && activo.EstadoKey.HasValue)
                    cmbox_Estado_Clientes.SelectedValue = activo.EstadoKey.Value;
            }
            catch { }
        }

        // =======================
        // Validaciones (mantengo las tuyas)
        // =======================
        private void txt_Cedula_Cliente_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void txt_Telefono_Clientes_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            var txt = sender as Guna2TextBox;
            if (txt == null) return;

            if (!char.IsControl(e.KeyChar))
            {
                int selectionLength = txt.SelectionLength;
                if (txt.Text.Length - selectionLength >= 10)
                    e.Handled = true;
            }
        }

        private void txt_Telefono_Clientes_Validating(object sender, CancelEventArgs e)
        {
            string tel = (txt_Telefono_Clientes.Text ?? "").Trim();

            if (!EsTelefonoEcuador(tel, out string msg))
            {
                e.Cancel = true;
                txt_Telefono_Clientes.BackColor = Color.MistyRose;
                MessageBox.Show(msg, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_Telefono_Clientes.SelectAll();
            }
            else
            {
                txt_Telefono_Clientes.BackColor = Color.White;
            }
        }

        private bool EsTelefonoEcuador(string tel, out string msg)
        {
            msg = null;

            if (string.IsNullOrWhiteSpace(tel))
            {
                msg = "Ingrese un número de teléfono.";
                return false;
            }

            if (Regex.IsMatch(tel, @"^09\d{8}$"))
                return true;

            if (Regex.IsMatch(tel, @"^0[2-7]\d{7}$"))
                return true;

            msg = "Teléfono inválido. Ecuador:\n- Celular: 09 + 8 dígitos (10 dígitos)\n- Fijo: 0 + (2 a 7) + 7 dígitos (9 dígitos)";
            return false;
        }
    }
}