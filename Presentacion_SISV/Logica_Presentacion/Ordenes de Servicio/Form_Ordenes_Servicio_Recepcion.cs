using Capa_Corte_Transversal.Helpers;
using Dominio_SISV.Permisos;
using System;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Presentacion_SISV.Controls.Ordenes_de_Servicio.Equipos;
using Union_Formularios_SISV.Controls.Ordenes_de_Servicio.Recepcion;
using Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Recepcion;

namespace Union_Formularios_SISV.Forms.Ordenes_de_Servicio
{
    public partial class Form_Ordenes_Servicio_Recepcion : Form, IOrdenesRecepcionView
    {
        private readonly object _session;
        private readonly Timer _debounceBuscar = new Timer { Interval = 300 };
        private readonly OrdenesRecepcionPresenter _presenter;
        private bool _uiWired;

        public Form_Ordenes_Servicio_Recepcion() : this(null) { }

        public Form_Ordenes_Servicio_Recepcion(object session)
        {
            InitializeComponent();
            _session = session;
            _presenter = new OrdenesRecepcionPresenter(this);

            Shown += async (s, e) =>
            {
                if (_uiWired) return;
                WireUi();
                await _presenter.InitializeAsync();
            };
        }

        private void WireUi()
        {
            _uiWired = true;

            MarcarBotonActivoRecepcion();

            if (btn_Equipos_Recepcion != null)
                btn_Equipos_Recepcion.Click += (s, e) =>
                    OpenInMainHost("Union_Formularios_SISV.Forms.Form_Ordenes_Servicio", "Equipos", "Gestión de equipos / órdenes");

            if (btn_Taller_Recepcion != null)
                btn_Taller_Recepcion.Click += (s, e) =>
                    OpenInMainHost("Union_Formularios_SISV.Forms.Ordenes_de_Servicio.Form_Ordenes_Servicio_Notificacion", "Notificación", "Actualizar estado / notificaciones");

            _debounceBuscar.Tick += async (s, e) =>
            {
                _debounceBuscar.Stop();
                await _presenter.BuscarAsync();
            };

            txt_Buscador_Recepcion.TextChanged += (s, e) =>
            {
                _debounceBuscar.Stop();
                _debounceBuscar.Start();
            };

            Cmbox_EstadoFiltro_Recepcion.SelectedIndexChanged += async (s, e) => await _presenter.BuscarAsync();

            btn_Seleccionar_Cliente.Click += async (s, e) => await _presenter.SelectClientAsync();
            btn_Nuevo_Recepcion.Click += async (s, e) => await _presenter.NuevoAsync();
            btn_AsignarTecnico_Recepcion.Click += async (s, e) => await _presenter.AssignTechnicianAsync();
            btn_CrearOrden_Recepcion.Click += async (s, e) => await _presenter.SaveAsync();

            flowRecepcion.SizeChanged += (s, e) => AjustarAnchoCards();
        }

        // ========= IOrdenesRecepcionView =========

        public int UsuarioId => SafeUsuarioId(_session);

        public string BuscarTexto => (txt_Buscador_Recepcion.Text ?? "").Trim();

        public short EstadoFiltroValor => GetComboInt16Value(Cmbox_EstadoFiltro_Recepcion, -1);

        public int TecnicoSeleccionadoId => GetComboInt32Value(cmbox_Tecnico_Recepcion, 0);

        public int EquipoSeleccionadoId => GetComboInt32Value(cmbox_EquipoCliente_Recepcion, -1);

        public string ProblemaReportado
        {
            get => txt_Detalles_Recepcion.Text ?? "";
            set => txt_Detalles_Recepcion.Text = value ?? "";
        }

        public string AccesoriosRecibidos
        {
            get => txt_AccesoriosRecibidos_Recepcion.Text ?? "";
            set => txt_AccesoriosRecibidos_Recepcion.Text = value ?? "";
        }

        public void BindEstadosFiltro(DataTable dt)
        {
            BindComboSafe(Cmbox_EstadoFiltro_Recepcion, dt, "EstadoNombre", "EstadoValor");
        }

