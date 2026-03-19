using Capa_Corte_Transversal.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms.Ordenes_de_Servicio;
using Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Notificacion;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Form_Ordenes_Servicio_Notificacion : Form, IOrdenesNotificacionView
    {
        private readonly object _session;
        private readonly OrdenesNotificacionPresenter _presenter;

        private Control _navBtnActivo;
        private Panel _navIndicator;
        private readonly Timer _navAnim = new Timer { Interval = 15 };
        private int _navTargetTop;
        private int _navTargetHeight;
        private readonly System.Collections.Generic.Dictionary<Control, BtnStyle> _navStyles =
            new System.Collections.Generic.Dictionary<Control, BtnStyle>();

        private readonly Color _navActiveColor = Color.FromArgb(28, 188, 135);
        private readonly Color _navActiveBg = Color.FromArgb(232, 250, 240);
        private readonly Color _navIdleFg = Color.FromArgb(60, 60, 60);
        private readonly Color _navIdleBg = Color.Transparent;

        private sealed class BtnStyle
        {
            public Color Back;
            public Color Fore;
            public Font Font;
        }

        public Form_Ordenes_Servicio_Notificacion() : this(null) { }

        public Form_Ordenes_Servicio_Notificacion(object session)
        {
            InitializeComponent();

            _session = session;
            _presenter = new OrdenesNotificacionPresenter(this);

            _navAnim.Tick += (s, e) => NavAnimTick();
            Load += async (s, e) => await InitAsync();
        }

        private async System.Threading.Tasks.Task InitAsync()
        {
            btn_Recepcion_Notificacion.Click += (s, e) => IrARecepcion();
            btn_Equipos_Notificacion.Click += (s, e) => IrAEquipos();

            btn_Seleccionar_Orden_Diagnostico.Click += async (s, e) => await _presenter.SeleccionarOrdenAsync();
            btn_Limpiar_Diagnostico.Click += (s, e) => _presenter.LimpiarPantallaOrden();

            btn_GuardarDiagnostico_Diagnostico.Click += async (s, e) => await _presenter.GuardarDiagnosticoAsync();
            btn_GuardarEstado_Diagnostico.Click += async (s, e) => await _presenter.GuardarEstadoAsync();

            btn_Previsualizar_Notificacion.Click += (s, e) => _presenter.PrevisualizarNotificacion();
            btn_Enviar_Notificacion.Click += async (s, e) => await _presenter.EnviarNotificacionAsync();

            await _presenter.InitializeAsync();
            ActivateNav(btn_Notificacion);
        }

        // ========= IOrdenesNotificacionView =========

        public int UsuarioId
        {
            get
            {
                try { return _session == null ? 0 : SessionHelper.GetUsuarioID(_session); }
                catch { return 0; }
            }
        }

        public string Diagnostico
        {
            get => txt_Diagnostico_Diagnostico.Text ?? "";
            set => txt_Diagnostico_Diagnostico.Text = value ?? "";
        }

        public string CorreoNotificacion
        {
            get => txt_Correo_Notificacion.Text ?? "";
            set => txt_Correo_Notificacion.Text = value ?? "";
        }

        public string AsuntoNotificacion
        {
            get => txt_Asunto_Notificacion.Text ?? "";
            set => txt_Asunto_Notificacion.Text = value ?? "";
        }

        public string MensajeNotificacion
        {
            get => txt_Mensaje_Notificacion.Text ?? "";
            set => txt_Mensaje_Notificacion.Text = value ?? "";
        }

        public int EstadoSeleccionadoId
        {
            get
            {
                try
                {
                    if (Cmbox_Estado_Diagnostico.SelectedValue == null) return 0;
                    return Convert.ToInt32(Cmbox_Estado_Diagnostico.SelectedValue);
                }
                catch { return 0; }
            }
        }

        public bool TieneEstadoSeleccionado => Cmbox_Estado_Diagnostico.SelectedValue != null;

        public void BindEstados(DataTable dt)
        {
            if (dt == null) dt = new DataTable();

            var view = new DataView(dt);
            if (dt.Columns.Contains("EstadoValor"))
                view.RowFilter = "EstadoValor <> -1";

            Cmbox_Estado_Diagnostico.DataSource = view;
            Cmbox_Estado_Diagnostico.DisplayMember =
                dt.Columns.Contains("EstadoNombre")
                    ? "EstadoNombre"
                    : (view.Table.Columns.Count > 0 ? view.Table.Columns[0].ColumnName : "");

            Cmbox_Estado_Diagnostico.ValueMember =
                dt.Columns.Contains("EstadoValor")
                    ? "EstadoValor"
                    : (view.Table.Columns.Count > 1 ? view.Table.Columns[1].ColumnName : "");
        }

        public bool TrySeleccionarOrden(out int ordenServicioId)
        {
            ordenServicioId = 0;

            using (var f = new Seleccion_Orden(_session))
            {
                var dr = f.ShowDialog(this);
                if (dr != DialogResult.OK) return false;

                ordenServicioId = f.OrdenServicioIDSeleccionado;
                return ordenServicioId > 0;
            }
        }

        public void SetOrdenDetalle(
            string codigoOrden,
            string cliente,
            string equipo,
            string diagnostico,
            string estadoNombre,
            string correoCliente,
            int? estadoId)
        {
            txt_OrdenSeleccionada_Diagnostico.Text = codigoOrden ?? "";
            txt_Cliente_Diagnostico.Text = cliente ?? "";
            txt_TipoEquipo_Diagnostico.Text = equipo ?? "";
            txt_Diagnostico_Diagnostico.Text = diagnostico ?? "";

            lbl_EstadoEquipo_Diagnostico.Text = estadoNombre ?? "";
            txt_Correo_Notificacion.Text = correoCliente ?? "";
            lbl_CorreoCliente_Notificacion.Text = correoCliente ?? "";

            if (estadoId.HasValue)
            {
                try { Cmbox_Estado_Diagnostico.SelectedValue = estadoId.Value; } catch { }
            }
        }

        public void ClearOrden()
        {
            txt_OrdenSeleccionada_Diagnostico.Text = "";
            txt_Cliente_Diagnostico.Text = "";
            txt_TipoEquipo_Diagnostico.Text = "";
            txt_Diagnostico_Diagnostico.Text = "";

            lbl_EstadoEquipo_Diagnostico.Text = "";

            txt_Correo_Notificacion.Text = "";
            txt_Asunto_Notificacion.Text = "";
            txt_Mensaje_Notificacion.Text = "";
            lbl_CorreoCliente_Notificacion.Text = "";

            lbl_HoraActualizacion_Diagnostico.Text = "";
            lbl_diaActualizacion_Diagnostico.Text = "";
            lbl_HoraActualizacion_Notificacion.Text = "";
            lbl_diaActualizacion_Notificacion.Text = "";
        }

        public void SetPermisosAcciones(bool puedeGuardarDiag, bool puedeCambiarEstado, bool puedeEnviarCorreo)
        {
            btn_GuardarDiagnostico_Diagnostico.Enabled = puedeGuardarDiag;
            btn_GuardarEstado_Diagnostico.Enabled = puedeCambiarEstado;
            btn_Enviar_Notificacion.Enabled = puedeEnviarCorreo;
        }

        public void SetVisibilidadNavegacion(bool verRecepcion, bool verEquipos)
        {
            if (btn_Recepcion_Notificacion != null)
                btn_Recepcion_Notificacion.Visible = verRecepcion;

            if (btn_Equipos_Notificacion != null)
                btn_Equipos_Notificacion.Visible = verEquipos;
        }

        public void SetActualizacionDiagnostico(DateTime dt)
        {
            lbl_HoraActualizacion_Diagnostico.Text = dt.ToString("HH:mm");
            lbl_diaActualizacion_Diagnostico.Text = dt.ToString("dd/MM/yyyy");
        }

        public void SetActualizacionNotificacion(DateTime dt)
        {
            lbl_HoraActualizacion_Notificacion.Text = dt.ToString("HH:mm");
            lbl_diaActualizacion_Notificacion.Text = dt.ToString("dd/MM/yyyy");
        }

        public void MostrarPrevisualizacion(string correo, string asunto, string mensaje)
        {
            MessageBox.Show(
                $"Para: {correo}\nAsunto: {asunto}\n\n{mensaje}",
                "Previsualización",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        public void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string msg, Exception ex = null)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(msg) ? "Ocurrió un error al procesar la operación." : msg,
                "SISV",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        public void CloseView()
        {
            BeginInvoke(new Action(() => Close()));
        }

        // ========= NAV =========

        private void ActivateNav(Control btn)
        {
            if (btn == null) return;

            if (!_navStyles.ContainsKey(btn))
                _navStyles[btn] = new BtnStyle { Back = btn.BackColor, Fore = btn.ForeColor, Font = btn.Font };

            if (_navBtnActivo != null && _navBtnActivo != btn && _navStyles.ContainsKey(_navBtnActivo))
            {
                var st = _navStyles[_navBtnActivo];
                SetFillOrBack(_navBtnActivo, _navIdleBg);
                _navBtnActivo.ForeColor = _navIdleFg;
                _navBtnActivo.Font = new Font(st.Font, FontStyle.Regular);
                TrySetProp(_navBtnActivo, "BorderColor", Color.Transparent);
                TrySetProp(_navBtnActivo, "BorderThickness", 0);
            }

            _navBtnActivo = btn;

            SetFillOrBack(btn, _navActiveBg);
            btn.ForeColor = _navActiveColor;
            btn.Font = new Font(btn.Font, FontStyle.Bold);

            TrySetProp(btn, "BorderColor", _navActiveColor);
            TrySetProp(btn, "BorderThickness", 1);

            EnsureIndicator(btn);

            _navTargetTop = btn.Top;
            _navTargetHeight = btn.Height;

            if (_navIndicator.Top == _navTargetTop && _navIndicator.Height == _navTargetHeight) return;
            _navAnim.Start();
        }

        private void EnsureIndicator(Control btn)
        {
            if (btn?.Parent == null) return;

            if (_navIndicator == null || _navIndicator.Parent != btn.Parent)
            {
                _navIndicator?.Dispose();
                _navIndicator = new Panel
                {
                    Width = 4,
                    Height = btn.Height,
                    Left = 0,
                    Top = btn.Top,
                    BackColor = _navActiveColor
                };

                btn.Parent.Controls.Add(_navIndicator);
                _navIndicator.BringToFront();
            }
            else
            {
                _navIndicator.BackColor = _navActiveColor;
                _navIndicator.BringToFront();
            }
        }

        private void NavAnimTick()
        {
            if (_navIndicator == null || _navBtnActivo == null)
            {
                _navAnim.Stop();
                return;
            }

            int speed = 10;

            int dy = _navTargetTop - _navIndicator.Top;
            if (Math.Abs(dy) <= speed) _navIndicator.Top = _navTargetTop;
            else _navIndicator.Top += Math.Sign(dy) * speed;

            int dh = _navTargetHeight - _navIndicator.Height;
            if (Math.Abs(dh) <= 2) _navIndicator.Height = _navTargetHeight;
            else _navIndicator.Height += Math.Sign(dh) * 2;

            if (_navIndicator.Top == _navTargetTop && _navIndicator.Height == _navTargetHeight)
                _navAnim.Stop();
        }

        private static void SetFillOrBack(Control c, Color color)
        {
            if (c == null) return;
            c.BackColor = color;
            TrySetProp(c, "FillColor", color);
        }

        private static void TrySetProp(Control c, string prop, object value)
        {
            try
            {
                var p = c.GetType().GetProperty(prop);
                if (p != null && p.CanWrite) p.SetValue(c, value);
            }
            catch { }
        }

        // ========= Navegación =========

        private void IrARecepcion()
        {
            ActivateNav(btn_Recepcion_Notificacion);
            OpenInMainHost("Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Recepcion",
                "Recepción", "Recepción / Solicitud de orden");
        }

        private void IrAEquipos()
        {
            ActivateNav(btn_Equipos_Notificacion);
            OpenInMainHost("Union_Formularios_SISV.Forms.Form_Ordenes_Servicio",
                "Equipos", "Gestión de equipos / órdenes");
        }

        private void OpenInMainHost(string formTypeName, string titulo, string descripcion)
        {
            var t = FindTypeInLoadedAssemblies(formTypeName);
            if (t == null || !typeof(Form).IsAssignableFrom(t))
            {
                MessageBox.Show("No se encontró el formulario destino.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Form child = null;

            var ctorObj = t.GetConstructor(new[] { typeof(object) });
            if (ctorObj != null) child = (Form)ctorObj.Invoke(new object[] { _session });

            if (child == null)
            {
                var ctorEmpty = t.GetConstructor(Type.EmptyTypes);
                if (ctorEmpty != null) child = (Form)ctorEmpty.Invoke(null);
            }

            if (child == null)
            {
                MessageBox.Show("No pude instanciar el formulario destino.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var main = this.Parent?.FindForm() as Union_Formularios_SISV.Form_Panel_Principal;
            if (main != null) { main.OpenChild(child, titulo, descripcion); return; }

            child.Show();
        }

        private static Type FindTypeInLoadedAssemblies(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName, false, true);
                if (t != null) return t;
            }
            return null;
        }
    }
}