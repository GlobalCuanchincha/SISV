using Capa_Corte_Transversal.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Form_Ordenes_Servicio_Notificacion : Form
    {
        private object _session;
        private int _usuarioId = 0;
        private byte _rolId = 0;

        private int _ordenSeleccionadaId = 0;

        private readonly Timer _debounceBuscar = new Timer { Interval = 300 };

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
                _rolId = TryGetRolSesionId(_session);
                if (_usuarioId <= 0) _usuarioId = 1;

                HookNavegacion();
                SetHeaderActive("taller");

                // Debounce buscar (ajusta nombres si difieren)
                var txtBuscar = FindControl<TextBox>("txt_Buscador_Taller", "txt_Buscador_Notificacion", "txt_Buscador");
                if (txtBuscar != null)
                {
                    _debounceBuscar.Tick += async (s, e) =>
                    {
                        _debounceBuscar.Stop();
                        await BuscarOrdenesAsync();
                    };

                    txtBuscar.TextChanged += (s, e) =>
                    {
                        _debounceBuscar.Stop();
                        _debounceBuscar.Start();
                    };
                }

                var cmbFiltroEstado = FindControl<ComboBox>("Cmbox_EstadoFiltro_Taller", "Cmbox_EstadoFiltro_Notificacion", "cmb_EstadoFiltro");
                if (cmbFiltroEstado != null)
                    cmbFiltroEstado.SelectedIndexChanged += async (s, e) => await BuscarOrdenesAsync();

                // Cargar estados (filtro + editor)
                await CargarEstadosAsync();

                // Buscar
                await BuscarOrdenesAsync();

                // Hook guardar (diagnóstico/solución/estado)
                var btnGuardar = FindControl<Button>("btn_Guardar_Taller", "btn_Actualizar_Taller", "btn_Guardar_Notificacion");
                if (btnGuardar != null)
                    btnGuardar.Click += async (s, e) => await GuardarTallerAsync();

                // Hook registrar notificación
                var btnNotif = FindControl<Button>("btn_EnviarNotificacion_Taller", "btn_Enviar_Notificacion");
                if (btnNotif != null)
                    btnNotif.Click += async (s, e) => await RegistrarNotificacionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Taller", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Navegación + botón activo
        // =========================

        private void HookNavegacion()
        {
            var btnEquipos = FindControl<Button>("btn_Equipos_Taller", "btn_Equipos_Notificacion");
            if (btnEquipos != null) btnEquipos.Click += (s, e) => NavegarAEquipos();

            var btnRecepcion = FindControl<Button>("btn_Recepcion_Taller", "btn_Recepcion_Notificacion");
            if (btnRecepcion != null) btnRecepcion.Click += (s, e) => NavegarARecepcion();
        }

        private void NavegarAEquipos()
        {
            var principal = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
            var f = new Form_Ordenes_Servicio(_session);

            if (principal != null) principal.OpenChild(f, "Órdenes de servicio", "Equipos");
            else f.Show();
        }

        private void NavegarARecepcion()
        {
            var principal = Application.OpenForms.OfType<Form_Panel_Principal>().FirstOrDefault();
            var f = new Form_Ordenes_Servicio_Recepcion(_session);

            if (principal != null) principal.OpenChild(f, "Órdenes de servicio", "Recepción / Solicitud");
            else f.Show();
        }

        private void SetHeaderActive(string active)
        {
            var btnEquipos = FindControl<Control>("btn_Equipos_Taller", "btn_Equipos_Notificacion", "btn_Equipos");
            var btnRecepcion = FindControl<Control>("btn_Recepcion_Taller", "btn_Recepcion_Notificacion", "btn_Recepcion");
            var btnTaller = FindControl<Control>("btn_Taller_Taller", "btn_Taller_Notificacion", "btn_Taller");

            ApplyTabStyle(btnEquipos, active == "equipos");
            ApplyTabStyle(btnRecepcion, active == "recepcion");
            ApplyTabStyle(btnTaller, active == "taller");
        }

        private void ApplyTabStyle(Control btn, bool active)
        {
            if (btn == null) return;

            btn.BackColor = active ? Color.White : Color.FromArgb(245, 245, 245);
            btn.ForeColor = active ? Color.Black : Color.FromArgb(90, 90, 90);

            var pFill = btn.GetType().GetProperty("FillColor");
            if (pFill != null) pFill.SetValue(btn, btn.BackColor, null);

            var pBorder = btn.GetType().GetProperty("BorderColor");
            if (pBorder != null) pBorder.SetValue(btn, active ? Color.FromArgb(30, 144, 255) : Color.Transparent, null);

            var pBorderThickness = btn.GetType().GetProperty("BorderThickness");
            if (pBorderThickness != null) pBorderThickness.SetValue(btn, active ? 2 : 0, null);
        }

        // =========================
        // Estados
        // =========================

        private async Task CargarEstadosAsync()
        {
            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Estados_Listar", null);

            var cmbFiltro = FindControl<ComboBox>("Cmbox_EstadoFiltro_Taller", "Cmbox_EstadoFiltro_Notificacion", "cmb_EstadoFiltro");
            if (cmbFiltro != null)
            {
                cmbFiltro.DataSource = dt.Copy();
                cmbFiltro.DisplayMember = "EstadoNombre";
                cmbFiltro.ValueMember = "EstadoValor";
                if (dt.Rows.Count > 0) cmbFiltro.SelectedIndex = 0;
            }

            var cmbEstado = FindControl<ComboBox>("cmbox_Estado_Taller", "cmbox_Estado_Notificacion", "cmb_Estado");
            if (cmbEstado != null)
            {
                // En taller normalmente NO quieres "Todos" (-1) en el editor,
                // por eso filtramos:
                var view = new DataView(dt);
                view.RowFilter = "EstadoValor <> -1";

                cmbEstado.DataSource = view;
                cmbEstado.DisplayMember = "EstadoNombre";
                cmbEstado.ValueMember = "EstadoValor";
                if (view.Count > 0) cmbEstado.SelectedIndex = 0;
            }
        }

        // =========================
        // Buscar / Render
        // =========================

        private async Task BuscarOrdenesAsync()
        {
            var txtBuscar = FindControl<TextBox>("txt_Buscador_Taller", "txt_Buscador_Notificacion", "txt_Buscador");
            var cmbFiltro = FindControl<ComboBox>("Cmbox_EstadoFiltro_Taller", "Cmbox_EstadoFiltro_Notificacion", "cmb_EstadoFiltro");
            var flow = FindControl<FlowLayoutPanel>("flowTaller", "flowNotificacion", "flowOrdenes");

            if (flow == null) return;

            string buscar = txtBuscar != null ? (txtBuscar.Text ?? "").Trim() : "";
            short estadoValor = -1;
            if (cmbFiltro != null && cmbFiltro.SelectedValue != null)
                estadoValor = Convert.ToInt16(cmbFiltro.SelectedValue);

            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Buscar", cmd =>
            {
                cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar);
                cmd.Parameters.AddWithValue("@EstadoValor", estadoValor);
                cmd.Parameters.AddWithValue("@Top", 200);
            });

            RenderFlow(flow, dt);

            var lblResultados = FindControl<Label>("lbl_Resultados_Taller", "lbl_Resultados_Notificacion", "lbl_Resultados");
            if (lblResultados != null) lblResultados.Text = "resultados " + dt.Rows.Count;
        }

        private void RenderFlow(FlowLayoutPanel flow, DataTable dt)
        {
            flow.SuspendLayout();
            flow.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int ordenId = Convert.ToInt32(r["OrdenServicioID"]);
                string codigo = Convert.ToString(r["CodigoOrden"]);
                string cliente = Convert.ToString(r["ClienteNombre"]);
                string equipo = Convert.ToString(r["EquipoNombre"]);
                string tecnico = Convert.ToString(r["TecnicoNombre"]);
                string estado = Convert.ToString(r["EstadoNombre"]);

                // Card simple (sin depender de tu UserControl)
                var p = new Panel();
                p.Height = 56;
                p.Width = Math.Max(250, flow.ClientSize.Width - 22);
                p.BackColor = (ordenId == _ordenSeleccionadaId) ? Color.FromArgb(230, 242, 255) : Color.White;
                p.BorderStyle = BorderStyle.FixedSingle;
                p.Tag = ordenId;

                var lbl = new Label();
                lbl.AutoSize = false;
                lbl.Dock = DockStyle.Fill;
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Padding = new Padding(12, 0, 12, 0);
                lbl.Text = string.Format("{0}   |   {1}   |   {2}   |   {3}", codigo, cliente, equipo, estado);

                p.Controls.Add(lbl);

                p.Click += async (s, e) => await SeleccionarOrdenAsync(ordenId);
                lbl.Click += async (s, e) => await SeleccionarOrdenAsync(ordenId);

                flow.Controls.Add(p);
            }

            flow.ResumeLayout();
        }

        private async Task SeleccionarOrdenAsync(int ordenId)
        {
            _ordenSeleccionadaId = ordenId;

            var flow = FindControl<FlowLayoutPanel>("flowTaller", "flowNotificacion", "flowOrdenes");
            if (flow != null)
            {
                foreach (Control c in flow.Controls)
                {
                    var pnl = c as Panel;
                    if (pnl != null)
                    {
                        int id = pnl.Tag != null ? Convert.ToInt32(pnl.Tag) : 0;
                        pnl.BackColor = (id == _ordenSeleccionadaId) ? Color.FromArgb(230, 242, 255) : Color.White;
                    }
                }
            }

            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_GetById", cmd =>
            {
                cmd.Parameters.AddWithValue("@OrdenServicioID", ordenId);
            });

            if (dt.Rows.Count == 0) return;

            var row = dt.Rows[0];

            SetText("lbl_CodigoOrden_Taller", Convert.ToString(row["CodigoOrden"]));
            SetText("lbl_Cliente_Taller", Convert.ToString(row["ClienteNombre"]));
            SetText("lbl_Equipo_Taller", Convert.ToString(row["EquipoNombre"]));

            var cmbEstado = FindControl<ComboBox>("cmbox_Estado_Taller", "cmbox_Estado_Notificacion", "cmb_Estado");
            if (cmbEstado != null && row["EstadoID"] != DBNull.Value)
                cmbEstado.SelectedValue = Convert.ToInt16(row["EstadoID"]);

            SetTextBox("txt_Diagnostico_Taller", Convert.ToString(row["Diagnostico"]));
            SetTextBox("txt_Solucion_Taller", Convert.ToString(row["Solucion"]));

            // historial (si tienes flow para esto)
            await CargarHistorialEstadoAsync();
        }

        private async Task CargarHistorialEstadoAsync()
        {
            var flowHist = FindControl<FlowLayoutPanel>("flowHistorial_Taller", "flowHistorialEstado");
            if (flowHist == null) return;
            if (_ordenSeleccionadaId <= 0) return;

            var dt = await ExecDataTableAsync("ops.usp_OrdenEstadoHistorial_Listar", cmd =>
            {
                cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
            });

            flowHist.SuspendLayout();
            flowHist.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                string estado = Convert.ToString(r["EstadoNombre"]);
                string fecha = Convert.ToString(r["Fecha"]);

                var item = new Label();
                item.AutoSize = true;
                item.Padding = new Padding(8);
                item.Text = string.Format("{0} - {1}", fecha, estado);

                flowHist.Controls.Add(item);
            }

            flowHist.ResumeLayout();
        }

        // =========================
        // Guardar taller
        // =========================

        private async Task GuardarTallerAsync()
        {
            try
            {
                if (_ordenSeleccionadaId <= 0)
                {
                    MessageBox.Show("Seleccione una orden.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var cmbEstado = FindControl<ComboBox>("cmbox_Estado_Taller", "cmbox_Estado_Notificacion", "cmb_Estado");
                short? estadoId = null;
                if (cmbEstado != null && cmbEstado.SelectedValue != null)
                {
                    short v = Convert.ToInt16(cmbEstado.SelectedValue);
                    if (v > 0) estadoId = v;
                }

                string diag = GetTextBox("txt_Diagnostico_Taller");
                string sol = GetTextBox("txt_Solucion_Taller");

                await ExecDataTableAsync("ops.usp_OrdenServicio_Taller_Guardar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@EstadoID", (object)estadoId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Diagnostico", string.IsNullOrWhiteSpace(diag) ? (object)DBNull.Value : diag);
                    cmd.Parameters.AddWithValue("@Solucion", string.IsNullOrWhiteSpace(sol) ? (object)DBNull.Value : sol);
                    cmd.Parameters.AddWithValue("@Cerrar", 0);
                });

                MessageBox.Show("Actualizado correctamente.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await BuscarOrdenesAsync();
                await SeleccionarOrdenAsync(_ordenSeleccionadaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Taller", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Registrar notificación
        // =========================

        private async Task RegistrarNotificacionAsync()
        {
            try
            {
                if (_ordenSeleccionadaId <= 0)
                {
                    MessageBox.Show("Seleccione una orden.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string email = GetTextBox("txt_EmailDestino_Taller");
                string msg = GetTextBox("txt_Mensaje_Taller");

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(msg))
                {
                    MessageBox.Show("Ingrese Email destino y Mensaje.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await ExecDataTableAsync("ops.usp_OrdenNotificacion_Registrar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId);
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@EmailDestino", email);
                    cmd.Parameters.AddWithValue("@Mensaje", msg);
                });

                MessageBox.Show("Notificación registrada (pendiente de envío).", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Taller", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // UI helpers
        // =========================

        private T FindControl<T>(params string[] names) where T : class
        {
            foreach (string n in names)
            {
                var arr = this.Controls.Find(n, true);
                if (arr != null && arr.Length > 0)
                {
                    var t = arr[0] as T;
                    if (t != null) return t;
                }
            }
            return null;
        }

        private void SetText(string labelName, string value)
        {
            var lbl = FindControl<Label>(labelName);
            if (lbl != null) lbl.Text = value ?? "";
        }

        private void SetTextBox(string txtName, string value)
        {
            var txt = FindControl<TextBox>(txtName);
            if (txt != null) txt.Text = value ?? "";
        }

        private string GetTextBox(string txtName)
        {
            var txt = FindControl<TextBox>(txtName);
            return txt != null ? (txt.Text ?? "").Trim() : "";
        }

        // =========================
        // Sesión helpers
        // =========================

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

        private byte TryGetRolSesionId(object session)
        {
            try
            {
                if (session == null) return 0;
                var t = session.GetType();
                var p = t.GetProperty("RoleId") ?? t.GetProperty("RoleID") ?? t.GetProperty("RolId") ?? t.GetProperty("RolID");
                if (p == null) return 0;
                var v = p.GetValue(session, null);
                if (v == null) return 0;
                return Convert.ToByte(v);
            }
            catch { return 0; }
        }

        // =========================
        // DB helper
        // =========================

        private static string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"] != null ? ConfigurationManager.ConnectionStrings["SISV"].ConnectionString : null;
            if (string.IsNullOrWhiteSpace(cs) && ConfigurationManager.ConnectionStrings["SISV_BD"] != null)
                cs = ConfigurationManager.ConnectionStrings["SISV_BD"].ConnectionString;
            if (string.IsNullOrWhiteSpace(cs) && ConfigurationManager.ConnectionStrings["DefaultConnection"] != null)
                cs = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

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
                if (fill != null) fill(cmd);

                await cn.OpenAsync();
                using (var rd = await cmd.ExecuteReaderAsync())
                    dt.Load(rd);
            }

            return dt;
        }
    }
}