        public void BindTecnicos(DataTable dt)
        {
            BindComboSafe(cmbox_Tecnico_Recepcion, dt, "TecnicoNombre", "TecnicoID");
        }

        public void BindEquiposCliente(DataTable dt)
        {
            if (dt == null) dt = new DataTable();

            if (dt.Columns.Contains("EquipoID") && dt.Columns.Contains("EquipoTexto"))
            {
                var row = dt.NewRow();
                row["EquipoID"] = -1;
                row["EquipoTexto"] = "Seleccione...";
                dt.Rows.InsertAt(row, 0);
            }

            cmbox_EquipoCliente_Recepcion.DataSource = dt;
            cmbox_EquipoCliente_Recepcion.DisplayMember = dt.Columns.Contains("EquipoTexto") ? "EquipoTexto" : "";
            cmbox_EquipoCliente_Recepcion.ValueMember = dt.Columns.Contains("EquipoID") ? "EquipoID" : "";
            cmbox_EquipoCliente_Recepcion.SelectedValue = -1;
        }

        public void RenderOrdenes(DataTable dt, int selectedOrderId)
        {
            if (dt == null) dt = new DataTable();

            flowRecepcion.SuspendLayout();
            flowRecepcion.Controls.Clear();

            foreach (DataRow r in dt.Rows)
            {
                int ordenId = I(r, 0, "OrdenServicioID");
                if (ordenId <= 0) continue;

                string codigo = S(r, "CodigoOrden");
                string cliente = S(r, "ClienteNombre", "Cliente");
                string equipo = S(r, "TipoEquipoNombre", "TipoEquipo", "Nombre_TipoEquipo");
                if (string.IsNullOrWhiteSpace(equipo))
                    equipo = S(r, "EquipoNombre", "Equipo"); string tecnico = S(r, "TecnicoNombre", "Tecnico");
                string estado = S(r, "EstadoNombre", "Estado");

                var card = new RecepcionTaskCard
                {
                    Width = Math.Max(200, flowRecepcion.ClientSize.Width - 22)
                };

                card.Bind(ordenId, codigo, cliente, equipo, tecnico, estado);
                card.SetSelected(ordenId == selectedOrderId);
                card.OrdenSeleccionada += async (s, args) => await _presenter.SelectOrderAsync(args.OrdenServicioID);

                flowRecepcion.Controls.Add(card);
            }

            flowRecepcion.ResumeLayout();
        }

        public void SetResultados(int total)
        {
            lbl_Resultados_Recepcion.Text = $"{total} resultados";
        }

        public void SetCodigoOrden(string codigo)
        {
            lbl_CodigoOrdenSolicitud_Recepcion.Text = string.IsNullOrWhiteSpace(codigo) ? "OS-????" : codigo;
        }

        public void SetClienteSeleccionado(int? clienteId, string clienteNombre)
        {
            lbl_NombreCliente_Equipos.Text = string.IsNullOrWhiteSpace(clienteNombre) ? "Sin selección" : clienteNombre;
        }

        public void ClearClienteSeleccionado()
        {
            lbl_NombreCliente_Equipos.Text = "Sin selección";
        }

        public void ClearEquiposCliente()
        {
            cmbox_EquipoCliente_Recepcion.DataSource = null;
        }

        public void SetEquipoSeleccionado(int equipoId)
        {
            try
            {
                cmbox_EquipoCliente_Recepcion.SelectedValue = equipoId > 0 ? (object)equipoId : -1;
            }
            catch
            {
                try { cmbox_EquipoCliente_Recepcion.SelectedIndex = 0; } catch { }
            }
        }

        public void SetTecnicoSeleccionado(int tecnicoId)
        {
            try
            {
                if (tecnicoId > 0) cmbox_Tecnico_Recepcion.SelectedValue = tecnicoId;
                else if (cmbox_Tecnico_Recepcion.Items.Count > 0) cmbox_Tecnico_Recepcion.SelectedIndex = 0;
            }
            catch
            {
                try { cmbox_Tecnico_Recepcion.SelectedIndex = 0; } catch { }
            }
        }

