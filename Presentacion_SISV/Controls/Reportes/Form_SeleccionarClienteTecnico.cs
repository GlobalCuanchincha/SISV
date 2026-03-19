using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Services.Reportes;
using Dominio_SISV.DTOs.Clientes;
using System;
using System.Data;
using System.Windows.Forms;
using Union_Formularios_SISV.Forms.Clientes;
using Union_Formularios_SISV.Controls.Ordenes_de_Servicio;
using Union_Formularios_SISV.Controls.Usuarios;

namespace Union_Formularios_SISV.Controls.Reportes
{
    public partial class Form_SeleccionarClienteTecnico : Form
    {
        public enum ModoSeleccion
        {
            Cliente,
            Tecnico,
            OrdenServicio
        }

        public sealed class ResultadoSeleccion
        {
            public int ID { get; set; }
            public string TextoPrincipal { get; set; }
            public string TextoSecundario { get; set; }
        }

        private readonly IReporteServicioService _service = new ReporteServicioService();
        private readonly ModoSeleccion _modo;
        private readonly int? _tecnicoIdFiltroOrden;

        private ResultadoSeleccion _seleccionActual;

        public ResultadoSeleccion Seleccion => _seleccionActual;

        public Form_SeleccionarClienteTecnico(ModoSeleccion modo, int? tecnicoIdFiltroOrden = null)
        {
            InitializeComponent();
            _modo = modo;
            _tecnicoIdFiltroOrden = tecnicoIdFiltroOrden;

            ConfigurarTitulos();
            ConfigurarFlow();
            this.Load += (s, e) => CargarDatos();
        }

        private void ConfigurarFlow()
        {
            flow_ClientTech.FlowDirection = FlowDirection.TopDown;
            flow_ClientTech.WrapContents = false;
            flow_ClientTech.AutoScroll = true;
        }

        private void ConfigurarTitulos()
        {
            switch (_modo)
            {
                case ModoSeleccion.Cliente:
                    lbl_Titulo_Selec.Text = "Seleccionar al Cliente";
                    lbl_Subtitulo_Selec.Text = "Dar doble clic sobre el cliente que se desee escoger";
                    lbl_Espacio1.Text = "Cédula";
                    lbl_Espacio2.Text = "Cliente";
                    lbl_Espacio3.Text = "Correo";
                    lbl_Espacio4.Text = "Teléfono";
                    lbl_Espacio5.Text = "Estado";
                    break;

                case ModoSeleccion.Tecnico:
                    lbl_Titulo_Selec.Text = "Seleccionar al Técnico";
                    lbl_Subtitulo_Selec.Text = "Dar doble clic sobre el técnico que se desee escoger";
                    lbl_Espacio1.Text = "Nombre de usuario";
                    lbl_Espacio2.Text = "Nombres";
                    lbl_Espacio3.Text = "Correo";
                    lbl_Espacio4.Text = "Rol";
                    lbl_Espacio5.Text = "Estado";
                    break;

                default:
                    lbl_Titulo_Selec.Text = "Seleccionar Orden de Servicio";
                    lbl_Subtitulo_Selec.Text = "Dar doble clic sobre la orden de servicio que se desee escoger";
                    lbl_Espacio1.Text = "Código";
                    lbl_Espacio2.Text = "Cliente";
                    lbl_Espacio3.Text = "Equipo";
                    lbl_Espacio4.Text = "Técnico";
                    lbl_Espacio5.Text = "Estado";
                    break;
            }
        }

        private void CargarDatos()
        {
            int usuarioId = Session.UsuarioId;

            DataTable dt;
            switch (_modo)
            {
                case ModoSeleccion.Cliente:
                    dt = _service.BuscarClientes(usuarioId, null);
                    RenderClientes(dt);
                    break;

                case ModoSeleccion.Tecnico:
                    dt = _service.BuscarTecnicos(usuarioId, null);
                    RenderTecnicos(dt);
                    break;

                default:
                    dt = _service.BuscarOrdenes(usuarioId, null, _tecnicoIdFiltroOrden);
                    RenderOrdenes(dt);
                    break;
            }
        }

