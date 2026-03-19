using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Permisos;
using Dominio_SISV.Services.OrdenesServicio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Equipos
{
    public sealed class OrdenesEquiposPresenter
    {
        private readonly IOrdenesEquiposView _view;
        private readonly IOrdenesEquipoService _svc;
        private readonly PermissionContext _perms;

        private int _equipoSeleccionadoId = 0;
        private int? _clienteIdSeleccionado = null;
        private string _clienteNombreSeleccionado = null;
        private DataTable _ultimoListado = new DataTable();

        private readonly bool _puedeAcceder;
        private readonly bool _puedeRegistrar;
        private readonly bool _puedeActualizar;
        private readonly bool _puedeVerRecepcion;
        private readonly bool _puedeVerNotificacion;

        public OrdenesEquiposPresenter(IOrdenesEquiposView view, IOrdenesEquipoService svc = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _svc = svc ?? new OrdenesEquipoService();

            _perms = new PermissionContext(
                Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            _puedeAcceder = _perms.HasAny(
                OpsPermissionCodes.EquiposAcceso,
                OpsPermissionCodes.EquiposRegistrar,
                OpsPermissionCodes.EquiposActualizar,
                OpsPermissionCodes.EquiposDesactivar);

            _puedeRegistrar = _perms.HasAny(OpsPermissionCodes.EquiposRegistrar);
            _puedeActualizar = _perms.HasAny(OpsPermissionCodes.EquiposActualizar);

            _puedeVerRecepcion = _perms.HasAny(
                OpsPermissionCodes.RecepcionAcceso,
                OpsPermissionCodes.RecepcionCrearOrden,
                OpsPermissionCodes.RecepcionAsignarTecnico,
                OpsPermissionCodes.RecepcionEditar);

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

                if (!_puedeAcceder)
                {
                    _view.ShowWarning("Acceso denegado. No tiene permisos para EQUIPOS.");
                    _view.CloseView();
                    return;
                }

                _view.SetVisibilidadNavegacion(_puedeVerRecepcion, _puedeVerNotificacion);

                await CargarCombosAsync();
                await NuevoAsync();
                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo inicializar Equipos.", ex);
            }
        }

        public async Task BuscarAsync()
        {
            try
            {
                int usuarioId = _view.UsuarioId;
                int? clienteId = _clienteIdSeleccionado;
                string filtro = _view.FiltroSeleccionado;
                string buscar = _view.BuscarTexto;

                var dt = await Task.Run(() => _svc.Buscar(
                    usuarioId,
                    clienteId,
                    filtro,
                    buscar,
                    null,
                    200));

                _ultimoListado = dt ?? new DataTable();

                _view.RenderEquipos(_ultimoListado, _equipoSeleccionadoId);
                _view.SetResultados(_ultimoListado.Rows.Count);
                UpdateActionState();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el listado de equipos.", ex);
            }
        }
        public async Task NuevoAsync()
        {
            _equipoSeleccionadoId = 0;

            _view.CodigoInterno = "";
            _view.Marca = "";
            _view.Modelo = "";
            _view.Serie = "";
            _view.ColorEquipo = "";
            _view.Accesorios = "";
            _view.Observaciones = "";

            _view.SetTipoEquipoSeleccionado(null);
            _view.SetConectividadSeleccionada(null);
            _view.SetModoActualizar(false);

            _view.RenderEquipos(_ultimoListado ?? new DataTable(), 0);

            try
            {
                var dt = await Task.Run(() => _svc.GenerarCodigoInterno(_view.UsuarioId));
                if (dt != null && dt.Rows.Count > 0)
                    _view.CodigoInterno = S(dt.Rows[0], "CodigoInternoSugerido", "CodigoInterno");
            }
            catch
            {
                _view.CodigoInterno = "";
            }

            if (string.IsNullOrWhiteSpace(_clienteNombreSeleccionado))
                _view.ClienteNombre = "Sin selección";
            else
                _view.ClienteNombre = _clienteNombreSeleccionado;

            UpdateActionState();
        }

        public async Task SeleccionarEquipoAsync(int equipoId)
        {
            try
            {
                _equipoSeleccionadoId = equipoId;
                _view.RenderEquipos(_ultimoListado ?? new DataTable(), _equipoSeleccionadoId);

                var dt = await Task.Run(() => _svc.GetById(_view.UsuarioId, equipoId));
                if (dt == null || dt.Rows.Count == 0)
                {
                    _view.ShowWarning("No se encontró el equipo seleccionado.");
                    return;
                }

                var row = dt.Rows[0];

                _clienteIdSeleccionado = I(row, (int?)null, "ClienteID", "ClienteID_Clientes");
                _clienteNombreSeleccionado = S(row, "ClienteNombre", "Cliente");
                _view.ClienteNombre = string.IsNullOrWhiteSpace(_clienteNombreSeleccionado)
                    ? "Sin selección"
                    : _clienteNombreSeleccionado;

                _view.CodigoInterno = S(row, "CodigoInterno", "Codigo");
                _view.Marca = S(row, "Marca");
                _view.Modelo = S(row, "Modelo");
                _view.Serie = S(row, "Serie", "NumeroSerie");
                _view.ColorEquipo = S(row, "Color");
                _view.Accesorios = S(row, "Accesorios");
                _view.Observaciones = S(row, "Observaciones");

                _view.SetTipoEquipoSeleccionado(GetRowValue(row, "TipoEquipoID"));
                _view.SetConectividadSeleccionada(GetRowValue(row, "Conectividad"));

                _view.SetModoActualizar(true);
                UpdateActionState();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el detalle del equipo.", ex);
            }
        }

        public async Task ElegirClienteAsync()
        {
            try
            {
                bool puedeEditar = _equipoSeleccionadoId > 0 ? _puedeActualizar : _puedeRegistrar;
                if (!puedeEditar)
                {
                    _view.ShowWarning("No tiene permisos para modificar el cliente del equipo.");
                    return;
                }

                if (!_view.TrySeleccionarCliente(out int? clienteId, out string clienteNombre))
                    return;

                _clienteIdSeleccionado = clienteId;
                _clienteNombreSeleccionado = clienteNombre ?? "";
                _view.ClienteNombre = string.IsNullOrWhiteSpace(_clienteNombreSeleccionado)
                    ? "Sin selección"
                    : _clienteNombreSeleccionado;

                await NuevoAsync();
                await BuscarAsync();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo seleccionar el cliente.", ex);
            }
        }

        public async Task GuardarAsync()
        {
            try
            {
                bool esNuevo = _equipoSeleccionadoId <= 0;

                if (esNuevo && !_puedeRegistrar)
                {
                    _view.ShowWarning("No tiene permiso para REGISTRAR equipos.");
                    return;
                }

                if (!esNuevo && !_puedeActualizar)
                {
                    _view.ShowWarning("No tiene permiso para ACTUALIZAR equipos.");
                    return;
                }

                if (!_clienteIdSeleccionado.HasValue || _clienteIdSeleccionado.Value <= 0)
                {
                    _view.ShowWarning("Seleccione un cliente para el equipo.");
                    return;
                }

                int usuarioId = _view.UsuarioId;
                int? equipoId = esNuevo ? (int?)null : _equipoSeleccionadoId;
                int clienteId = _clienteIdSeleccionado.Value;
                int tipoEquipoId = _view.TipoEquipoSeleccionadoId;
                string codigoInterno = (_view.CodigoInterno ?? "").Trim();
                string marca = NullIfEmpty(_view.Marca);
                string modelo = NullIfEmpty(_view.Modelo);
                string serie = NullIfEmpty(_view.Serie);
                string color = NullIfEmpty(_view.ColorEquipo);
                string conectividad = NullIfEmpty(_view.ConectividadSeleccionada);
                string accesorios = NullIfEmpty(_view.Accesorios);
                string observaciones = NullIfEmpty(_view.Observaciones);

                if (tipoEquipoId <= 0)
                {
                    _view.ShowWarning("Seleccione el tipo de equipo.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(codigoInterno))
                {
                    _view.ShowWarning("No se generó el código interno.");
                    return;
                }

                var dt = await Task.Run(() => _svc.Guardar(
                    usuarioId,
                    equipoId,
                    clienteId,
                    tipoEquipoId,
                    codigoInterno,
                    marca,
                    modelo,
                    serie,
                    color,
                    conectividad,
                    accesorios,
                    observaciones,
                    true
                ));

                if (dt != null && dt.Rows.Count > 0)
                    _equipoSeleccionadoId = I(dt.Rows[0], _equipoSeleccionadoId, "EquipoID");

                _view.ShowInfo(esNuevo ? "Equipo registrado." : "Equipo actualizado.");

                await BuscarAsync();
                if (_equipoSeleccionadoId > 0)
                    await SeleccionarEquipoAsync(_equipoSeleccionadoId);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo guardar el equipo.", ex);
            }
        }
        private async Task CargarCombosAsync()
        {
            var dtFiltros = await Task.Run(() => _svc.FiltrosListar(_view.UsuarioId));
            var dtTipo = await Task.Run(() => _svc.TipoEquipoListar(_view.UsuarioId));
            var dtCon = await Task.Run(() => _svc.ConectividadListar(_view.UsuarioId));

            _view.BindFiltros(dtFiltros);
            _view.BindTiposEquipo(dtTipo);
            _view.BindConectividades(dtCon);
        }

        private void UpdateActionState()
        {
            bool puedeGuardar = _equipoSeleccionadoId > 0 ? _puedeActualizar : _puedeRegistrar;
            bool puedeElegirCliente = puedeGuardar;

            _view.SetModoActualizar(_equipoSeleccionadoId > 0);
            _view.SetPermisosAcciones(puedeGuardar, puedeElegirCliente);
        }

        private static object GetRowValue(DataRow row, string col)
        {
            if (row == null || row.Table == null) return null;
            if (!row.Table.Columns.Contains(col)) return null;
            return row[col] == DBNull.Value ? null : row[col];
        }

        private static string NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
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