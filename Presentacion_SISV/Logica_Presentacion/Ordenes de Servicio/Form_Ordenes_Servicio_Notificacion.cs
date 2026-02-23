using Capa_Corte_Transversal.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