        private void RenderClientes(DataTable dt)
        {
            flow_ClientTech.Controls.Clear();
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
            {
                var card = new ClientTaskCard();

                var vm = new ClienteCardVM
                {
                    ClienteID = SafeInt(r["ClienteID"]),
                    Cedula = SafeStr(r["Cedula"]),
                    Cliente = SafeStr(r["NombreCompleto"]),
                    Correo = SafeStr(r["Correo"]),
                    Telefono = SafeStr(r["Telefono"]),
                    EsActivo = SafeBool(r["Activo"]),
                    EstadoNombre = SafeBool(r["Activo"]) ? "Activo" : "Inactivo"
                };

                card.Bind(vm);

                // Click simple: deja seleccionado visualmente
                card.ClientSelected += (s, e) =>
                {
                    _seleccionActual = new ResultadoSeleccion
                    {
                        ID = e.Cliente.ClienteID,
                        TextoPrincipal = e.Cliente.Cliente,
                        TextoSecundario = e.Cliente.Cedula
                    };
                };

                // Doble clic: selecciona y cierra de una vez
                HookDoubleClickRecursive(card, () =>
                {
                    _seleccionActual = new ResultadoSeleccion
                    {
                        ID = vm.ClienteID,
                        TextoPrincipal = vm.Cliente,
                        TextoSecundario = vm.Cedula
                    };

                    ConfirmarSeleccion();
                });

                flow_ClientTech.Controls.Add(card);
                card.Width = flow_ClientTech.ClientSize.Width - 25;
            }
        }
        private void RenderTecnicos(DataTable dt)
        {
            flow_ClientTech.Controls.Clear();
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
            {
                bool activo = SafeBool(r["Activo"]);
                if (!activo) continue;

                var usuarioId = SafeInt(r["UsuarioID"]);
                var login = SafeStr(r["LoginName"]);
                var nombres = SafeStr(r["Nombres"]);
                var apellidos = SafeStr(r["Apellidos"]);
                var correo = SafeStr(r["Correo"]);
                var rol = SafeStr(r["RolNombre"]);
                var nombreCompleto = (nombres + " " + apellidos).Trim();

                var card = new UsuariosTaskCard();
                card.Bind(usuarioId, login, nombres, apellidos, correo, rol, activo, false);

                card.UsuarioSeleccionado += (s, e) =>
                {
                    _seleccionActual = new ResultadoSeleccion
                    {
                        ID = usuarioId,
                        TextoPrincipal = nombreCompleto,
                        TextoSecundario = login
                    };
                };

                HookDoubleClickRecursive(card, () =>
                {
                    _seleccionActual = new ResultadoSeleccion
                    {
                        ID = usuarioId,
                        TextoPrincipal = nombreCompleto,
                        TextoSecundario = login
                    };

                    ConfirmarSeleccion();
                });

                flow_ClientTech.Controls.Add(card);
                card.Width = flow_ClientTech.ClientSize.Width - 25;
            }
        }

        private void RenderOrdenes(DataTable dt)
        {
            flow_ClientTech.Controls.Clear();
            if (dt == null) return;

            foreach (DataRow r in dt.Rows)
            {
                int ordenId = SafeInt(r["OrdenServicioID"]);
                string codigo = SafeStr(r["CodigoOrden"]);
                string cliente = SafeStr(r["ClienteNombre"]);
                string correo = SafeStr(r["Correo"]);
                string equipo = SafeStr(r["EquipoNombre"]);
                string estado = SafeStr(r["EstadoNombre"]);

                var card = new OrdenTaskCard();
                card.Bind(ordenId, codigo, cliente, correo, equipo, estado);

                card.CardClicked += (s, e) =>
                {
                    _seleccionActual = new ResultadoSeleccion
                    {
                        ID = ordenId,
                        TextoPrincipal = codigo,
                        TextoSecundario = cliente
                    };
                };

                HookDoubleClickRecursive(card, () =>
                {
                    _seleccionActual = new ResultadoSeleccion
                    {
                        ID = ordenId,
                        TextoPrincipal = codigo,
                        TextoSecundario = cliente
                    };

                    ConfirmarSeleccion();
                });

                flow_ClientTech.Controls.Add(card);
                card.Width = flow_ClientTech.ClientSize.Width - 25;
            }
        }
        private void ConfirmarSeleccion()
        {
            if (_seleccionActual == null || _seleccionActual.ID <= 0)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void HookDoubleClickRecursive(Control c, Action action)
        {
            c.DoubleClick += (s, e) => action();
            foreach (Control child in c.Controls)
                HookDoubleClickRecursive(child, action);
        }

        private static string SafeStr(object v) => v == null || v == DBNull.Value ? "" : v.ToString();

        private static int SafeInt(object v)
        {
            int x;
            return int.TryParse(SafeStr(v), out x) ? x : 0;
        }

        private static bool SafeBool(object v)
        {
            if (v == null || v == DBNull.Value) return false;
            bool b;
            if (bool.TryParse(v.ToString(), out b)) return b;
            int i;
            return int.TryParse(v.ToString(), out i) && i != 0;
        }
    }
}