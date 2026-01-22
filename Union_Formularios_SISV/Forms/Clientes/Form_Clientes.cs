using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms.Clientes;

namespace Union_Formularios_SISV.Forms
{
    public partial class Form_Clientes : Form
    {
        private readonly ClienteDb _db;
        private readonly Timer _searchDebounce;

        private string _selectedCedula = null;

        private List<ClientTaskCard> _renderedCards = new List<ClientTaskCard>();

        public Form_Clientes()
        {
            InitializeComponent();

            _db = new ClienteDb(GetConnectionString());

            _searchDebounce = new Timer { Interval = 350 };
            _searchDebounce.Tick += (_, __) =>
            {
                _searchDebounce.Stop();
                LoadClientes();
            };

            // Eventos UI
            this.Load += Form_Clientes_Load;

            txt_Buscador_Items_Clientes.TextChanged += (_, __) => DebounceSearch();
            cmbox_Filtrarpor_Clientes.SelectedIndexChanged += (_, __) => LoadClientes();
            cmbox_EstadoFiltro_Clientes.SelectedIndexChanged += (_, __) => LoadClientes();

            btn_Registrar_Clientes.Click += Btn_Registrar_Clientes_Click;
            btn_Actualizar_Clientes.Click += Btn_Actualizar_Clientes_Click;
            btn_Limpiar_Clientes.Click += (_, __) => ClearSelectionAndForm();

            txt_Buscador_Items_Clientes.KeyDown += Txt_Buscador_Items_Clientes_KeyDown;




        }
        private void SetInputsEnabled(bool enabled)
        {
            // Fuerza habilitados (evita que queden gris/disabled)
            txt_Cedula_Cliente.Enabled = enabled;
            txt_Telefono_Clientes.Enabled = enabled;
            txt_Nombre_Clientes.Enabled = enabled;
            txt_Apellido_Clientes.Enabled = enabled;
            txt_Correo_Clientes.Enabled = enabled;
            txt_Direccion_Clientes.Enabled = enabled;

            cmbox_Estado_Clientes.Enabled = enabled;
        }

        private void SetCedulaReadOnly(bool readOnly)
        {
            // En vez de Enabled=false (que se ve gris), usamos ReadOnly
            // Sirve para TextBox normal y también para Guna2TextBox.
            txt_Cedula_Cliente.ReadOnly = readOnly;

            // Si tu control NO tiene ReadOnly, comenta la línea y deja Enabled=true.
        }



        private static string GetConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings["SISV"]?.ConnectionString;
            if (!string.IsNullOrWhiteSpace(cs)) return cs;

            if (ConfigurationManager.ConnectionStrings.Count > 0)
                return ConfigurationManager.ConnectionStrings[0].ConnectionString;

            throw new InvalidOperationException("No se encontró ConnectionString. Agrega uno en App.config (name=\"SISV\").");
        }

        private void Form_Clientes_Load(object sender, EventArgs e)
        {
            SetInputsEnabled(true);
            SetCedulaReadOnly(false);


            try
            {
                ConfigureCombos();
                LoadEstados();
                ClearSelectionAndForm();
                LoadClientes();
            }
            catch (Exception ex)
            {
                ShowError("Error al inicializar formulario de clientes.", ex);
            }
        }

        // =========================
        // Combos
        // =========================
        private void ConfigureCombos()
        {
            // Filtrar por
            var filtros = new List<FiltroItem>
            {
                new FiltroItem("nombre",    "Nombre (nombres+apellidos)"),
                new FiltroItem("cedula",    "Cédula"),
                new FiltroItem("email",     "Email"),
                new FiltroItem("telefono",  "Teléfono"),
                new FiltroItem("direccion", "Dirección"),
                new FiltroItem("apellidos", "Apellidos"),
            };

            cmbox_Filtrarpor_Clientes.DisplayMember = nameof(FiltroItem.Texto);
            cmbox_Filtrarpor_Clientes.ValueMember = nameof(FiltroItem.Key);
            cmbox_Filtrarpor_Clientes.DataSource = filtros;
            cmbox_Filtrarpor_Clientes.SelectedValue = "nombre";
        }

