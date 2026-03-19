using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Permisos;
using Dominio_SISV.Services.OrdenesServicio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Shared
{
    public sealed class SeleccionOrdenPresenter
    {
        private readonly ISeleccionOrdenView _view;
        private readonly IOrdenesNotificacionService _svc;
        private readonly PermissionContext _perms;

        private int? _ordenSeleccionadaId;

        public SeleccionOrdenPresenter(ISeleccionOrdenView view, IOrdenesNotificacionService svc = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _svc = svc ?? new OrdenesNotificacionService();

            _perms = new PermissionContext(
                Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_view.UsuarioId <= 0)
                {
                    _view.ShowWarning("No se pudo obtener UsuarioID de sesión.");
                    _view.CloseView();
                    return;
                }

                bool puedeUsarSelector = _perms.HasAny(
                    OpsPermissionCodes.NotifAcceso,
                    OpsPermissionCodes.NotifGuardarDiag,
                    OpsPermissionCodes.NotifCambiarEstado,
                    OpsPermissionCodes.NotifEnviarCorreo);

                if (!puedeUsarSelector)
                {
                    _view.ShowWarning("Acceso denegado.");
                    _view.CloseView();
                    return;
                }

                _view.BindFiltros(BuildFiltrosDataTable());
                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo inicializar la selección de órdenes.", ex);
            }
        }

        public async Task BuscarAsync()
        {
            try
            {
                var dt = await Task.Run(() => _svc.ListarParaSeleccion(
                    _view.FiltroSeleccionado,
                    _view.TextoBusqueda));

                _view.RenderOrdenes(dt, _ordenSeleccionadaId);
                _view.SetResultados(dt?.Rows.Count ?? 0);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el listado de órdenes.", ex);
            }
        }

        public void SeleccionarOrden(int ordenServicioId)
        {
            _ordenSeleccionadaId = ordenServicioId;
            _view.SetOrdenSeleccionada(ordenServicioId);
            _view.CloseWithOk();
        }

        private static DataTable BuildFiltrosDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Value", typeof(string));
            dt.Columns.Add("Text", typeof(string));

            dt.Rows.Add("ORDEN", "Orden");
            dt.Rows.Add("CLIENTE", "Cliente");
            dt.Rows.Add("CORREO", "Correo");
            dt.Rows.Add("EQUIPO", "Equipo");
            dt.Rows.Add("ESTADO", "Estado");

            return dt;
        }
    }
}