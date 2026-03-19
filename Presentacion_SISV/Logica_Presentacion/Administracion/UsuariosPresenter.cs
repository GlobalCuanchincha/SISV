using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Services.Usuarios;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Administracion
{
    public sealed class UsuariosPresenter
    {
        private const string P_ACCESO = "SEC_USUARIOS_ACCESO";
        private const string P_REG = "SEC_USUARIOS_REGISTRAR";
        private const string P_UPD = "SEC_USUARIOS_ACTUALIZAR";
        private const string P_GEST = "SEC_USUARIOS_GESTIONAR_PERM";

        private readonly IUsuariosView _view;
        private readonly IUsuarioService _service;
        private readonly PermissionContext _perm;

        private int? _usuarioSeleccionadoId;
        private DataTable _dtActual;

        public UsuariosPresenter(IUsuariosView view, IUsuarioService service = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _service = service ?? new UsuarioService();
            _perm = new PermissionContext(Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        public async Task InitializeAsync()
        {
            if (_view.UsuarioSesionId <= 0)
            {
                _view.ShowWarning("No se pudo obtener UsuarioID de sesión.");
                _view.CloseView();
                return;
            }

            if (!_perm.HasAny(P_ACCESO, P_REG, P_UPD, P_GEST))
            {
                _view.ShowWarning("Acceso denegado. No tiene permisos para Usuarios.");
                _view.CloseView();
                return;
            }

            try
            {
                _view.EnsureFiltroCombo();

                byte rolSesion = await _service.GetRolSesionAsync(_view.UsuarioSesionId);

                var dtRoles = await _service.ListarRolesAsync(true);
                dtRoles = FiltrarRolesSegunSesion(dtRoles, rolSesion);
                _view.BindRoles(dtRoles);

                var dtFiltro = await _service.ListarEstadosAsync("Filtro");
                var dtForm = await _service.ListarEstadosAsync("Form");
                _view.BindEstados(dtFiltro, dtForm);

                _view.ResetForm();
                await BuscarAsync();
                UpdateUiPermissions();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo inicializar Usuarios.", ex);
            }
        }        

        public async Task BuscarAsync()
        {
            try
            {
                string texto = (_view.TextoBusqueda ?? "").Trim();
                string filtroRaw = (_view.FiltroTexto ?? "Todos").Trim().ToLowerInvariant();
                string rolFiltro = (_view.RolFiltroTexto ?? "Todos").Trim();
                string estadoTxt = (_view.EstadoFiltroTexto ?? "Todos").Trim();

                bool hasAny = !string.IsNullOrWhiteSpace(texto)
                              || !string.Equals(rolFiltro, "Todos", StringComparison.OrdinalIgnoreCase)
                              || !string.Equals(estadoTxt, "Todos", StringComparison.OrdinalIgnoreCase);

                if (!hasAny)
                {
                    _dtActual = await _service.ListarRecientesAsync(_view.UsuarioSesionId, 200);
                }
                else
                {
                    string filtroKey = "todos";
                    if (filtroRaw.Contains("usuario")) filtroKey = "login";
                    else if (filtroRaw.Contains("nombre")) filtroKey = "nombre";
                    else if (filtroRaw.Contains("correo") || filtroRaw.Contains("email")) filtroKey = "email";

                    _dtActual = await _service.BuscarAsync(
                        _view.UsuarioSesionId,
                        texto,
                        filtroKey,
                        rolFiltro,
                        estadoTxt,
                        200);
                }

                if (_dtActual == null)
                    _dtActual = new DataTable();

                _view.RenderUsuarios(_dtActual, _usuarioSeleccionadoId);
                UpdateUiPermissions();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el listado de usuarios.", ex);
            }
        }

        public async Task SeleccionarAsync(int usuarioId)
        {
            try
            {
                _usuarioSeleccionadoId = usuarioId;
                _view.RenderUsuarios(_dtActual, _usuarioSeleccionadoId);

                var dt = await _service.GetByIdAsync(_view.UsuarioSesionId, usuarioId);
                if (dt == null || dt.Rows.Count == 0) return;

                var row = dt.Rows[0];
                _view.ShowUsuarioDetalle(row);

                try
                {
                    var foto = await _service.GetFotoAsync(_view.UsuarioSesionId, usuarioId);
                    if (foto != null && foto.Length > 0) _view.SetFotoFromBytes(foto);
                    else _view.SetDefaultFoto();
                }
                catch
                {
                    _view.SetDefaultFoto();
                }

                _view.SetModeActualizar(true);
                UpdateUiPermissions();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo cargar el detalle del usuario.", ex);
            }
        }

        public async Task GuardarAsync()
        {
            try
            {
                bool esNuevo = !_usuarioSeleccionadoId.HasValue;

                if (esNuevo && !_perm.Has(P_REG))
                {
                    _view.ShowWarning("No tiene permisos para registrar usuarios.");
                    return;
                }

                if (!esNuevo && !_perm.Has(P_UPD))
                {
                    _view.ShowWarning("No tiene permisos para actualizar usuarios.");
                    return;
                }

                var req = _view.BuildGuardarRequest(_usuarioSeleccionadoId);
                int idGuardado = await _service.GuardarAsync(req);

                _view.ClearPendingFoto();

                await BuscarAsync();
                await SeleccionarAsync(idGuardado);

                _view.ShowInfo(esNuevo ? "Usuario registrado." : "Usuario actualizado.");
            }
            catch (Exception ex)
            {
                _view.ShowWarning(ex.Message);
            }
        }

        public void Limpiar()
        {
            _usuarioSeleccionadoId = null;
            _view.ResetForm();
            _view.RenderUsuarios(_dtActual, null);
            UpdateUiPermissions();
        }

        private void UpdateUiPermissions()
        {
            bool puedeEditar = _usuarioSeleccionadoId.HasValue ? _perm.Has(P_UPD) : _perm.Has(P_REG);

            _view.SetEditingEnabled(puedeEditar);
            _view.SetGuardarEnabled(puedeEditar);
            _view.SetGestionarPermisosEnabled(_perm.Has(P_GEST));
        }
        private DataTable FiltrarRolesSegunSesion(DataTable dtRoles, byte rolSesion)
        {
            if (dtRoles == null) return new DataTable();

            if (rolSesion == 1) return dtRoles;

            if (rolSesion == 2)
            {
                var dt = dtRoles.Clone();

                foreach (DataRow r in dtRoles.Rows)
                {
                    int rolId = 0;

                    if (dtRoles.Columns.Contains("RolID") && r["RolID"] != DBNull.Value)
                        rolId = Convert.ToInt32(r["RolID"]);
                    else if (dtRoles.Columns.Contains("RoleID") && r["RoleID"] != DBNull.Value)
                        rolId = Convert.ToInt32(r["RoleID"]);
                    else if (dtRoles.Columns.Contains("IdRol") && r["IdRol"] != DBNull.Value)
                        rolId = Convert.ToInt32(r["IdRol"]);
                    else if (dtRoles.Columns.Contains("ID") && r["ID"] != DBNull.Value)
                        rolId = Convert.ToInt32(r["ID"]);

                    if (rolId == 1 || rolId == 2)
                        continue;

                    dt.ImportRow(r);
                }

                return dt;
            }

            return dtRoles;
        }
    }
}
