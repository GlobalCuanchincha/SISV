using Capa_Corte_Transversal.Loggin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Union_Formularios_SISV.Controls.Servicios;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Servicio
{
    public partial class Form_Servicio : Form, IServicioView
    {
        private readonly ServicioPresenter _presenter;

        public Form_Servicio()
        {
            InitializeComponent();
            _presenter = new ServicioPresenter(this);

            Load += (_, __) =>
            {
                ConfigurarUI();
                _presenter.Initialize();
            };
        }

        public int UsuarioId => Session.UsuarioId;

        public string TextoBusqueda => (txt_Buscador_Servicio?.Text ?? "").Trim();

        public int? CategoriaFiltroId
        {
            get
            {
                int catId = 0;
                if (cmbox_CategoriaFiltro_Servicio != null && cmbox_CategoriaFiltro_Servicio.SelectedValue != null)
                    int.TryParse(cmbox_CategoriaFiltro_Servicio.SelectedValue.ToString(), out catId);

                return catId > 0 ? (int?)catId : null;
            }
        }

        public string EstadoFiltroTexto => cmbox_EstadoFiltro_Servicio?.Text ?? "Todos";

        public string CodigoServicio
        {
            get => txt_Codigo_Servicio?.Text ?? "";
            set { if (txt_Codigo_Servicio != null) txt_Codigo_Servicio.Text = value ?? ""; }
        }

        public string NombreServicio
        {
            get => txt_Nombre_Servicio?.Text ?? "";
            set { if (txt_Nombre_Servicio != null) txt_Nombre_Servicio.Text = value ?? ""; }
        }

        public int CategoriaServicioId
        {
            get
            {
                int catId = 0;
                if (cmbox_Categoria_Servicio != null && cmbox_Categoria_Servicio.SelectedValue != null)
                    int.TryParse(cmbox_Categoria_Servicio.SelectedValue.ToString(), out catId);

                return catId;
            }
        }

        public decimal PrecioServicio
        {
            get => nuc_Precio_Servicio != null ? nuc_Precio_Servicio.Value : 0m;
            set
            {
                if (nuc_Precio_Servicio != null)
                    nuc_Precio_Servicio.Value = ClampNumericGuna(nuc_Precio_Servicio, value);
            }
        }

        public bool ActivoServicio
        {
            get => (cmbox_Estado_Servicio?.Text ?? "Activo").Equals("Activo", StringComparison.OrdinalIgnoreCase);
            set
            {
                if (cmbox_Estado_Servicio != null)
                    cmbox_Estado_Servicio.Text = value ? "Activo" : "Inactivo";
            }
        }

        public bool PuedeAcceder => BuildPerm().HasAny("OPS_SERVICIOS_ACCESO", "OPS_SERVICIOS_REGISTRAR", "OPS_SERVICIOS_ACTUALIZAR", "OPS_SERVICIOS_DESACTIVAR");
        public bool PuedeRegistrar => BuildPerm().Has("OPS_SERVICIOS_REGISTRAR");
        public bool PuedeActualizar => BuildPerm().Has("OPS_SERVICIOS_ACTUALIZAR");
        public bool PuedeDesactivar => BuildPerm().Has("OPS_SERVICIOS_DESACTIVAR");

        public void BindCategorias(DataTable dtCategorias)
        {
            var dtFiltro = dtCategorias?.Copy() ?? new DataTable();

            if (!dtFiltro.Columns.Contains("CategoriaServicioID"))
                dtFiltro.Columns.Add("CategoriaServicioID", typeof(int));
            if (!dtFiltro.Columns.Contains("Categoria"))
                dtFiltro.Columns.Add("Categoria", typeof(string));

            bool existeTodos = dtFiltro.AsEnumerable()
                .Any(r => SafeInt(r, "CategoriaServicioID") == 0);

            if (!existeTodos)
                dtFiltro.Rows.InsertAt(dtFiltro.NewRow(), 0);

            dtFiltro.Rows[0]["CategoriaServicioID"] = 0;
            dtFiltro.Rows[0]["Categoria"] = "Todos";

            if (cmbox_CategoriaFiltro_Servicio != null)
            {
                cmbox_CategoriaFiltro_Servicio.DisplayMember = "Categoria";
                cmbox_CategoriaFiltro_Servicio.ValueMember = "CategoriaServicioID";
                cmbox_CategoriaFiltro_Servicio.DataSource = dtFiltro;
            }

            if (cmbox_Categoria_Servicio != null)
            {
                DataTable dtEdit = dtFiltro.Copy();
                var rowTodos = dtEdit.AsEnumerable().FirstOrDefault(x => SafeInt(x, "CategoriaServicioID") == 0);
                if (rowTodos != null) dtEdit.Rows.Remove(rowTodos);

                cmbox_Categoria_Servicio.DisplayMember = "Categoria";
                cmbox_Categoria_Servicio.ValueMember = "CategoriaServicioID";
                cmbox_Categoria_Servicio.DataSource = dtEdit;

                if (dtEdit.Rows.Count > 0)
                    cmbox_Categoria_Servicio.SelectedIndex = 0;
            }
        }

        public void RenderServicios(DataTable dt, int? selectedServicioId)
        {
            if (FlowServicioCard == null) return;

            FlowServicioCard.SuspendLayout();
            try
            {
                foreach (Control c in FlowServicioCard.Controls) c.Dispose();
                FlowServicioCard.Controls.Clear();

                if (dt == null || dt.Rows.Count == 0) return;

                int cardW = Math.Max(10, FlowServicioCard.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 6);

                foreach (DataRow r in dt.Rows)
                {
                    var card = new ServicioTaskCard
                    {
                        Width = cardW,
                        Margin = new Padding(0, 0, 0, 10)
                    };

                    card.Bind(r);
                    card.SetSelected(selectedServicioId.HasValue && card.ServicioID == selectedServicioId.Value);
                    card.CardClicked += (_, __) => _presenter.Seleccionar(card.ServicioID);

                    FlowServicioCard.Controls.Add(card);
                }
            }
            finally
            {
                FlowServicioCard.ResumeLayout(true);
            }
        }

        public void SetResultados(int total)
        {
            if (lbl_Num_Resultados_Servicio != null)
                lbl_Num_Resultados_Servicio.Text = total.ToString();
        }

        public void SetModoActualizar(bool actualizar)
        {
            if (btn_Registrar_Servicio != null)
                btn_Registrar_Servicio.Text = actualizar ? "Actualizar" : "Registrar";
        }

        public void SetGuardarEnabled(bool enabled)
        {
            if (btn_Registrar_Servicio != null)
                btn_Registrar_Servicio.Enabled = enabled;
        }

        public void SetDesactivarEnabled(bool enabled)
        {
            if (btn_Desactivar_Servicio != null)
                btn_Desactivar_Servicio.Enabled = enabled;
        }

        public void SetCodigoLabel(string text)
        {
            if (lbl_Cod_Servicio != null)
                lbl_Cod_Servicio.Text = text ?? "--";
        }

        public void ClearFormInputs()
        {
            if (txt_Codigo_Servicio != null) txt_Codigo_Servicio.Text = "";
            if (txt_Nombre_Servicio != null) txt_Nombre_Servicio.Text = "";
            if (nuc_Precio_Servicio != null) nuc_Precio_Servicio.Value = 0;

            if (cmbox_Categoria_Servicio != null && cmbox_Categoria_Servicio.Items.Count > 0)
                cmbox_Categoria_Servicio.SelectedIndex = 0;

            if (cmbox_Estado_Servicio != null)
                cmbox_Estado_Servicio.Text = "Activo";
        }

        public void FocusNombre()
        {
            txt_Nombre_Servicio?.Focus();
        }

        public void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "Servicios", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ShowWarning(string msg)
        {
            MessageBox.Show(msg, "Servicios", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public void ShowError(string msg, Exception ex = null)
        {
            MessageBox.Show(
                string.IsNullOrWhiteSpace(msg) ? "Ocurrió un error al procesar la operación." : msg,
                "Servicios",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        public void CloseView()
        {
            BeginInvoke(new Action(() => Close()));
        }

        private void ConfigurarUI()
        {
            if (FlowServicioCard != null)
            {
                FlowServicioCard.FlowDirection = FlowDirection.TopDown;
                FlowServicioCard.WrapContents = false;
                FlowServicioCard.AutoScroll = true;
            }

            if (txt_Buscador_Servicio != null) txt_Buscador_Servicio.TextChanged += (_, __) => _presenter.Buscar();
            if (cmbox_CategoriaFiltro_Servicio != null) cmbox_CategoriaFiltro_Servicio.SelectedIndexChanged += (_, __) => _presenter.Buscar();
            if (cmbox_EstadoFiltro_Servicio != null) cmbox_EstadoFiltro_Servicio.SelectedIndexChanged += (_, __) => _presenter.Buscar();

            if (btn_Registrar_Servicio != null) btn_Registrar_Servicio.Click += (_, __) => _presenter.Guardar();
            if (btn_Desactivar_Servicio != null) btn_Desactivar_Servicio.Click += (_, __) => _presenter.Desactivar();
            if (btn_Limpiar_Servicio != null) btn_Limpiar_Servicio.Click += (_, __) => _presenter.Limpiar();

            if (cmbox_EstadoFiltro_Servicio != null)
            {
                cmbox_EstadoFiltro_Servicio.Items.Clear();
                cmbox_EstadoFiltro_Servicio.Items.Add("Todos");
                cmbox_EstadoFiltro_Servicio.Items.Add("Activos");
                cmbox_EstadoFiltro_Servicio.Items.Add("Inactivos");
                cmbox_EstadoFiltro_Servicio.SelectedIndex = 0;
            }

            if (cmbox_Estado_Servicio != null)
            {
                cmbox_Estado_Servicio.Items.Clear();
                cmbox_Estado_Servicio.Items.Add("Activo");
                cmbox_Estado_Servicio.Items.Add("Inactivo");
                cmbox_Estado_Servicio.SelectedIndex = 0;
            }

            if (lbl_Cod_Servicio != null) lbl_Cod_Servicio.Text = "--";
            if (lbl_Num_Resultados_Servicio != null) lbl_Num_Resultados_Servicio.Text = "0";
        }

        private PermissionContext BuildPerm()
        {
            return new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        private static int SafeInt(DataRow r, string col)
        {
            if (r == null || r.Table == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) ? x : 0;
        }

        private static decimal ClampNumericGuna(Guna.UI2.WinForms.Guna2NumericUpDown nud, decimal value)
        {
            if (nud == null) return value;
            if (value < nud.Minimum) return nud.Minimum;
            if (value > nud.Maximum) return nud.Maximum;
            return value;
        }
    }
}