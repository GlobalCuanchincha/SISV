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
    public sealed class SeleccionClientePresenter
    {
        private readonly ISeleccionClienteView _view;
        private readonly IOrdenesRecepcionService _svc;
        private readonly PermissionContext _perms;

        private int? _clienteSeleccionadoId;
        private string _clienteSeleccionadoNombre;

        public SeleccionClientePresenter(ISeleccionClienteView view, IOrdenesRecepcionService svc = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _svc = svc ?? new OrdenesRecepcionService();

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
                    OpsPermissionCodes.RecepcionAcceso,
                    OpsPermissionCodes.RecepcionCrearOrden,
                    OpsPermissionCodes.RecepcionEditar,
                    OpsPermissionCodes.EquiposAcceso,
                    OpsPermissionCodes.EquiposRegistrar,
                    OpsPermissionCodes.EquiposActualizar);

                if (!puedeUsarSelector)
                {
                    _view.ShowWarning("Acceso denegado.");
                    _view.CloseView();
                    return;
                }

                var dtFiltros = await Task.Run(() => _svc.ClienteFiltrosListar(_view.UsuarioId));
                _view.BindFiltros(dtFiltros);

                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo inicializar la selección de clientes.", ex);
            }
        }

        public async Task BuscarAsync()
        {
            try
            {
                int usuarioId = _view.UsuarioId;
                string filtro = _view.FiltroSeleccionado;
                string textoBusqueda = _view.TextoBusqueda;

                var dt = await Task.Run(() => _svc.ClientesActivosBuscar(
                    usuarioId,
                    filtro,
                    textoBusqueda,
                    200));

                _view.RenderClientes(dt, _clienteSeleccionadoId);
                _view.SetResultados(dt?.Rows.Count ?? 0);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el listado de clientes.", ex);
            }
        }
        public void SeleccionarCliente(int clienteId, string nombreCompleto)
        {
            _clienteSeleccionadoId = clienteId;
            _clienteSeleccionadoNombre = nombreCompleto ?? "";

            _view.SetClienteSeleccionado(clienteId, _clienteSeleccionadoNombre);
            _view.CloseWithOk();
        }
    }
}