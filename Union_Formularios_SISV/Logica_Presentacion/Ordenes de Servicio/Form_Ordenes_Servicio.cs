using Capa_Corte_Transversal.Helpers;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

        // cliente seleccionado (para registrar equipo)
        private int? _clienteIdSeleccionado = null;
        private string _clienteNombreSeleccionado = null;

        // Debounce búsqueda
        private readonly Timer _debounceBuscar = new Timer { Interval = 350 };
        private bool _isLoading = false;

        public Form_Ordenes_Servicio() : this(null) { }

        public Form_Ordenes_Servicio(object session)
        {
            InitializeComponent();

            _session = session;
            _usuarioId = TryGetUsuarioSesionId();
            _rolId = TryGetRolSesionId();

            Load += async (s, e) => await Form_LoadAsync();
        }

        private async Task Form_LoadAsync()
        {
            // Roles permitidos (según lo que vienes usando): 1 SuperAdmin, 2 Admin, 4 Técnico
            if (_usuarioId <= 0)
            {
                MessageBox.Show("No se pudo obtener UsuarioID de sesión.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            if (_rolId != 1 && _rolId != 2 && _rolId != 4)
            {
                MessageBox.Show("Acceso denegado. Solo SuperAdministrador, Administrador y Técnico.", "SISV",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            _isLoading = true;

            // Events
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

            // Navegación (si esos forms existen en tu proyecto)
            btn_Recepcion_Equipos.Click += (s, e) => AbrirFormularioSiExiste("Union_Formularios_SISV.Forms.Form_Ordenes_Servicio_Recepcion");
            btn_Notificacion_Equipos.Click += (s, e) => AbrirFormularioSiExiste("Union_Formularios_SISV.Forms.Form_Ordenes_Servicio_Notificacion");

            // Ajuste cards en resize
            Flow_OrdenServicio_Equipos.SizeChanged += (s, e) => AjustarAnchoCards();

            // Cargar combos
            await CargarCombosAsync();

            _isLoading = false;

            await LimpiarEquipoAsync();
            await BuscarEquiposAsync();
        }

        // =========================
        // COMBOS
        // =========================
        private async Task CargarCombosAsync()
        {
            // Filtros de equipos
            var dtFiltros = await TryExecDataTableAsync(
                new[] { "ops.usp_Equipo_Filtros_Listar", "dbo.usp_Equipo_Filtros_Listar" },
                cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
            );

            cmbox_Filtrarpor_Equipos.DisplayMember = "Text";
            cmbox_Filtrarpor_Equipos.ValueMember = "Value";
            cmbox_Filtrarpor_Equipos.DataSource = dtFiltros;

            // Tipos de equipo
            var dtTipo = await TryExecDataTableAsync(
                new[] { "ops.usp_TipoEquipo_Listar", "dbo.usp_TipoEquipo_Listar" },
                cmd => cmd.Parameters.AddWithValue("@UsuarioID", _usuarioId)
            );

            Cmbox_TipoEquipo_Equipos.DisplayMember = "TipoEquipoNombre";
            Cmbox_TipoEquipo_Equipos.ValueMember = "TipoEquipoID";
            Cmbox_TipoEquipo_Equipos.DataSource = dtTipo;

            // Conectividad
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
                        cmd.Parameters.AddWithValue("@SoloActivos", DBNull.Value); // el SP decide por rol
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

                // conectividad (Value = string)
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

                        // al elegir cliente, refresca listado
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

            // Generar código interno sugerido
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

            // No reseteo el cliente global del filtro si tú quieres conservarlo,
            // pero sí vacío los campos de equipo.
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

        private void AbrirFormularioSiExiste(string fullTypeName)
        {
            try
            {
                var t = FindTypeInLoadedAssemblies(fullTypeName);
                if (t == null)
                {
                    MessageBox.Show($"No se encontró el formulario: {fullTypeName}", "SISV",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Form f = null;

                // intenta ctor(session)
                var ctor1 = t.GetConstructor(new[] { typeof(object) });
                if (ctor1 != null) f = (Form)ctor1.Invoke(new object[] { _session });
                else f = (Form)Activator.CreateInstance(t);

                f.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