        public void SetModoActualizar(bool actualizar)
        {
            btn_CrearOrden_Recepcion.Text = actualizar ? "Actualizar orden" : "Crear orden";
        }

        public void SetPermisosAcciones(bool puedeGuardar, bool puedeAsignarTecnico)
        {
            btn_CrearOrden_Recepcion.Enabled = puedeGuardar;
            btn_AsignarTecnico_Recepcion.Enabled = puedeAsignarTecnico;
            cmbox_Tecnico_Recepcion.Enabled = puedeAsignarTecnico;
        }

        public void SetVisibilidadNavegacion(bool verEquipos, bool verNotificacion)
        {
            if (btn_Equipos_Recepcion != null)
                btn_Equipos_Recepcion.Visible = verEquipos;

            if (btn_Taller_Recepcion != null)
                btn_Taller_Recepcion.Visible = verNotificacion;
        }

        public bool TrySeleccionarCliente(out int? clienteId, out string clienteNombre)
        {
            clienteId = null;
            clienteNombre = null;

            using (var f = new Seleccion_Cliente(_session))
            {
                if (f.ShowDialog(this) != DialogResult.OK) return false;

                clienteId = f.SelectedClienteID;
                clienteNombre = f.SelectedClienteNombre;
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

        // ========= UI / Nav =========

        private void AjustarAnchoCards()
        {
            foreach (Control c in flowRecepcion.Controls)
                if (c is RecepcionTaskCard card)
                    card.Width = Math.Max(200, flowRecepcion.ClientSize.Width - 22);
        }

        private void MarcarBotonActivoRecepcion()
        {
            SetNavStyle(btn_Equipos_Recepcion, false);
            SetNavStyle(btn_Taller_Recepcion, false);

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

            try { c.Font = new Font(c.Font, active ? FontStyle.Bold : FontStyle.Regular); } catch { }

            if (c is Button b)
            {
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = active ? 2 : 1;
                b.BackColor = active ? Color.White : Color.FromArgb(245, 245, 245);
                return;
            }

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
            if (main != null)
            {
                main.OpenChild(child, titulo, descripcion);
                return;
            }

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

        // ========= Helpers =========

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

        private static short GetComboInt16Value(ComboBox cb, short def)
        {
            if (cb == null) return def;
            object v = cb.SelectedValue;
            if (v == null || v == DBNull.Value) return def;
            if (v is IConvertible) return Convert.ToInt16(v);
            return def;
        }

        private static int GetComboInt32Value(ComboBox cb, int def)
        {
            if (cb == null) return def;
            object v = cb.SelectedValue;
            if (v == null || v == DBNull.Value) return def;
            if (v is IConvertible) return Convert.ToInt32(v);
            return def;
        }

        private static int SafeUsuarioId(object session)
        {
            try { return session == null ? 0 : SessionHelper.GetUsuarioID(session); }
            catch { return 0; }
        }

        private static string S(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToString(row[c]);

            return "";
        }

        private static int I(DataRow row, int def, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToInt32(row[c]);

            return def;
        }
        public void SetTecnicoHabilitado(bool enabled)
        {
            cmbox_Tecnico_Recepcion.Enabled = enabled;
        }

        public void BindTecnicosNoDisponible()
        {
            var dt = new DataTable();
            dt.Columns.Add("TecnicoID", typeof(int));
            dt.Columns.Add("TecnicoNombre", typeof(string));

            dt.Rows.Add(0, "N/A");

            cmbox_Tecnico_Recepcion.DataSource = dt;
            cmbox_Tecnico_Recepcion.DisplayMember = "TecnicoNombre";
            cmbox_Tecnico_Recepcion.ValueMember = "TecnicoID";
            cmbox_Tecnico_Recepcion.SelectedValue = 0;
        }
    }
}