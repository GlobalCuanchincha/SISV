using Capa_Corte_Transversal.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Form_Ordenes_Servicio_Notificacion : Form
    {
        private object _session;
        private int _usuarioId = 0;

        private int _ordenSeleccionadaId = 0;
        private string _codigoOrdenSeleccionada = "";

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

        public Form_Ordenes_Servicio_Notificacion()
        {
            InitializeComponent();
            Shown += async (s, e) => await InicializarAsync();
        }

        public Form_Ordenes_Servicio_Notificacion(object session)
        {
            InitializeComponent();
            _session = session;
            Shown += async (s, e) => await InicializarAsync();

            btn_Recepcion_Notificacion.Click += (s, e) => IrARecepcion();
            btn_Equipos_Notificacion.Click += (s, e) => IrAEquipos();
        }

        private async Task InicializarAsync()
        {
            try
            {
                if (_session == null) _session = GetSessionFromPrincipal();

                _usuarioId = TryGetUsuarioSesionId(_session);
                if (_usuarioId <= 0) _usuarioId = 1;

                // Hooks
                btn_Seleccionar_Orden_Diagnostico.Click += async (s, e) => await AbrirSeleccionOrdenAsync();

                btn_GuardarDiagnostico_Diagnostico.Click += async (s, e) => await GuardarDiagnosticoAsync();
                btn_GuardarEstado_Diagnostico.Click += async (s, e) => await GuardarEstadoAsync();

                btn_Previsualizar_Notificacion.Click += (s, e) => PrevisualizarNotificacion();
                btn_Enviar_Notificacion.Click += async (s, e) => await EnviarNotificacionAsync();

                // Estados
                await CargarEstadosAsync();

                // Estado UI inicial
                LimpiarPantallaOrden();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Notificación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // 1) Selección de orden
        // =========================================================
        private async Task AbrirSeleccionOrdenAsync()
        {
            try
            {
                using (var f = new Seleccion_Orden(_session))
                {
                    var dr = f.ShowDialog(this);
                    if (dr != DialogResult.OK) return;

                    _ordenSeleccionadaId = f.OrdenServicioIDSeleccionado;
                    if (_ordenSeleccionadaId <= 0) return;

                    await CargarDetalleOrdenAsync(_ordenSeleccionadaId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Selección de orden", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CargarDetalleOrdenAsync(int ordenServicioId)
        {
            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_GetDetalle_Notificacion", cmd =>
            {
                cmd.Parameters.AddWithValue("@OrdenServicioID", ordenServicioId);
            });

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No se encontró la orden seleccionada.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimpiarPantallaOrden();
                return;
            }

            var r = dt.Rows[0];

            _codigoOrdenSeleccionada = Convert.ToString(r["CodigoOrden"]);

            txt_OrdenSeleccionada_Diagnostico.Text = _codigoOrdenSeleccionada;
            txt_Cliente_Diagnostico.Text = Convert.ToString(r["Cliente"]);
            txt_TipoEquipo_Diagnostico.Text = Convert.ToString(r["Equipo"]);
            txt_Diagnostico_Diagnostico.Text = Convert.ToString(r["Diagnostico"]);

            lbl_EstadoEquipo_Diagnostico.Text = Convert.ToString(r["EstadoNombre"]);

            if (r["EstadoID"] != DBNull.Value)
            {
                var estadoId = Convert.ToInt16(r["EstadoID"]);
                if (Cmbox_Estado_Diagnostico.DataSource != null)
                    Cmbox_Estado_Diagnostico.SelectedValue = estadoId;
            }

            // Notificación: correo cliente
            var correo = Convert.ToString(r["CorreoCliente"]);
            txt_Correo_Notificacion.Text = correo;
            lbl_CorreoCliente_Notificacion.Text = correo;

            // Asunto por defecto (si está vacío)
            if (string.IsNullOrWhiteSpace(txt_Asunto_Notificacion.Text))
                txt_Asunto_Notificacion.Text = $"Actualización de orden {_codigoOrdenSeleccionada}";

            // Reset labels de actualización (visualmente “cargado”)
            SetUpdateLabelsDiagnostico(DateTime.Now);
            SetUpdateLabelsNotificacion(DateTime.Now);
        }

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


        private void IrARecepcion()
        {
            try
            {
                ActivateNav(btn_Recepcion_Notificacion);

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
        private void IrAEquipos()
        {
            try
            {
                ActivateNav(btn_Equipos_Notificacion);

                OpenInMainHost(
                    CreateFormFromAnyCtor(new[]
                    {
                "Union_Formularios_SISV.Forms.Form_Ordenes_Servicio",
                "Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Solicitud"
                    }),
                    "Equipos",
                    "Gestión de equipos / órdenes"
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

        private void LimpiarPantallaOrden()
        {
            _ordenSeleccionadaId = 0;
            _codigoOrdenSeleccionada = "";

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

        // =========================================================
        // 2) Estados
        // =========================================================
        private async Task CargarEstadosAsync()
        {
            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Estados_Listar", null);

            // Editor: sin “Todos”
            var view = new DataView(dt);
            view.RowFilter = "EstadoValor <> -1";

            Cmbox_Estado_Diagnostico.DataSource = view;
            Cmbox_Estado_Diagnostico.DisplayMember = "EstadoNombre";
            Cmbox_Estado_Diagnostico.ValueMember = "EstadoValor";
        }

        // =========================================================
        // 3) Guardar diagnóstico
        // =========================================================
        private async Task GuardarDiagnosticoAsync()
        {
            try
            {
                if (_ordenSeleccionadaId <= 0)
                {
                    MessageBox.Show("Primero seleccione una orden.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string diag = (txt_Diagnostico_Diagnostico.Text ?? "").Trim();
                if (string.IsNullOrWhiteSpace(diag))
                {
                    MessageBox.Show("Ingrese el diagnóstico.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await ExecNonQueryAsync("ops.usp_OrdenServicio_GuardarDiagnostico", cmd =>
                {
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@Diagnostico", diag);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                });

                MessageBox.Show("Diagnóstico guardado correctamente.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetUpdateLabelsDiagnostico(DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Diagnóstico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // 4) Guardar estado
        // =========================================================
        private async Task GuardarEstadoAsync()
        {
            try
            {
                if (_ordenSeleccionadaId <= 0)
                {
                    MessageBox.Show("Primero seleccione una orden.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (Cmbox_Estado_Diagnostico.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un estado.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int nuevoEstadoId = Convert.ToInt32(Cmbox_Estado_Diagnostico.SelectedValue);

                await ExecNonQueryAsync("ops.usp_OrdenServicio_ActualizarEstado", cmd =>
                {
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@NuevoEstadoID", nuevoEstadoId);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                });

                lbl_EstadoEquipo_Diagnostico.Text = Convert.ToString(((DataRowView)Cmbox_Estado_Diagnostico.SelectedItem)["EstadoNombre"]);

                MessageBox.Show("Estado actualizado correctamente.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetUpdateLabelsDiagnostico(DateTime.Now);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Estado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // 5) Previsualizar / Enviar notificación
        // =========================================================
        private void PrevisualizarNotificacion()
        {
            if (_ordenSeleccionadaId <= 0)
            {
                MessageBox.Show("Primero seleccione una orden.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string correo = (txt_Correo_Notificacion.Text ?? "").Trim();
            string asunto = (txt_Asunto_Notificacion.Text ?? "").Trim();
            string mensaje = (txt_Mensaje_Notificacion.Text ?? "").Trim();

            MessageBox.Show(
                $"Para: {correo}\nAsunto: {asunto}\n\n{mensaje}",
                "Previsualización",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private async Task EnviarNotificacionAsync()
        {
            if (_ordenSeleccionadaId <= 0)
            {
                MessageBox.Show("Primero seleccione una orden.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string correo = (txt_Correo_Notificacion.Text ?? "").Trim();
            string asunto = (txt_Asunto_Notificacion.Text ?? "").Trim();
            string mensaje = (txt_Mensaje_Notificacion.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(asunto) || string.IsNullOrWhiteSpace(mensaje))
            {
                MessageBox.Show("Complete Correo, Asunto y Mensaje.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string estadoEnvio = "ENVIADO";
            string errorDetalle = null;

            try
            {
                await EnviarCorreoAsync(correo, asunto, mensaje);
            }
            catch (Exception ex)
            {
                estadoEnvio = "ERROR";
                errorDetalle = ex.Message;
            }

            // Registrar en BD SIEMPRE (éxito o error)
            int notifId = 0;
            try
            {
                object o = await ExecScalarAsync("ops.usp_OrdenServicio_RegistrarNotificacion", cmd =>
                {
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    cmd.Parameters.AddWithValue("@Asunto", asunto);
                    cmd.Parameters.AddWithValue("@Mensaje", mensaje);
                    cmd.Parameters.AddWithValue("@EstadoEnvio", estadoEnvio);
                    cmd.Parameters.AddWithValue("@ErrorDetalle", (object)errorDetalle ?? DBNull.Value);
                });

                if (o != null && o != DBNull.Value)
                    notifId = Convert.ToInt32(o);
            }
            catch (Exception exDb)
            {
                MessageBox.Show(exDb.Message, "SISV - Registrar notificación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (estadoEnvio == "ENVIADO")
            {
                MessageBox.Show($"Notificación enviada y registrada. (ID: {notifId})", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    $"No se pudo enviar el correo.\n\nSe registró el intento (ID: {notifId}).\n\nDetalle: {errorDetalle}",
                    "SISV",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }

            SetUpdateLabelsNotificacion(DateTime.Now);
        }

        private static async Task EnviarCorreoAsync(string toEmail, string subject, string body)
        {
            string host = ConfigurationManager.AppSettings["Smtp.Host"];
            int port = int.Parse(ConfigurationManager.AppSettings["Smtp.Port"]);
            bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["Smtp.EnableSsl"]);

            string user = ConfigurationManager.AppSettings["Smtp.User"];
            string pass = ConfigurationManager.AppSettings["Smtp.Pass"];
            string fromEmail = ConfigurationManager.AppSettings["Smtp.FromEmail"];
            string fromName = ConfigurationManager.AppSettings["Smtp.FromName"];

            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(fromEmail, fromName);
                msg.To.Add(toEmail);
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                using (var smtp = new SmtpClient(host, port))
                {
                    smtp.EnableSsl = enableSsl;
                    smtp.Credentials = new NetworkCredential(user, pass);
                    await smtp.SendMailAsync(msg);
                }
            }
        }

        // =========================================================
        // UI: labels de actualización
        // =========================================================
        private void SetUpdateLabelsDiagnostico(DateTime dt)
        {
            lbl_HoraActualizacion_Diagnostico.Text = dt.ToString("HH:mm");
            lbl_diaActualizacion_Diagnostico.Text = dt.ToString("dd/MM/yyyy");
        }

        private void SetUpdateLabelsNotificacion(DateTime dt)
        {
            lbl_HoraActualizacion_Notificacion.Text = dt.ToString("HH:mm");
            lbl_diaActualizacion_Notificacion.Text = dt.ToString("dd/MM/yyyy");
        }

        // =========================================================
        // Sesión helpers
        // =========================================================
        private object GetSessionFromPrincipal()
        {
            try
            {
                var principal = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
                if (principal == null) return null;

                var field = principal.GetType().GetField("_session",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                return field != null ? field.GetValue(principal) : null;
            }
            catch { return null; }
        }

        private int TryGetUsuarioSesionId(object session)
        {
            try
            {
                if (session == null) return 0;
                return SessionHelper.GetUsuarioID(session);
            }
            catch { return 0; }
        }

        // =========================================================
        // DB helpers (solo SP)
        // =========================================================
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

        private static async Task<DataTable> ExecDataTableAsync(string sp, Action<SqlCommand> fill)
        {
            var dt = new DataTable();

            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fill?.Invoke(cmd);

                await cn.OpenAsync();

                using (var rd = await cmd.ExecuteReaderAsync())
                {
                    // Evita crash si el SP no retorna resultset
                    if (rd.FieldCount > 0)
                        dt.Load(rd);
                }
            }

            return dt;
        }

        private static async Task<int> ExecNonQueryAsync(string sp, Action<SqlCommand> fill)
        {
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fill?.Invoke(cmd);

                await cn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task<object> ExecScalarAsync(string sp, Action<SqlCommand> fill)
        {
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand(sp, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                fill?.Invoke(cmd);

                await cn.OpenAsync();
                return await cmd.ExecuteScalarAsync();
            }
        }
    }
}
