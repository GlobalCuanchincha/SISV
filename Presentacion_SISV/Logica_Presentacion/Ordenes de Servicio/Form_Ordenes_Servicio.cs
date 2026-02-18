using Capa_Corte_Transversal.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Equipos;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Ordenes_Servicio : Form
    {
        private readonly object _session;
        private readonly int _usuarioId;
        private readonly byte _rolId;

        private int _equipoSeleccionadoId = 0;

        private int? _clienteIdSeleccionado = null;
        private string _clienteNombreSeleccionado = null;

        private readonly Timer _debounceBuscar = new Timer { Interval = 350 };
        private bool _isLoading = false;

        // =========================
        // NAV UI (activo + animación)
        // =========================
        private Control _navBtnActivo;
        private Panel _navIndicator;
        private readonly Timer _navAnim = new Timer { Interval = 15 };
        private int _navTargetTop;
        private int _navTargetHeight;

        private readonly Dictionary<Control, BtnStyle> _navStyles = new Dictionary<Control, BtnStyle>();

        private readonly Color _navActiveColor = Color.FromArgb(28, 188, 135);      // Verde (como módulo órdenes)
        private readonly Color _navActiveBg = Color.FromArgb(232, 250, 240);        // Verde claro
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
            _usuarioId = TryGetUsuarioSesionId();
            _rolId = TryGetRolSesionId();

            // ✅ NAV: eventos aquí (no en Load)
            btn_Recepcion_Equipos.Click += (s, e) => IrARecepcion();
            btn_Notificacion_Equipos.Click += (s, e) => IrANotificacion();

            // Anim timer
            _navAnim.Tick += (s, e) => NavAnimTick();

            Load += async (s, e) => await Form_LoadAsync();
        }

        private async Task Form_LoadAsync()
        {
            // Roles permitidos: 1 SuperAdmin, 2 Admin, 4 Técnico
            if (_usuarioId <= 0)
            {
                MessageBox.Show("No se pudo obtener UsuarioID de sesión.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // ✅ evita CreateHandle crash
                BeginInvoke(new Action(() => Close()));
                return;
            }

            if (_rolId != 1 && _rolId != 2 && _rolId != 4)
            {
                MessageBox.Show("Acceso denegado. Solo SuperAdministrador, Administrador y Técnico.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                BeginInvoke(new Action(() => Close()));
                return;
            }

            _isLoading = true;

            // Events búsqueda
            _debounceBuscar.Tick += async (s, e) =>
            {
                _debounceBuscar.Stop();
                await BuscarEquiposAsync();
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
                await BuscarEquiposAsync();
            };

            btn_Selecciona_Clientes_Equipo.Click += async (s, e) => await ElegirClienteAsync();
            btn_Guardar_Equipos.Click += async (s, e) => await GuardarEquipoAsync();
            btn_Limpiar_Equipos.Click += async (s, e) => await LimpiarEquipoAsync();

            // Ajuste cards en resize
            Flow_OrdenServicio_Equipos.SizeChanged += (s, e) => AjustarAnchoCards();

            // Cargar combos
            await CargarCombosAsync();

            _isLoading = false;

            await LimpiarEquipoAsync();
            await BuscarEquiposAsync();

            // ✅ NAV: marca botón activo en esta pantalla (Equipos)
            // Si existe un botón llamado "btn_Equipos" lo usa, si no, deja activo el botón de Equipos (fallback).
            var btnEquipos = FindControlDeep("btn_Equipos");
            if (btnEquipos != null)
                ActivateNav(btnEquipos);
            else
                ActivateNav(btn_Recepcion_Equipos); // fallback si tu diseñador no tiene btn_Equipos
        }

        // ======================================================
        // NAVEGACIÓN: abre en el panel del Form_Panel_Principal
        // ======================================================
        private void IrARecepcion()
        {
            try
            {
                ActivateNav(btn_Recepcion_Equipos);

                // ✅ abre dentro del panel host (Form_Panel_Principal)
                OpenInMainHost(
                    CreateFormFromAnyCtor(new[]
                    {
                        "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Recepcion",
                        "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Solicitud"
                    }),
                    "Recepción",
                    "Recepción / Solicitud de orden"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void IrANotificacion()
        {
            try
            {
                ActivateNav(btn_Notificacion_Equipos);

                OpenInMainHost(
                    CreateFormFromAnyCtor(new[]
                    {
                        "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Notificacion",
                        "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Notificaciones",
                        "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Solicitud"
                    }),
                    "Notificación",
                    "Actualizar estado / notificaciones del servicio"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenInMainHost(Form child, string titulo, string descripcion)
        {
            if (child == null)
            {
                MessageBox.Show("No se encontró el formulario destino (revisa nombre de clase/namespace).", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Este form está dentro de Panel_Escritorio, así que el Form real es el principal:
            var main = this.Parent?.FindForm() as Union_Formularios_SISV.Form_Panel_Principal;

            if (main != null)
            {
                main.OpenChild(child, titulo, descripcion);
                return;
            }

            // Fallback: si por alguna razón no está en el host
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

            // Asegura que sea Form
            if (!typeof(Form).IsAssignableFrom(t)) return null;

            // 1) ctor(object session)
            var ctorObj = t.GetConstructor(new[] { typeof(object) });
            if (ctorObj != null)
                return (Form)ctorObj.Invoke(new object[] { _session });

            // 2) ctor(int usuarioId)
            var ctorInt = t.GetConstructor(new[] { typeof(int) });
            if (ctorInt != null)
                return (Form)ctorInt.Invoke(new object[] { _usuarioId });

            // 3) ctor() vacío
            var ctorEmpty = t.GetConstructor(Type.EmptyTypes);
            if (ctorEmpty != null)
                return (Form)ctorEmpty.Invoke(null);

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

        // ======================================================
        // NAV UI (activo + animación)
        // ======================================================
        private void ActivateNav(Control btn)
        {
            if (btn == null) return;

            // Guarda estilo original si no existe
            if (!_navStyles.ContainsKey(btn))
            {
                _navStyles[btn] = new BtnStyle
                {
                    Back = btn.BackColor,
                    Fore = btn.ForeColor,
                    Font = btn.Font
                };
            }

            // Restaura anterior
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

            // Activo
            SetFillOrBack(btn, _navActiveBg);
            btn.ForeColor = _navActiveColor;
            btn.Font = new Font(btn.Font, FontStyle.Bold);

            TrySetProp(btn, "BorderColor", _navActiveColor);
            TrySetProp(btn, "BorderThickness", 1);

            EnsureIndicator(btn);

            // Target anim
            _navTargetTop = btn.Top;
            _navTargetHeight = btn.Height;

            if (_navIndicator.Top == _navTargetTop && _navIndicator.Height == _navTargetHeight)
                return;

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

            // Top
            int dy = _navTargetTop - _navIndicator.Top;
            if (Math.Abs(dy) <= speed) _navIndicator.Top = _navTargetTop;
            else _navIndicator.Top += Math.Sign(dy) * speed;

            // Height
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

            // Para Guna2Button / controles con FillColor
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

        private Control FindControlDeep(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var found = this.Controls.Find(name, true);
            if (found != null && found.Length > 0) return found[0];

            return null;
        }

        // =========================
        // COMBOS
        // =========================
        private async Task CargarCombosAsync()
        {
            var dtFiltros = await TryExecDataTableAsync(
                new[] { "ops.usp_Equipo_Filtros_Listar", "dbo.usp_Equipo_Filtros_Listar" },
                cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
            );

            cmbox_Filtrarpor_Equipos.DisplayMember = "Text";
            cmbox_Filtrarpor_Equipos.ValueMember = "Value";
            cmbox_Filtrarpor_Equipos.DataSource = dtFiltros;

            var dtTipo = await TryExecDataTableAsync(
                new[] { "ops.usp_TipoEquipo_Listar", "dbo.usp_TipoEquipo_Listar" },
                cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
            );

            Cmbox_TipoEquipo_Equipos.DisplayMember = "TipoEquipoNombre";
            Cmbox_TipoEquipo_Equipos.ValueMember = "TipoEquipoID";
            Cmbox_TipoEquipo_Equipos.DataSource = dtTipo;

            var dtCon = await TryExecDataTableAsync(
                new[] { "ops.usp_Conectividad_Listar", "dbo.usp_Conectividad_Listar" },
                cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
            );

            cmbox_Conectividad_Equipos.DisplayMember = "Text";
            cmbox_Conectividad_Equipos.ValueMember = "Value";
            cmbox_Conectividad_Equipos.DataSource = dtCon;
        }

        // =========================
        // BUSCAR + FLOW
        // =========================
        private async Task BuscarEquiposAsync()
        {
            try
            {
                string buscar = (txt_Buscador_Items_Equipos.Text ?? "").Trim();
                string filtro = Convert.ToString(cmbox_Filtrarpor_Equipos.SelectedValue) ?? "todos";

                var dt = await TryExecDataTableAsync(
                    new[] { "ops.usp_Equipo_Buscar_v2", "dbo.usp_Equipo_Buscar_v2" },
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                        cmd.Parameters.AddWithValue("@ClienteID", (object)_clienteIdSeleccionado ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@FiltroPor", filtro);
                        cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar);
                        cmd.Parameters.AddWithValue("@SoloActivos", DBNull.Value);
                        cmd.Parameters.AddWithValue("@Top", 200);
                    }
                );

                RenderFlowEquipos(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Equipos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderFlowEquipos(DataTable dt)
        {
            Flow_OrdenServicio_Equipos.SuspendLayout();
            Flow_OrdenServicio_Equipos.Controls.Clear();

            int count = 0;

            foreach (DataRow r in dt.Rows)
            {
                int equipoId = Convert.ToInt32(r["EquipoID"]);
                string codigo = Convert.ToString(r["CodigoInterno"]);
                string nombreEquipo = Convert.ToString(r["NombreEquipo"]);
                string cliente = Convert.ToString(r["Cliente"]);
                string serie = Convert.ToString(r["Serie"]);

                var card = new EquiposTaskCard();
                card.Width = Flow_OrdenServicio_Equipos.ClientSize.Width - 22;
                card.Bind(equipoId, codigo, nombreEquipo, cliente, serie);
                card.SetSelected(equipoId == _equipoSeleccionadoId);

                card.EquipoSeleccionado += async (s, args) =>
                {
                    await SeleccionarEquipoAsync(args.EquipoID);
                };

                Flow_OrdenServicio_Equipos.Controls.Add(card);
                count++;
            }

            lbl_Contador_de_resultados_Equipos.Text = $"{count} resultados";
            Flow_OrdenServicio_Equipos.ResumeLayout();
        }

        private Form CrearForm(Type formType)
        {
            Form form;

            // 1) Preferir ctor(object session)
            var ctorSession = formType.GetConstructor(new[] { typeof(object) });
            if (ctorSession != null)
            {
                form = (Form)ctorSession.Invoke(new object[] { _session });
            }
            else
            {
                // 2) ctor(int usuarioId, byte rolId)
                var ctor = formType.GetConstructor(new[] { typeof(int), typeof(byte) });
                if (ctor != null)
                    form = (Form)ctor.Invoke(new object[] { _usuarioId, _rolId });
                else
                    form = (Form)Activator.CreateInstance(formType);
            }

            // Embebido
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;

            return form;
        }

        private void AjustarAnchoCards()
        {
            foreach (Control c in Flow_OrdenServicio_Equipos.Controls)
                if (c is EquiposTaskCard card)
                    card.Width = Flow_OrdenServicio_Equipos.ClientSize.Width - 22;
        }

        // =========================
        // SELECCIONAR EQUIPO
        // =========================
        private async Task SeleccionarEquipoAsync(int equipoId)
        {
            _equipoSeleccionadoId = equipoId;

            foreach (Control c in Flow_OrdenServicio_Equipos.Controls)
                if (c is EquiposTaskCard card)
                    card.SetSelected(card.EquipoID == _equipoSeleccionadoId);

            btn_Guardar_Equipos.Text = "Actualizar";

            try
            {
                var dt = await TryExecDataTableAsync(
                    new[] { "ops.usp_Equipo_GetById", "dbo.usp_Equipo_GetById" },
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                        cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                    }
                );

                if (dt.Rows.Count == 0) return;

                var row = dt.Rows[0];

                _clienteIdSeleccionado = row["ClienteID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ClienteID"]);
                _clienteNombreSeleccionado = Convert.ToString(row["ClienteNombre"]) ?? "";

                lbl_Cliente_Equipos.Text = _clienteNombreSeleccionado ?? "";
                lbl_CodigoInterno_Equipos.Text = Convert.ToString(row["CodigoInterno"]) ?? "";

                if (row["TipoEquipoID"] != DBNull.Value)
                    Cmbox_TipoEquipo_Equipos.SelectedValue = Convert.ToInt32(row["TipoEquipoID"]);

                txt_Marca_Equipos.Text = Convert.ToString(row["Marca"]) ?? "";
                txt_Modelo_Equipos.Text = Convert.ToString(row["Modelo"]) ?? "";
                txt_NumSerie_Equipos.Text = Convert.ToString(row["Serie"]) ?? "";
                txt_Color_Equipos.Text = Convert.ToString(row["Color"]) ?? "";

                var con = Convert.ToString(row["Conectividad"]) ?? "N/A";
                SelectComboValue(cmbox_Conectividad_Equipos, con);

                txt_Accesorios_Equipos.Text = Convert.ToString(row["Accesorios"]) ?? "";
                txt_Observaciones.Text = Convert.ToString(row["Observaciones"]) ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Equipos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // ELEGIR CLIENTE
        // =========================
        private async Task ElegirClienteAsync()
        {
            try
            {
                using (var f = new Seleccion_Cliente(_session))
                {
                    var dr = f.ShowDialog(this);
                    if (dr == DialogResult.OK && f.SelectedClienteID.HasValue)
                    {
                        _clienteIdSeleccionado = f.SelectedClienteID.Value;
                        _clienteNombreSeleccionado = f.SelectedClienteNombre ?? "";

                        lbl_Cliente_Equipos.Text = _clienteNombreSeleccionado;

                        await BuscarEquiposAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Seleccionar Cliente", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // LIMPIAR / NUEVO
        // =========================
        private async Task LimpiarEquipoAsync()
        {
            _equipoSeleccionadoId = 0;

            foreach (Control c in Flow_OrdenServicio_Equipos.Controls)
                if (c is EquiposTaskCard card)
                    card.SetSelected(false);

            btn_Guardar_Equipos.Text = "Guardar";

            try
            {
                var dt = await TryExecDataTableAsync(
                    new[] { "ops.usp_Equipo_GenerarCodigoInterno", "dbo.usp_Equipo_GenerarCodigoInterno" },
                    cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
                );

                if (dt.Rows.Count > 0 && dt.Columns.Contains("CodigoInternoSugerido"))
                    lbl_CodigoInterno_Equipos.Text = Convert.ToString(dt.Rows[0]["CodigoInternoSugerido"]) ?? "";
                else
                    lbl_CodigoInterno_Equipos.Text = "";
            }
            catch
            {
                lbl_CodigoInterno_Equipos.Text = "";
            }

            txt_Marca_Equipos.Text = "";
            txt_Modelo_Equipos.Text = "";
            txt_NumSerie_Equipos.Text = "";
            txt_Color_Equipos.Text = "";
            txt_Accesorios_Equipos.Text = "";
            txt_Observaciones.Text = "";

            if (Cmbox_TipoEquipo_Equipos.Items.Count > 0) Cmbox_TipoEquipo_Equipos.SelectedIndex = 0;
            if (cmbox_Conectividad_Equipos.Items.Count > 0) cmbox_Conectividad_Equipos.SelectedIndex = 0;

            await Task.CompletedTask;
        }

        // =========================
        // GUARDAR (SP ops.usp_Equipo_Guardar)
        // =========================
        private async Task GuardarEquipoAsync()
        {
            try
            {
                if (!_clienteIdSeleccionado.HasValue || _clienteIdSeleccionado.Value <= 0)
                {
                    MessageBox.Show("Seleccione un cliente para el equipo.", "SISV",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (Cmbox_TipoEquipo_Equipos.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione el tipo de equipo.", "SISV",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int tipoEquipoId = Convert.ToInt32(Cmbox_TipoEquipo_Equipos.SelectedValue);

                string codigoInterno = (lbl_CodigoInterno_Equipos.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(codigoInterno))
                {
                    MessageBox.Show("No se generó el código interno.", "SISV",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string marca = NullIfEmpty(txt_Marca_Equipos.Text);
                string modelo = NullIfEmpty(txt_Modelo_Equipos.Text);
                string serie = NullIfEmpty(txt_NumSerie_Equipos.Text);
                string color = NullIfEmpty(txt_Color_Equipos.Text);

                string conectividad = Convert.ToString(cmbox_Conectividad_Equipos.SelectedValue) ?? "N/A";

                string accesorios = NullIfEmpty(txt_Accesorios_Equipos.Text);
                string observ = NullIfEmpty(txt_Observaciones.Text);

                bool esNuevo = _equipoSeleccionadoId <= 0;

                var dt = await TryExecDataTableAsync(
                    new[] { "ops.usp_Equipo_Guardar", "dbo.usp_Equipo_Guardar" },
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                        cmd.Parameters.AddWithValue("@EquipoID", esNuevo ? (object)DBNull.Value : _equipoSeleccionadoId);
                        cmd.Parameters.AddWithValue("@ClienteID", _clienteIdSeleccionado.Value);
                        cmd.Parameters.AddWithValue("@TipoEquipoID", tipoEquipoId);
                        cmd.Parameters.AddWithValue("@CodigoInterno", codigoInterno);

                        cmd.Parameters.AddWithValue("@Marca", (object)marca ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Modelo", (object)modelo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Serie", (object)serie ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Color", (object)color ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Conectividad", (object)conectividad ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@Accesorios", (object)accesorios ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Observaciones", (object)observ ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@Activo", 1);
                    }
                );

                int idGuardado = esNuevo ? Convert.ToInt32(dt.Rows[0]["EquipoID"]) : _equipoSeleccionadoId;
                _equipoSeleccionadoId = idGuardado;

                MessageBox.Show(esNuevo ? "Equipo registrado." : "Equipo actualizado.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                await BuscarEquiposAsync();
                await SeleccionarEquipoAsync(_equipoSeleccionadoId);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message, "SISV - Equipos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Equipos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Helpers (Session / DB / UI)
        // =========================
        private int TryGetUsuarioSesionId()
        {
            try
            {
                if (_session == null) return 0;
                return SessionHelper.GetUsuarioID(_session);
            }
            catch { return 0; }
        }

        private byte TryGetRolSesionId()
        {
            try
            {
                if (_session == null) return 0;

                var t = _session.GetType();
                var p = t.GetProperty("RoleId") ?? t.GetProperty("RoleID") ?? t.GetProperty("RolId") ?? t.GetProperty("RolID");
                if (p == null) return 0;

                var v = p.GetValue(_session, null);
                if (v == null) return 0;

                return Convert.ToByte(v);
            }
            catch { return 0; }
        }

        private static string NullIfEmpty(string s)
        {
            s = (s ?? "").Trim();
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        private static void SelectComboValue(ComboBox cb, string value)
        {
            if (cb == null) return;
            for (int i = 0; i < cb.Items.Count; i++)
            {
                var itemVal = cb.GetItemText(cb.Items[i]);
                if (string.Equals(Convert.ToString(cb.Items[i]), value, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(Convert.ToString(cb.SelectedValue), value, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(itemVal, value, StringComparison.OrdinalIgnoreCase))
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }
        }

        private static async Task<DataTable> TryExecDataTableAsync(string[] sps, Action<SqlCommand> fillParams)
        {
            Exception last = null;
            foreach (var sp in sps)
            {
                try
                {
                    return await ExecDataTableAsync(sp, fillParams);
                }
                catch (SqlException ex)
                {
                    last = ex;
                    continue;
                }
            }
            throw last ?? new Exception("No se pudo ejecutar el procedimiento almacenado.");
        }

        private static string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString
                  ?? ConfigurationManager.ConnectionStrings["SISV_BD"]?.ConnectionString
                  ?? ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(cs)) return cs;

            if (ConfigurationManager.ConnectionStrings.Count > 0)
                return ConfigurationManager.ConnectionStrings[0].ConnectionString;

            throw new Exception("No se encontró ConnectionString en App.config.");
        }

        private static async Task<DataTable> ExecDataTableAsync(string sp, Action<SqlCommand> fillParams)
        {
            var dt = new DataTable();
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fillParams?.Invoke(cmd);

                await cn.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                    dt.Load(rd);
            }
            return dt;
        }
    }
}