        private void LoadEstados()
        {
            var estados = _db.ListarEstados(); // EstadoKey, EstadoNombre, EsActivo

            // Para el combo del formulario (estado del cliente)
            cmbox_Estado_Clientes.DisplayMember = nameof(EstadoItem.EstadoNombre);
            cmbox_Estado_Clientes.ValueMember = nameof(EstadoItem.EstadoKey);
            cmbox_Estado_Clientes.DataSource = estados.ToList();

            // Para el filtro (incluye "Todos")
            var filtroEstados = new List<EstadoItem> { new EstadoItem(null, "Todos", null) };
            filtroEstados.AddRange(estados);

            cmbox_EstadoFiltro_Clientes.DisplayMember = nameof(EstadoItem.EstadoNombre);
            cmbox_EstadoFiltro_Clientes.ValueMember = nameof(EstadoItem.EstadoKey);
            cmbox_EstadoFiltro_Clientes.DataSource = filtroEstados;
            cmbox_EstadoFiltro_Clientes.SelectedIndex = 0;

            // Default en formulario: Activo si existe
            var activo = estados.FirstOrDefault(x => x.EstadoNombre?.ToLower().Contains("activo") == true && x.EstadoNombre?.ToLower().Contains("inactivo") == false);
            if (activo != null) cmbox_Estado_Clientes.SelectedValue = activo.EstadoKey;
        }

        // =========================
        // Buscar / Render cards
        // =========================
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

            // Búsqueda inmediata al Enter
            _searchDebounce.Stop();
            LoadClientes();

