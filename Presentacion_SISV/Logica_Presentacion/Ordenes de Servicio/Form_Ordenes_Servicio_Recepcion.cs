using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Presentacion_SISV.Controls.Ordenes_de_Servicio.Equipos;
using Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Recepcion;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Form_Ordenes_Servicio_Recepcion : Form
    {
        private readonly object _session;

        private int _usuarioId = 1;
        private int _ordenSeleccionadaId = 0;

        private int? _clienteIdSeleccionado = null;

        private readonly Timer _debounceBuscar = new Timer { Interval = 300 };

        public Form_Ordenes_Servicio_Recepcion() : this(null) { }

        public Form_Ordenes_Servicio_Recepcion(object session)
        {
            InitializeComponent();
            _session = session;

            Shown += async (s, e) => await InicializarAsync();
        }

        private async Task InicializarAsync()
        {
            try
            {
                _usuarioId = TryGetUsuarioSesionId(_session) ?? 1;

                // Navegación
                if (btn_Equipos_Recepcion != null)
                    btn_Equipos_Recepcion.Click += (s, e) => NavegarA(new Form_Ordenes_Servicio(_session));

                if (btn_Taller_Recepcion != null)
                    btn_Taller_Recepcion.Click += (s, e) => NavegarA(new Form_Ordenes_Servicio_Notificacion(_session));

                MarcarBotonActivoRecepcion();

                // Debounce buscar
                _debounceBuscar.Tick += async (s, e) =>
                {
                    _debounceBuscar.Stop();
                    await BuscarOrdenesAsync();
                };

                txt_Buscador_Recepcion.TextChanged += (s, e) =>
                {
                    _debounceBuscar.Stop();
                    _debounceBuscar.Start();
                };

                Cmbox_EstadoFiltro_Recepcion.SelectedIndexChanged += async (s, e) => await BuscarOrdenesAsync();

                btn_Seleccionar_Cliente.Click += async (s, e) => await SeleccionarClienteAsync();
                btn_Nuevo_Recepcion.Click += async (s, e) => await NuevoAsync();

                btn_AsignarTecnico_Recepcion.Click += async (s, e) => await AsignarTecnicoAsync();
                btn_CrearOrden_Recepcion.Click += async (s, e) => await GuardarAsync();

                flowRecepcion.SizeChanged += (s, e) => AjustarAnchoCards();

                await CargarEstadosFiltroAsync();
                await CargarTecnicosAsync();

                await NuevoAsync();
                await BuscarOrdenesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // UI: Botón activo
        // =========================
        private void MarcarBotonActivoRecepcion()
        {
            // Inactivos
            SetNavStyle(btn_Equipos_Recepcion, false);
            SetNavStyle(btn_Taller_Recepcion, false);

            // Activo: puede llamarse btn_Recepcion, o ser un botón central sin evento.
            var btnCentro = FindControlRecursive(this, "btn_Recepcion")
                         ?? FindControlRecursive(this, "btn_Recepcion_Recepcion")
                         ?? FindControlRecursive(this, "btn_Solicitud_Recepcion");

            SetNavStyle(btnCentro, true);
        }

        private static Control FindControlRecursive(Control root, string name)
        {
            if (root == null) return null;
            var found = root.Controls.Find(name, true);
            return found != null && found.Length > 0 ? found[0] : null;
        }

        private static void SetNavStyle(Control c, bool active)
        {
            if (c == null) return;

            // Fuente
            try { c.Font = new Font(c.Font, active ? FontStyle.Bold : FontStyle.Regular); } catch { }

            // Soporta botones normales
            if (c is Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = active ? 2 : 1;
                b.BackColor = active ? Color.White : Color.FromArgb(245, 245, 245);
                return;
            }

            // Soporta Guna2Button u otros por reflexión
            TrySetProp(c, "FillColor", active ? Color.White : Color.FromArgb(245, 245, 245));
            TrySetProp(c, "BorderThickness", active ? 2 : 1);
            TrySetProp(c, "BorderColor", active ? Color.FromArgb(0, 120, 215) : Color.Gainsboro);
        }

        private static void TrySetProp(object obj, string prop, object value)
        {
            try
            {
                var pi = obj.GetType().GetProperty(prop);
                if (pi != null && pi.CanWrite) pi.SetValue(obj, value);
            }
            catch { }
        }

        // =========================
        // Navegación en el MISMO PANEL
        // =========================
        private void NavegarA(Form next)
        {
            var container = this.Parent;
            if (container == null)
            {
                next.StartPosition = FormStartPosition.CenterScreen;
                next.Show();
                this.Hide();
                return;
            }

            next.TopLevel = false;
            next.FormBorderStyle = FormBorderStyle.None;
            next.Dock = DockStyle.Fill;

            // Reemplazar el contenido del panel contenedor
            var old = container.Controls.Cast<Control>().ToArray();
            container.Controls.Clear();
            foreach (var c in old) c.Dispose();

            container.Controls.Add(next);
            next.Show();
        }

        // =========================
        // Cargar combos
        // =========================
        private async Task CargarEstadosFiltroAsync()
        {
            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Estados_Listar", null);

            Cmbox_EstadoFiltro_Recepcion.DataSource = dt;
            Cmbox_EstadoFiltro_Recepcion.DisplayMember = "EstadoNombre";
            Cmbox_EstadoFiltro_Recepcion.ValueMember = "EstadoValor";
            if (dt.Rows.Count > 0) Cmbox_EstadoFiltro_Recepcion.SelectedIndex = 0;
        }

        private async Task CargarTecnicosAsync()
        {
            var dt = await ExecDataTableAsync("ops.usp_Tecnico_ListarActivos", cmd =>
            {
                cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
            });

            cmbox_Tecnico_Recepcion.DataSource = dt;
            cmbox_Tecnico_Recepcion.DisplayMember = "TecnicoNombre";
            cmbox_Tecnico_Recepcion.ValueMember = "TecnicoID";
            if (dt.Rows.Count > 0) cmbox_Tecnico_Recepcion.SelectedIndex = 0;
        }

        private async Task CargarEquiposClienteAsync(int clienteId)
        {
            var dt = await ExecDataTableAsync("ops.usp_Equipo_ListarPorCliente", cmd =>
            {
                cmd.Parameters.AddWithValue("@ClienteID", clienteId);
            });

            if (!dt.Columns.Contains("EquipoID"))
                throw new Exception("ops.usp_Equipo_ListarPorCliente debe devolver columna 'EquipoID'.");

            if (!dt.Columns.Contains("EquipoTexto"))
                throw new Exception("ops.usp_Equipo_ListarPorCliente debe devolver columna 'EquipoTexto'.");

            var row = dt.NewRow();
            row["EquipoID"] = -1;
            row["EquipoTexto"] = "Seleccione...";
            dt.Rows.InsertAt(row, 0);

            cmbox_EquipoCliente_Recepcion.DataSource = dt;
            cmbox_EquipoCliente_Recepcion.DisplayMember = "EquipoTexto";
            cmbox_EquipoCliente_Recepcion.ValueMember = "EquipoID";
            cmbox_EquipoCliente_Recepcion.SelectedValue = -1;
        }

        // =========================
        // Nuevo / Generar Código
        // =========================
        private async Task NuevoAsync()
        {
            _ordenSeleccionadaId = 0;

            txt_Detalles_Recepcion.Clear();
            txt_AccesoriosRecibidos_Recepcion.Clear();

            _clienteIdSeleccionado = null;

            lbl_NombreCliente_Equipos.Text = "Sin selección";
            cmbox_EquipoCliente_Recepcion.DataSource = null;

            btn_CrearOrden_Recepcion.Text = "Crear orden";

            await GenerarCodigoAsync();

            foreach (Control c in flowRecepcion.Controls)
                if (c is RecepcionTaskCard card) card.SetSelected(false);
        }

        private async Task GenerarCodigoAsync()
        {
            var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_GenerarCodigo", null);

            if (dt.Rows.Count > 0 && dt.Columns.Contains("CodigoOrdenSugerido"))
                lbl_CodigoOrdenSolicitud_Recepcion.Text = Convert.ToString(dt.Rows[0]["CodigoOrdenSugerido"]);
            else
                lbl_CodigoOrdenSolicitud_Recepcion.Text = "OS-????";
        }

        // =========================
        // Buscar / Render flow
        // =========================
        private async Task BuscarOrdenesAsync()
        {
            try
            {
                string buscar = (txt_Buscador_Recepcion.Text ?? "").Trim();
                short estadoValor = GetComboInt16Value(Cmbox_EstadoFiltro_Recepcion, "EstadoValor", -1);

                var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Buscar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@Buscar", string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar);
                    cmd.Parameters.AddWithValue("@EstadoValor", estadoValor);
                    cmd.Parameters.AddWithValue("@Top", 200);
                });

                RenderFlow(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderFlow(DataTable dt)
        {
            flowRecepcion.SuspendLayout();
            flowRecepcion.Controls.Clear();

            int count = 0;

            foreach (DataRow r in dt.Rows)
            {
                int ordenId = Convert.ToInt32(r["OrdenServicioID"]);
                string codigo = Convert.ToString(r["CodigoOrden"]);
                string cliente = Convert.ToString(r["ClienteNombre"]);
                string equipo = Convert.ToString(r["EquipoNombre"]);
                string tecnico = Convert.ToString(r["TecnicoNombre"]);
                string estado = Convert.ToString(r["EstadoNombre"]);
                int? estadoId = dt.Columns.Contains("EstadoID") && r["EstadoID"] != DBNull.Value ? (int?)Convert.ToInt32(r["EstadoID"]) : null;

                var card = new RecepcionTaskCard();
                card.Width = Math.Max(200, flowRecepcion.ClientSize.Width - 22);

                card.Bind(ordenId, codigo, cliente, equipo, tecnico, estado);
                card.SetSelected(ordenId == _ordenSeleccionadaId);

                card.OrdenSeleccionada += async (s, args) => await SeleccionarOrdenAsync(args.OrdenServicioID);

                flowRecepcion.Controls.Add(card);
                count++;
            }

            lbl_Resultados_Recepcion.Text = $"{count} resultados";
            flowRecepcion.ResumeLayout();
        }

        private void AjustarAnchoCards()
        {
            foreach (Control c in flowRecepcion.Controls)
                if (c is RecepcionTaskCard card)
                    card.Width = Math.Max(200, flowRecepcion.ClientSize.Width - 22);
        }

        // =========================
        // Seleccionar orden
        // =========================
        private async Task SeleccionarOrdenAsync(int ordenServicioId)
        {
            _ordenSeleccionadaId = ordenServicioId;

            foreach (Control c in flowRecepcion.Controls)
                if (c is RecepcionTaskCard card)
                    card.SetSelected(card.OrdenServicioID == _ordenSeleccionadaId);

            try
            {
                var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_GetById", cmd =>
                {
                    cmd.Parameters.AddWithValue("@OrdenServicioID", ordenServicioId);
                });

                if (dt.Rows.Count == 0) return;
                var row = dt.Rows[0];

                lbl_CodigoOrdenSolicitud_Recepcion.Text = Convert.ToString(row["CodigoOrden"]);

                _clienteIdSeleccionado = Convert.ToInt32(row["ClienteID"]);
                lbl_NombreCliente_Equipos.Text = Convert.ToString(row["ClienteNombre"]) ?? "";

                await CargarEquiposClienteAsync(_clienteIdSeleccionado.Value);

                if (row["EquipoID"] != DBNull.Value)
                    cmbox_EquipoCliente_Recepcion.SelectedValue = Convert.ToInt32(row["EquipoID"]);
                else
                    cmbox_EquipoCliente_Recepcion.SelectedValue = -1;

                txt_Detalles_Recepcion.Text = Convert.ToString(row["ProblemaReportado"]);
                txt_AccesoriosRecibidos_Recepcion.Text = Convert.ToString(row["AccesoriosRecibidos"]);

                if (row["TecnicoID"] != DBNull.Value)
                    cmbox_Tecnico_Recepcion.SelectedValue = Convert.ToInt32(row["TecnicoID"]);

                btn_CrearOrden_Recepcion.Text = "Actualizar orden";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Seleccionar cliente
        // =========================
        private async Task SeleccionarClienteAsync()
        {
            using (var f = new Seleccion_Cliente(_session))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return;

                _clienteIdSeleccionado = f.SelectedClienteID;
                lbl_NombreCliente_Equipos.Text = f.SelectedClienteNombre ?? "Sin selección";

                await CargarEquiposClienteAsync(_clienteIdSeleccionado.Value);

                _ordenSeleccionadaId = 0;
                btn_CrearOrden_Recepcion.Text = "Crear orden";
                await GenerarCodigoAsync();
            }
        }

        // =========================
        // Asignar técnico
        // =========================
        private async Task AsignarTecnicoAsync()
        {
            try
            {
                if (_ordenSeleccionadaId <= 0)
                {
                    MessageBox.Show("Primero seleccione una orden del listado.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int tecnicoId = GetComboInt32Value(cmbox_Tecnico_Recepcion, "TecnicoID", 0);
                if (tecnicoId <= 0)
                {
                    MessageBox.Show("Seleccione un técnico.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await ExecDataTableAsync("ops.usp_OrdenServicio_SetTecnico", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@TecnicoID", tecnicoId);
                });

                MessageBox.Show("Técnico asignado.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await BuscarOrdenesAsync();
                await SeleccionarOrdenAsync(_ordenSeleccionadaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Guardar (crear/actualizar)
        // =========================
        private async Task GuardarAsync()
        {
            try
            {
                if (_clienteIdSeleccionado == null || _clienteIdSeleccionado <= 0)
                {
                    MessageBox.Show("Seleccione un cliente.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int clienteId = _clienteIdSeleccionado.Value;

                int equipoId = GetComboInt32Value(cmbox_EquipoCliente_Recepcion, "EquipoID", -1);
                if (equipoId <= 0)
                {
                    MessageBox.Show("Seleccione un equipo del cliente.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int tecnicoId = GetComboInt32Value(cmbox_Tecnico_Recepcion, "TecnicoID", 0);
                if (tecnicoId <= 0)
                {
                    MessageBox.Show("Seleccione un técnico.", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string detalles = (txt_Detalles_Recepcion.Text ?? "").Trim();
                string accesorios = (txt_AccesoriosRecibidos_Recepcion.Text ?? "").Trim();

                var dt = await ExecDataTableAsync("ops.usp_OrdenServicio_Recepcion_Guardar", cmd =>
                {
                    cmd.Parameters.AddWithValue("@UsuarioID_Actor", _usuarioId);
                    cmd.Parameters.AddWithValue("@OrdenServicioID", _ordenSeleccionadaId <= 0 ? (object)DBNull.Value : _ordenSeleccionadaId);
                    cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                    cmd.Parameters.AddWithValue("@EquipoID", equipoId);
                    cmd.Parameters.AddWithValue("@TecnicoID", tecnicoId);
                    cmd.Parameters.AddWithValue("@ProblemaReportado", string.IsNullOrWhiteSpace(detalles) ? (object)DBNull.Value : detalles);
                    cmd.Parameters.AddWithValue("@AccesoriosRecibidos", string.IsNullOrWhiteSpace(accesorios) ? (object)DBNull.Value : accesorios);
                });

                int ordenId = Convert.ToInt32(dt.Rows[0]["OrdenServicioID"]);
                string codigo = Convert.ToString(dt.Rows[0]["CodigoOrden"]);

                _ordenSeleccionadaId = ordenId;
                lbl_CodigoOrdenSolicitud_Recepcion.Text = codigo ?? lbl_CodigoOrdenSolicitud_Recepcion.Text;

                MessageBox.Show(
                    (btn_CrearOrden_Recepcion.Text.Contains("Actualizar") ? "Orden actualizada." : "Orden creada."),
                    "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);

                await BuscarOrdenesAsync();
                await SeleccionarOrdenAsync(_ordenSeleccionadaId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV - Recepción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================
        // Helpers Combo (evita DataRowView->IConvertible)
        // =========================
        private static short GetComboInt16Value(ComboBox cb, string valueCol, short defaultVal)
        {
            if (cb == null) return defaultVal;

            object v = cb.SelectedValue;

            if (v is DataRowView drv && drv.DataView.Table.Columns.Contains(valueCol))
                v = drv[valueCol];

            if (v == null || v == DBNull.Value) return defaultVal;
            if (v is IConvertible) return Convert.ToInt16(v);

            return defaultVal;
        }

        private static int GetComboInt32Value(ComboBox cb, string valueCol, int defaultVal)
        {
            if (cb == null) return defaultVal;

            object v = cb.SelectedValue;

            if (v is DataRowView drv && drv.DataView.Table.Columns.Contains(valueCol))
                v = drv[valueCol];

            if (v == null || v == DBNull.Value) return defaultVal;
            if (v is IConvertible) return Convert.ToInt32(v);

            return defaultVal;
        }

        // =========================
        // DB Helper (SOLO SP)
        // =========================
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
                    dt.Load(rd);
            }

            return dt;
        }

        // =========================
        // Sesión: lectura robusta (mínimo)
        // =========================
        private static int? TryGetUsuarioSesionId(object session)
        {
            if (session == null) return null;

            var candidates = new[]
            {
                "UsuarioID", "UsuarioId", "UserId", "IdUsuario", "UsuarioID_Usuarios", "UsuarioSesionID", "UsuarioID_Sesion"
            };

            var t = session.GetType();
            foreach (var p in candidates)
            {
                var pi = t.GetProperty(p);
                if (pi == null) continue;
                var val = pi.GetValue(session);
                if (val is int i && i > 0) return i;
                if (val is IConvertible conv)
                {
                    try
                    {
                        var x = Convert.ToInt32(conv);
                        if (x > 0) return x;
                    }
                    catch { }
                }
            }
            return null;
        }
    }
}
