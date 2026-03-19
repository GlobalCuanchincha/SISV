using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Permisos;
using Dominio_SISV.Services.OrdenesServicio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Recepcion
{
    public sealed class OrdenesRecepcionPresenter
    {
        private readonly IOrdenesRecepcionView _view;
        private readonly IOrdenesRecepcionService _svc;
        private readonly PermissionContext _perms;

        private int _ordenSeleccionadaId = 0;
        private int? _clienteIdSeleccionado = null;

        private readonly bool _puedeRecepcion;
        private readonly bool _puedeCrear;
        private readonly bool _puedeEditar;
        private readonly bool _puedeAsignarTecnico;
        private readonly bool _puedeVerEquipos;
        private readonly bool _puedeVerNotificacion;

        public OrdenesRecepcionPresenter(IOrdenesRecepcionView view, IOrdenesRecepcionService svc = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _svc = svc ?? new OrdenesRecepcionService();

            _perms = new PermissionContext(
                Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            _puedeRecepcion = _perms.HasAny(
                OpsPermissionCodes.RecepcionAcceso,
                OpsPermissionCodes.RecepcionCrearOrden,
                OpsPermissionCodes.RecepcionAsignarTecnico,
                OpsPermissionCodes.RecepcionEditar);

            _puedeCrear = _perms.HasAny(OpsPermissionCodes.RecepcionCrearOrden);
            _puedeEditar = _perms.HasAny(OpsPermissionCodes.RecepcionEditar);
            _puedeAsignarTecnico = _perms.HasAny(OpsPermissionCodes.RecepcionAsignarTecnico);

            _puedeVerEquipos = _perms.HasAny(
                OpsPermissionCodes.EquiposAcceso,
                OpsPermissionCodes.EquiposRegistrar,
                OpsPermissionCodes.EquiposActualizar,
                OpsPermissionCodes.EquiposDesactivar);

            _puedeVerNotificacion = _perms.HasAny(
                OpsPermissionCodes.NotifAcceso,
                OpsPermissionCodes.NotifGuardarDiag,
                OpsPermissionCodes.NotifCambiarEstado,
                OpsPermissionCodes.NotifEnviarCorreo);
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

                if (!_puedeRecepcion)
                {
                    _view.ShowWarning("Acceso denegado. No tiene permisos para RECEPCIÓN.");
                    _view.CloseView();
                    return;
                }

                _view.SetVisibilidadNavegacion(_puedeVerEquipos, _puedeVerNotificacion);

                await CargarEstadosFiltroAsync();

                if (_puedeAsignarTecnico)
                {
                    await CargarTecnicosAsync();
                    _view.SetTecnicoHabilitado(true);
                }
                else
                {
                    _view.BindTecnicosNoDisponible();
                    _view.SetTecnicoHabilitado(false);
                }

                await NuevoAsync();
                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo inicializar Recepción.", ex);
            }
        }

        public async Task BuscarAsync()
        {
            try
            {
                string buscarTexto = _view.BuscarTexto;
                short estadoFiltro = _view.EstadoFiltroValor;

                var dt = await Task.Run(() => _svc.Buscar(buscarTexto, estadoFiltro, 200));

                _view.RenderOrdenes(dt, _ordenSeleccionadaId);
                _view.SetResultados(dt?.Rows.Count ?? 0);
                UpdateActionState();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el listado de órdenes.", ex);
            }
        }

        public async Task NuevoAsync()
        {
            _ordenSeleccionadaId = 0;
            _clienteIdSeleccionado = null;

            _view.ProblemaReportado = "";
            _view.AccesoriosRecibidos = "";

            _view.ClearClienteSeleccionado();
            _view.ClearEquiposCliente();
            _view.SetTecnicoSeleccionado(0);
            _view.SetModoActualizar(false);
            _view.RenderOrdenes(new DataTable(), 0);

            await GenerarCodigoAsync();
            UpdateActionState();
        }

        public async Task SelectOrderAsync(int ordenServicioId)
        {
            try
            {
                _ordenSeleccionadaId = ordenServicioId;

                var dt = await Task.Run(() => _svc.GetById(ordenServicioId));
                if (dt == null || dt.Rows.Count == 0)
                {
                    _view.ShowWarning("No se encontró la orden seleccionada.");
                    return;
                }

                var row = dt.Rows[0];

                _view.SetCodigoOrden(S(row, "CodigoOrden"));

                _clienteIdSeleccionado = I(row, (int?)null, "ClienteID");
                _view.SetClienteSeleccionado(_clienteIdSeleccionado, S(row, "ClienteNombre", "Cliente"));

                if (_clienteIdSeleccionado.HasValue)
                    await CargarEquiposClienteAsync(_clienteIdSeleccionado.Value);

                int equipoId = I(row, -1, "EquipoID");
                _view.SetEquipoSeleccionado(equipoId);

                _view.ProblemaReportado = S(row, "ProblemaReportado");
                _view.AccesoriosRecibidos = S(row, "AccesoriosRecibidos");

                int tecnicoId = I(row, 0, "TecnicoID");
                _view.SetTecnicoSeleccionado(tecnicoId);

                _view.SetModoActualizar(true);
                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el detalle de la orden.", ex);
            }
        }

        public async Task SelectClientAsync()
        {
            try
            {
                if (!_view.TrySeleccionarCliente(out int? clienteId, out string clienteNombre))
                    return;

                _clienteIdSeleccionado = clienteId;
                _view.SetClienteSeleccionado(clienteId, clienteNombre);

                if (_clienteIdSeleccionado.HasValue)
                    await CargarEquiposClienteAsync(_clienteIdSeleccionado.Value);
                else
                    _view.ClearEquiposCliente();

                _ordenSeleccionadaId = 0;
                _view.SetModoActualizar(false);
                await GenerarCodigoAsync();
                UpdateActionState();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo seleccionar el cliente.", ex);
            }
        }

        public async Task AssignTechnicianAsync()
        {
            try
            {
                if (!_puedeAsignarTecnico)
                {
                    _view.ShowWarning("No tiene permiso para ASIGNAR TÉCNICO.");
                    return;
                }

                if (_ordenSeleccionadaId <= 0)
                {
                    _view.ShowWarning("Primero seleccione una orden del listado.");
                    return;
                }

                int tecnicoId = _view.TecnicoSeleccionadoId;
                if (tecnicoId <= 0)
                {
                    _view.ShowWarning("Seleccione un técnico.");
                    return;
                }

                await Task.Run(() => _svc.SetTecnico(_view.UsuarioId, _ordenSeleccionadaId, tecnicoId));

                _view.ShowInfo("Técnico asignado.");
                await BuscarAsync();
                await SelectOrderAsync(_ordenSeleccionadaId);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo asignar el técnico.", ex);
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                bool esNuevo = _ordenSeleccionadaId <= 0;

                if (esNuevo && !_puedeCrear)
                {
                    _view.ShowWarning("No tiene permiso para CREAR orden.");
                    return;
                }

                if (!esNuevo && !_puedeEditar)
                {
                    _view.ShowWarning("No tiene permiso para EDITAR orden.");
                    return;
                }

                if (_clienteIdSeleccionado == null || _clienteIdSeleccionado <= 0)
                {
                    _view.ShowWarning("Seleccione un cliente.");
                    return;
                }

                int equipoId = _view.EquipoSeleccionadoId;
                if (equipoId <= 0)
                {
                    _view.ShowWarning("Seleccione un equipo del cliente.");
                    return;
                }

                int? tecnicoId = null;

                if (_puedeAsignarTecnico)
                {
                    int tecnicoSeleccionado = _view.TecnicoSeleccionadoId;
                    if (tecnicoSeleccionado <= 0)
                    {
                        _view.ShowWarning("Seleccione un técnico.");
                        return;
                    }

                    tecnicoId = tecnicoSeleccionado;
                }

                string buscarTexto = _view.BuscarTexto; // no obligatorio, solo por consistencia UI-thread
                int usuarioId = _view.UsuarioId;
                int? ordenId = esNuevo ? (int?)null : _ordenSeleccionadaId;
                int clienteId = _clienteIdSeleccionado.Value;
                string detalles = (_view.ProblemaReportado ?? "").Trim();
                string accesorios = (_view.AccesoriosRecibidos ?? "").Trim();

                var dt = await Task.Run(() => _svc.GuardarRecepcion(
                    usuarioId,
                    ordenId,
                    clienteId,
                    equipoId,
                    tecnicoId,
                    detalles,
                    accesorios
                ));

                if (dt == null || dt.Rows.Count == 0)
                {
                    _view.ShowWarning("No se pudo guardar la orden.");
                    return;
                }

                _ordenSeleccionadaId = I(dt.Rows[0], 0, "OrdenServicioID");

                string codigo = S(dt.Rows[0], "CodigoOrden");
                if (!string.IsNullOrWhiteSpace(codigo))
                    _view.SetCodigoOrden(codigo);

                _view.ShowInfo(esNuevo ? "Orden creada." : "Orden actualizada.");

                await BuscarAsync();
                await SelectOrderAsync(_ordenSeleccionadaId);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo guardar la orden.", ex);
            }
        }
        private async Task CargarEstadosFiltroAsync()
        {
            var dt = await Task.Run(() => _svc.EstadosListar());
            _view.BindEstadosFiltro(dt);
        }

        private async Task CargarTecnicosAsync()
        {
            var dt = await Task.Run(() => _svc.TecnicosListarActivos(_view.UsuarioId));
            _view.BindTecnicos(dt);
        }

        private async Task CargarEquiposClienteAsync(int clienteId)
        {
            var dt = await Task.Run(() => _svc.EquiposListarPorCliente(clienteId));

            if (dt == null || !dt.Columns.Contains("EquipoID") || !dt.Columns.Contains("EquipoTexto"))
                throw new Exception("La lista de equipos del cliente no devolvió las columnas esperadas.");

            _view.BindEquiposCliente(dt);
        }

        private async Task GenerarCodigoAsync()
        {
            var dt = await Task.Run(() => _svc.GenerarCodigoOrden());

            string codigo = "OS-????";
            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("CodigoOrdenSugerido"))
                codigo = Convert.ToString(dt.Rows[0]["CodigoOrdenSugerido"]);

            _view.SetCodigoOrden(codigo);
        }

        private void UpdateActionState()
        {
            bool puedeGuardar = _ordenSeleccionadaId > 0 ? _puedeEditar : _puedeCrear;
            bool puedeAsignar = _ordenSeleccionadaId > 0 && _puedeAsignarTecnico;

            _view.SetPermisosAcciones(puedeGuardar, puedeAsignar);
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

        private static int? I(DataRow row, int? def, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToInt32(row[c]);

            return def;
        }
    }
}