            // Si filtro=cedula y no hay resultados => mensaje "no existe"
            var filtroKey = GetFiltroKey();
            if (filtroKey == "cedula")
            {
                var count = ParseCount(lbl_Cantidad_Resultados_Clientes.Text);
                if (count == 0)
                    ShowWarn("El usuario no existe.");
            }
        }

        private void LoadClientes()
        {
            try
            {
                var filtro = GetFiltroKey();
                var buscar = (txt_Buscador_Items_Clientes.Text ?? "").Trim();
                int? estadoKey = GetEstadoFiltroKey();

                var list = _db.Buscar(filtro, buscar, estadoKey, top: 200);

                lbl_Cantidad_Resultados_Clientes.Text = $"{list.Count} resultados";

                RenderCards(list);

                // Si el seleccionado ya no está, limpiar selección
                if (!string.IsNullOrWhiteSpace(_selectedCedula))
                {
                    var stillExists = list.Any(x => string.Equals(x.Cedula, _selectedCedula, StringComparison.OrdinalIgnoreCase));
                    if (!stillExists) ClearSelectionOnly();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error al consultar clientes.", ex);
            }
        }

        private void RenderCards(List<ClienteCardVM> list)
        {
            flowClientCard.SuspendLayout();
            flowClientCard.Controls.Clear();
            _renderedCards.Clear();

            foreach (var vm in list)
            {
                var card = new ClientTaskCard
                {
                    Margin = new Padding(6),
                    Width = flowClientCard.ClientSize.Width - 28, // ajusta si quieres
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };

                card.Bind(vm);
                card.SetSelected(!string.IsNullOrWhiteSpace(_selectedCedula) && _selectedCedula == vm.Cedula);
                card.ClientSelected += Card_ClientSelected;

                _renderedCards.Add(card);
                flowClientCard.Controls.Add(card);
            }

            flowClientCard.ResumeLayout();
        }

        private void Card_ClientSelected(object sender, ClienteCardSelectedEventArgs e)
        {
            if (e?.Cliente == null) return;

            _selectedCedula = e.Cliente.Cedula;

            // Marcar visualmente
            foreach (var c in _renderedCards)
                c.SetSelected(c.Cedula == _selectedCedula);

            // Cargar detalle y llenar form
            LoadClienteDetalle(_selectedCedula);
        }

        private void LoadClienteDetalle(string cedula)
        {
            try
            {
                var det = _db.GetByCedula(cedula);
                if (det == null)
                {
                    ShowWarn("El usuario no existe.");
                    ClearSelectionAndForm();
                    return;
                }

                SetInputsEnabled(true);

                lbl_Seleccion_Clientes.Text = $"Seleccionado: {det.Cedula}";
                txt_Cedula_Cliente.Text = det.Cedula ?? "";
                txt_Nombre_Clientes.Text = det.Nombres ?? "";
                txt_Apellido_Clientes.Text = det.Apellidos ?? "";
                txt_Correo_Clientes.Text = det.Correo ?? "";
                txt_Telefono_Clientes.Text = det.Telefono ?? "";
                txt_Direccion_Clientes.Text = det.Direccion ?? "";

                if (det.EstadoKey != null)
                    cmbox_Estado_Clientes.SelectedValue = det.EstadoKey;
                else
                {
                    if (det.EsActivo == true) TrySelectEstadoPorTexto("activo");
                    else if (det.EsActivo == false) TrySelectEstadoPorTexto("inactivo");
                }

                SetCedulaReadOnly(true);

                btn_Actualizar_Clientes.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowError("Error al cargar detalle del cliente.", ex);
            }
        }

        private void Btn_Registrar_Clientes_Click(object sender, EventArgs e)
        {
            try
            {
                var cedula = (txt_Cedula_Cliente.Text ?? "").Trim();
                var nombres = (txt_Nombre_Clientes.Text ?? "").Trim();
                var apellidos = (txt_Apellido_Clientes.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(cedula))
                {
                    ShowWarn("Error: Ingrese la cédula.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
                {
                    ShowWarn("Error: Campos incompletos (nombres/apellidos).");
                    return;
                }

                int? estadoKey = GetEstadoFormKey();

                var det = _db.Crear(
                    cedula: cedula,
                    nombres: nombres,
                    apellidos: apellidos,
                    correo: (txt_Correo_Clientes.Text ?? "").Trim(),
                    telefono: (txt_Telefono_Clientes.Text ?? "").Trim(),
                    direccion: (txt_Direccion_Clientes.Text ?? "").Trim(),
                    estadoKey: estadoKey
                );

                ShowOk("Guardado correctamente.");

                // refrescar y seleccionar
                _selectedCedula = det?.Cedula ?? cedula;
                txt_Buscador_Items_Clientes.Text = _selectedCedula; // opcional
                cmbox_Filtrarpor_Clientes.SelectedValue = "cedula";
                LoadClientes();
                LoadClienteDetalle(_selectedCedula);
            }
            catch (SqlException ex)
            {
                // Mensajes de tus THROW: "La cédula ya existe.", etc.
                ShowWarn(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Error al registrar cliente.", ex);
            }
        }

        private void Btn_Actualizar_Clientes_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedCedula))
                {
                    ShowWarn("Seleccione un cliente para actualizar.");
                    return;
                }

                var nombres = (txt_Nombre_Clientes.Text ?? "").Trim();
                var apellidos = (txt_Apellido_Clientes.Text ?? "").Trim();

                if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
                {
                    ShowWarn("Campos incompletos.");
                    return;
                }

                int? estadoKey = GetEstadoFormKey();

                var det = _db.Actualizar(
                    cedula: _selectedCedula,
                    nombres: nombres,
                    apellidos: apellidos,
                    correo: (txt_Correo_Clientes.Text ?? "").Trim(),
                    telefono: (txt_Telefono_Clientes.Text ?? "").Trim(),
                    direccion: (txt_Direccion_Clientes.Text ?? "").Trim(),
                    estadoKey: estadoKey
                );

                ShowOk("Actualizado correctamente.");

                LoadClientes();
                LoadClienteDetalle(_selectedCedula);
            }
            catch (SqlException ex)
            {
                ShowWarn(ex.Message);
            }
            catch (Exception ex)
            {
                ShowError("Error al actualizar cliente.", ex);
            }
        }

        private void ClearSelectionOnly()
        {
            _selectedCedula = null;
            lbl_Seleccion_Clientes.Text = "Sin seleccionar";
            btn_Actualizar_Clientes.Enabled = false;

            foreach (var c in _renderedCards)
                c.SetSelected(false);
        }

        private void ClearSelectionAndForm()
        {
            ClearSelectionOnly();

            SetInputsEnabled(true);
            SetCedulaReadOnly(false);

            txt_Cedula_Cliente.Text = "";
            txt_Telefono_Clientes.Text = "";
            txt_Nombre_Clientes.Text = "";
            txt_Apellido_Clientes.Text = "";
            txt_Correo_Clientes.Text = "";
            txt_Direccion_Clientes.Text = "";

            TrySelectEstadoPorTexto("activo");
        }

        // =========================
        // Helpers UI
        // =========================
        private string GetFiltroKey()
        {
            if (cmbox_Filtrarpor_Clientes.SelectedValue is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            if (cmbox_Filtrarpor_Clientes.SelectedItem is FiltroItem fi)
                return fi.Key;

            return "nombre";
        }

        private int? GetEstadoFiltroKey()
        {
            if (cmbox_EstadoFiltro_Clientes.SelectedValue == null) return null;

            if (cmbox_EstadoFiltro_Clientes.SelectedValue is int i) return i;

            // si viene como string
            if (int.TryParse(cmbox_EstadoFiltro_Clientes.SelectedValue.ToString(), out var val))
                return val;

            return null;
        }

        private int? GetEstadoFormKey()
        {
            if (cmbox_Estado_Clientes.SelectedValue == null) return null;
            if (cmbox_Estado_Clientes.SelectedValue is int i) return i;
            if (int.TryParse(cmbox_Estado_Clientes.SelectedValue.ToString(), out var val)) return val;
            return null;
        }

        private void TrySelectEstadoPorTexto(string containsLower)
        {
            // intenta seleccionar "Activo" / "Inactivo" en cmbox_Estado_Clientes
            if (cmbox_Estado_Clientes.DataSource is IEnumerable<EstadoItem> src)
            {
                var match = src.FirstOrDefault(x => (x.EstadoNombre ?? "").ToLower().Contains(containsLower));
                if (match != null) cmbox_Estado_Clientes.SelectedValue = match.EstadoKey;
            }
        }

        private void ShowOk(string msg)
            => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Information);

        private void ShowWarn(string msg)
            => MessageBox.Show(msg, "SISV", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void ShowError(string title, Exception ex)
            => MessageBox.Show($"{title}\n\n{ex.Message}", "SISV", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private static int ParseCount(string text)
        {
            // espera formato: "X resultados"
            if (string.IsNullOrWhiteSpace(text)) return 0;
            var parts = text.Split(' ');
            if (parts.Length == 0) return 0;
            return int.TryParse(parts[0], out var n) ? n : 0;
        }

        // =========================
        // Clases auxiliares
        // =========================
        private sealed class FiltroItem
        {
            public FiltroItem(string key, string texto) { Key = key; Texto = texto; }
            public string Key { get; }
            public string Texto { get; }
        }

        private sealed class EstadoItem
        {
            public EstadoItem(int? estadoKey, string estadoNombre, bool? esActivo)
            { EstadoKey = estadoKey; EstadoNombre = estadoNombre; EsActivo = esActivo; }

            public int? EstadoKey { get; }
            public string EstadoNombre { get; }
            public bool? EsActivo { get; }
        }

        // =========================
        // Data Access (solo Stored Procedures)
        // =========================
        private sealed class ClienteDb
        {
            private readonly string _cs;
            public ClienteDb(string connectionString) => _cs = connectionString;

            public List<EstadoItem> ListarEstados()
            {
                var list = new List<EstadoItem>();

                using (var cn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("crm.usp_Cliente_Estados_Listar", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cn.Open();

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            int? key = rd["EstadoKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["EstadoKey"]);
                            var nombre = rd["EstadoNombre"] == DBNull.Value ? null : Convert.ToString(rd["EstadoNombre"]);
                            bool? activo = rd["EsActivo"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(rd["EsActivo"]);

                            list.Add(new EstadoItem(key, nombre, activo));
                        }
                    }
                }

                return list;
            }

            public List<ClienteCardVM> Buscar(string filtroPor, string buscar, int? estadoKey, int top)
            {
                var list = new List<ClienteCardVM>();

                using (var cn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("crm.usp_Cliente_Buscar", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@FiltroPor", SqlDbType.NVarChar, 30) { Value = (object)(filtroPor ?? "nombre") });
                    cmd.Parameters.Add(new SqlParameter("@Buscar", SqlDbType.NVarChar, 200) { Value = (object)(string.IsNullOrWhiteSpace(buscar) ? DBNull.Value : (object)buscar) });
                    cmd.Parameters.Add(new SqlParameter("@EstadoKey", SqlDbType.Int) { Value = (object)(estadoKey ?? (int?)null) ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@Top", SqlDbType.Int) { Value = top });

                    cn.Open();

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            var vm = new ClienteCardVM
                            {
                                Cedula = rd["Cedula"] == DBNull.Value ? "" : Convert.ToString(rd["Cedula"]),
                                Cliente = rd["Cliente"] == DBNull.Value ? "" : Convert.ToString(rd["Cliente"]),
                                Correo = rd["Correo"] == DBNull.Value ? "" : Convert.ToString(rd["Correo"]),
                                Telefono = rd["Telefono"] == DBNull.Value ? "" : Convert.ToString(rd["Telefono"]),
                                EstadoKey = rd["EstadoKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["EstadoKey"]),
                                EstadoNombre = rd["EstadoNombre"] == DBNull.Value ? "" : Convert.ToString(rd["EstadoNombre"]),
                                EsActivo = rd["EsActivo"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(rd["EsActivo"]),
                                TotalCoincidencias = rd["TotalCoincidencias"] == DBNull.Value ? 0 : Convert.ToInt32(rd["TotalCoincidencias"])
                            };
                            list.Add(vm);
                        }
                    }
                }

                return list;
            }

            public ClienteDetalleVM GetByCedula(string cedula)
            {
                using (var cn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("crm.usp_Cliente_GetByCedula", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.NVarChar, 30) { Value = cedula });

                    cn.Open();

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new ClienteDetalleVM
                        {
                            Cedula = rd["Cedula"] == DBNull.Value ? "" : Convert.ToString(rd["Cedula"]),
                            Nombres = rd["Nombres"] == DBNull.Value ? "" : Convert.ToString(rd["Nombres"]),
                            Apellidos = rd["Apellidos"] == DBNull.Value ? "" : Convert.ToString(rd["Apellidos"]),
                            Cliente = rd["Cliente"] == DBNull.Value ? "" : Convert.ToString(rd["Cliente"]),
                            Correo = rd["Correo"] == DBNull.Value ? "" : Convert.ToString(rd["Correo"]),
                            Telefono = rd["Telefono"] == DBNull.Value ? "" : Convert.ToString(rd["Telefono"]),
                            Direccion = rd["Direccion"] == DBNull.Value ? "" : Convert.ToString(rd["Direccion"]),
                            EstadoKey = rd["EstadoKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["EstadoKey"]),
                            EstadoNombre = rd["EstadoNombre"] == DBNull.Value ? "" : Convert.ToString(rd["EstadoNombre"]),
                            EsActivo = rd["EsActivo"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(rd["EsActivo"]),
                        };
                    }
                }
            }

            public ClienteDetalleVM Crear(string cedula, string nombres, string apellidos, string correo, string telefono, string direccion, int? estadoKey)
            {
                using (var cn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("crm.usp_Cliente_Crear", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.NVarChar, 30) { Value = cedula });
                    cmd.Parameters.Add(new SqlParameter("@Nombres", SqlDbType.NVarChar, 120) { Value = nombres });
                    cmd.Parameters.Add(new SqlParameter("@Apellidos", SqlDbType.NVarChar, 120) { Value = apellidos });
                    cmd.Parameters.Add(new SqlParameter("@Correo", SqlDbType.NVarChar, 220) { Value = (object)(string.IsNullOrWhiteSpace(correo) ? DBNull.Value : (object)correo) });
                    cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 30) { Value = (object)(string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : (object)telefono) });
                    cmd.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.NVarChar, 220) { Value = (object)(string.IsNullOrWhiteSpace(direccion) ? DBNull.Value : (object)direccion) });
                    cmd.Parameters.Add(new SqlParameter("@EstadoKey", SqlDbType.Int) { Value = (object)(estadoKey ?? (int?)null) ?? DBNull.Value });

                    var pOut = new SqlParameter("@ClienteIDOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    cmd.Parameters.Add(pOut);

                    cn.Open();

                    // este SP retorna el detalle (usp_Cliente_GetByCedula)
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new ClienteDetalleVM
                        {
                            Cedula = rd["Cedula"] == DBNull.Value ? "" : Convert.ToString(rd["Cedula"]),
                            Nombres = rd["Nombres"] == DBNull.Value ? "" : Convert.ToString(rd["Nombres"]),
                            Apellidos = rd["Apellidos"] == DBNull.Value ? "" : Convert.ToString(rd["Apellidos"]),
                            Cliente = rd["Cliente"] == DBNull.Value ? "" : Convert.ToString(rd["Cliente"]),
                            Correo = rd["Correo"] == DBNull.Value ? "" : Convert.ToString(rd["Correo"]),
                            Telefono = rd["Telefono"] == DBNull.Value ? "" : Convert.ToString(rd["Telefono"]),
                            Direccion = rd["Direccion"] == DBNull.Value ? "" : Convert.ToString(rd["Direccion"]),
                            EstadoKey = rd["EstadoKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["EstadoKey"]),
                            EstadoNombre = rd["EstadoNombre"] == DBNull.Value ? "" : Convert.ToString(rd["EstadoNombre"]),
                            EsActivo = rd["EsActivo"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(rd["EsActivo"]),
                        };
                    }
                }
            }


            public ClienteDetalleVM Actualizar(string cedula, string nombres, string apellidos, string correo, string telefono, string direccion, int? estadoKey)
            {
                using (var cn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("crm.usp_Cliente_Actualizar", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@Cedula", SqlDbType.NVarChar, 30) { Value = cedula });
                    cmd.Parameters.Add(new SqlParameter("@Nombres", SqlDbType.NVarChar, 120) { Value = (object)(string.IsNullOrWhiteSpace(nombres) ? DBNull.Value : (object)nombres) });
                    cmd.Parameters.Add(new SqlParameter("@Apellidos", SqlDbType.NVarChar, 120) { Value = (object)(string.IsNullOrWhiteSpace(apellidos) ? DBNull.Value : (object)apellidos) });
                    cmd.Parameters.Add(new SqlParameter("@Correo", SqlDbType.NVarChar, 220) { Value = (object)(string.IsNullOrWhiteSpace(correo) ? DBNull.Value : (object)correo) });
                    cmd.Parameters.Add(new SqlParameter("@Telefono", SqlDbType.NVarChar, 30) { Value = (object)(string.IsNullOrWhiteSpace(telefono) ? DBNull.Value : (object)telefono) });
                    cmd.Parameters.Add(new SqlParameter("@Direccion", SqlDbType.NVarChar, 220) { Value = (object)(string.IsNullOrWhiteSpace(direccion) ? DBNull.Value : (object)direccion) });
                    cmd.Parameters.Add(new SqlParameter("@EstadoKey", SqlDbType.Int) { Value = (object)(estadoKey ?? (int?)null) ?? DBNull.Value });

                    cn.Open();

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;

                        return new ClienteDetalleVM
                        {
                            Cedula = rd["Cedula"] == DBNull.Value ? "" : Convert.ToString(rd["Cedula"]),
                            Nombres = rd["Nombres"] == DBNull.Value ? "" : Convert.ToString(rd["Nombres"]),
                            Apellidos = rd["Apellidos"] == DBNull.Value ? "" : Convert.ToString(rd["Apellidos"]),
                            Cliente = rd["Cliente"] == DBNull.Value ? "" : Convert.ToString(rd["Cliente"]),
                            Correo = rd["Correo"] == DBNull.Value ? "" : Convert.ToString(rd["Correo"]),
                            Telefono = rd["Telefono"] == DBNull.Value ? "" : Convert.ToString(rd["Telefono"]),
                            Direccion = rd["Direccion"] == DBNull.Value ? "" : Convert.ToString(rd["Direccion"]),
                            EstadoKey = rd["EstadoKey"] == DBNull.Value ? (int?)null : Convert.ToInt32(rd["EstadoKey"]),
                            EstadoNombre = rd["EstadoNombre"] == DBNull.Value ? "" : Convert.ToString(rd["EstadoNombre"]),
                            EsActivo = rd["EsActivo"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(rd["EsActivo"]),
                        };
                    }
                }
            }
        }
    }
}
