using Capa_Corte_Transversal.Helpers;
using Presentacion_SISV.Controls.Ordenes_de_Servicio.Equipos;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Equipos;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Ordenes_Servicio : Form, IOrdenesEquiposView
    {
        private readonly object _session;
        private readonly Timer _debounceBuscar = new Timer { Interval = 350 };
        private readonly OrdenesEquiposPresenter _presenter;
        private bool _isLoading;

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

        public Form_Ordenes_Servicio() : this(null) { }

        public Form_Ordenes_Servicio(object session)
        {
            InitializeComponent();

            _session = session;
            _presenter = new OrdenesEquiposPresenter(this);

            btn_Recepcion_Equipos.Click += (s, e) => IrARecepcion();
            btn_Notificacion_Equipos.Click += (s, e) => IrANotificacion();

            _navAnim.Tick += (s, e) => NavAnimTick();
            Load += async (s, e) => await Form_LoadAsync();
        }

        private async Task Form_LoadAsync()
        {
            _isLoading = true;

            _debounceBuscar.Tick += async (s, e) =>
            {
                _debounceBuscar.Stop();
                await _presenter.BuscarAsync();
            };

            txt_Buscador_Items_Equipos.TextChanged += (s, e) =>
            {
                if (_isLoading) return;
                _debounceBuscar.Stop();
                _debounceBuscar.Start();
            };

            cmbox_Filtrarpor_Equipos.SelectedIndexChanged += async (s, e) =>
            {
                if (_isLoading) return;
                await _presenter.BuscarAsync();
            };

            btn_Selecciona_Clientes_Equipo.Click += async (s, e) => await _presenter.ElegirClienteAsync();
            btn_Guardar_Equipos.Click += async (s, e) => await _presenter.GuardarAsync();
            btn_Limpiar_Equipos.Click += async (s, e) => await _presenter.NuevoAsync();

            Flow_OrdenServicio_Equipos.SizeChanged += (s, e) => AjustarAnchoCards();

            _isLoading = false;
            await _presenter.InitializeAsync();

            var btnEquipos = FindControlDeep("btn_Equipos");
            if (btnEquipos != null) ActivateNav(btnEquipos);
            else ActivateNav(btn_Recepcion_Equipos);
        }

        // ========= IOrdenesEquiposView =========

        public int UsuarioId
        {
            get
            {
                try { return _session == null ? 0 : SessionHelper.GetUsuarioID(_session); }
                catch { return 0; }
            }
        }

        public string BuscarTexto => (txt_Buscador_Items_Equipos.Text ?? "").Trim();

        public string FiltroSeleccionado
        {
            get
            {
                if (cmbox_Filtrarpor_Equipos.SelectedValue == null) return "todos";
                return Convert.ToString(cmbox_Filtrarpor_Equipos.SelectedValue) ?? "todos";
            }
        }

        public int TipoEquipoSeleccionadoId
        {
            get
            {
                try
                {
                    if (Cmbox_TipoEquipo_Equipos.SelectedValue == null) return 0;
                    return Convert.ToInt32(Cmbox_TipoEquipo_Equipos.SelectedValue);
                }
                catch { return 0; }
            }
        }

        public string ConectividadSeleccionada
        {
            get
            {
                if (cmbox_Conectividad_Equipos.SelectedValue == null) return null;
                return Convert.ToString(cmbox_Conectividad_Equipos.SelectedValue);
            }
        }

        public string CodigoInterno
        {
            get => lbl_CodigoInterno_Equipos.Text ?? "";
            set => lbl_CodigoInterno_Equipos.Text = value ?? "";
        }

        public string ClienteNombre
        {
            get => lbl_Cliente_Equipos.Text ?? "";
            set => lbl_Cliente_Equipos.Text = string.IsNullOrWhiteSpace(value) ? "Sin selección" : value;
        }

        public string Marca
        {
            get => txt_Marca_Equipos.Text ?? "";
            set => txt_Marca_Equipos.Text = value ?? "";
        }

        public string Modelo
        {
            get => txt_Modelo_Equipos.Text ?? "";
            set => txt_Modelo_Equipos.Text = value ?? "";
        }

        public string Serie
        {
            get => txt_NumSerie_Equipos.Text ?? "";
            set => txt_NumSerie_Equipos.Text = value ?? "";
        }

        public string ColorEquipo
        {
            get => txt_Color_Equipos.Text ?? "";
            set => txt_Color_Equipos.Text = value ?? "";
        }

        public string Accesorios
        {
            get => txt_Accesorios_Equipos.Text ?? "";
            set => txt_Accesorios_Equipos.Text = value ?? "";
        }

        public string Observaciones
        {
            get => txt_Observaciones.Text ?? "";
            set => txt_Observaciones.Text = value ?? "";
        }

        public void BindFiltros(DataTable dt)
        {
            BindComboSafe(cmbox_Filtrarpor_Equipos, dt, "Text", "Value");
        }

        public void BindTiposEquipo(DataTable dt)
        {
            BindComboSafe(Cmbox_TipoEquipo_Equipos, dt, "TipoEquipoNombre", "TipoEquipoID");
        }

        public void BindConectividades(DataTable dt)
        {
            BindComboSafe(cmbox_Conectividad_Equipos, dt, "ConectividadNombre", "ConectividadValor");
        }

        public void RenderEquipos(DataTable dt, int selectedEquipoId)
        {
            if (dt == null) dt = new DataTable();

            Flow_OrdenServicio_Equipos.SuspendLayout();
            Flow_OrdenServicio_Equipos.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int equipoId = ToInt(r, "EquipoID");
                if (equipoId <= 0) continue;

                string codigo = ToStr(r, "CodigoInterno", "Codigo");
                string nombreEquipo = ToStr(r, "NombreEquipo", "Equipo", "Descripcion");
                string cliente = ToStr(r, "Cliente", "ClienteNombre");
                string serie = ToStr(r, "Serie", "NumeroSerie");

                var card = new EquiposTaskCard
                {
                    Width = Math.Max(200, Flow_OrdenServicio_Equipos.ClientSize.Width - 22)
                };

                card.Bind(equipoId, codigo, nombreEquipo, cliente, serie);
                card.SetSelected(equipoId == selectedEquipoId);

                card.EquipoSeleccionado += async (s, args) =>
                {
                    await _presenter.SeleccionarEquipoAsync(args.EquipoID);
                };

                Flow_OrdenServicio_Equipos.Controls.Add(card);
            }

            Flow_OrdenServicio_Equipos.ResumeLayout();
        }

        public void SetResultados(int total)
        {
            lbl_Contador_de_resultados_Equipos.Text = $"{total} resultados";
        }

        public void SetTipoEquipoSeleccionado(object value)
        {
            try
            {
                if (value == null) Cmbox_TipoEquipo_Equipos.SelectedIndex = 0;
                else Cmbox_TipoEquipo_Equipos.SelectedValue = value;
            }
            catch
            {
                try { Cmbox_TipoEquipo_Equipos.SelectedIndex = 0; } catch { }
            }
        }

        public void SetConectividadSeleccionada(object value)
        {
            try
            {
                if (value == null) cmbox_Conectividad_Equipos.SelectedIndex = 0;
                else cmbox_Conectividad_Equipos.SelectedValue = value;
            }
            catch
            {
                try { cmbox_Conectividad_Equipos.SelectedIndex = 0; } catch { }
            }
        }

        public void SetModoActualizar(bool actualizar)
        {
            btn_Guardar_Equipos.Text = actualizar ? "Actualizar" : "Guardar";
        }

        public void SetPermisosAcciones(bool puedeGuardar, bool puedeElegirCliente)
        {
            btn_Guardar_Equipos.Enabled = puedeGuardar;
            btn_Selecciona_Clientes_Equipo.Enabled = puedeElegirCliente;
        }

        public void SetVisibilidadNavegacion(bool verRecepcion, bool verNotificacion)
        {
            if (btn_Recepcion_Equipos != null)
                btn_Recepcion_Equipos.Visible = verRecepcion;

            if (btn_Notificacion_Equipos != null)
                btn_Notificacion_Equipos.Visible = verNotificacion;
        }

        public bool TrySeleccionarCliente(out int? clienteId, out string clienteNombre)
        {
            clienteId = null;
            clienteNombre = null;

            using (var f = new Seleccion_Cliente(_session))
            {
                var dr = f.ShowDialog(this);
                if (dr != DialogResult.OK || !f.SelectedClienteID.HasValue)
                    return false;

                clienteId = f.SelectedClienteID.Value;
                clienteNombre = f.SelectedClienteNombre ?? "";
                return true;
            }
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

        // ========= Navegación =========

        private void IrARecepcion()
        {
            ActivateNav(btn_Recepcion_Equipos);

            OpenInMainHost(
                CreateFormFromAnyCtor(new[]
                {
                    "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Recepcion"
                }),
                "Recepción",
                "Recepción / Solicitud de orden"
            );
        }

        private void IrANotificacion()
        {
            ActivateNav(btn_Notificacion_Equipos);

            OpenInMainHost(
                CreateFormFromAnyCtor(new[]
                {
                    "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Notificacion"
                }),
                "Notificación",
                "Actualizar estado / notificaciones del servicio"
            );
        }

        private void OpenInMainHost(Form child, string titulo, string descripcion)
        {
            if (child == null)
            {
                MessageBox.Show("No se encontró el formulario destino.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var main = this.Parent?.FindForm() as Union_Formularios_SISV.Form_Panel_Principal;
            if (main != null)
            {
                main.OpenChild(child, titulo, descripcion);
                return;
            }

            child.Show();
        }

        private Form CreateFormFromAnyCtor(string[] fullTypeNames)
        {
            Type t = null;
            foreach (var name in fullTypeNames)
            {
                t = FindTypeInLoadedAssemblies(name);
                if (t != null) break;
            }
            if (t == null) return null;
            if (!typeof(Form).IsAssignableFrom(t)) return null;

            var ctorObj = t.GetConstructor(new[] { typeof(object) });
            if (ctorObj != null) return (Form)ctorObj.Invoke(new object[] { _session });

            var ctorEmpty = t.GetConstructor(Type.EmptyTypes);
            if (ctorEmpty != null) return (Form)ctorEmpty.Invoke(null);

            return null;
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

        // ========= NAV UI =========

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

        private static Control FindControlDeep(string name)
        {
            var openForms = Application.OpenForms;
            foreach (Form f in openForms)
            {
                var found = f.Controls.Find(name, true);
                if (found != null && found.Length > 0) return found[0];
            }
            return null;
        }

        private void AjustarAnchoCards()
        {
            foreach (Control c in Flow_OrdenServicio_Equipos.Controls)
                if (c is EquiposTaskCard card)
                    card.Width = Math.Max(200, Flow_OrdenServicio_Equipos.ClientSize.Width - 22);
        }

        private static void BindComboSafe(ComboBox cb, DataTable dt, string displayCol, string valueCol)
        {
            if (cb == null) return;
            if (dt == null) dt = new DataTable();

            string disp = dt.Columns.Contains(displayCol) ? displayCol : (dt.Columns.Count > 0 ? dt.Columns[0].ColumnName : "");
            string val = dt.Columns.Contains(valueCol) ? valueCol : (dt.Columns.Count > 1 ? dt.Columns[1].ColumnName : disp);

            cb.DataSource = dt;
            if (!string.IsNullOrWhiteSpace(disp)) cb.DisplayMember = disp;
            if (!string.IsNullOrWhiteSpace(val)) cb.ValueMember = val;

            if (dt.Rows.Count > 0) cb.SelectedIndex = 0;
        }

        private static int ToInt(DataRow r, string col)
        {
            return r.Table.Columns.Contains(col) && r[col] != DBNull.Value
                ? Convert.ToInt32(r[col])
                : 0;
        }

        private static string ToStr(DataRow r, params string[] cols)
        {
            foreach (var col in cols)
            {
                if (r.Table.Columns.Contains(col) && r[col] != DBNull.Value)
                    return Convert.ToString(r[col]);
            }
            return "";
        }
    }
}