using Capa_Corte_Transversal.Config;
using Capa_Corte_Transversal.Helpers;
using Capa_Corte_Transversal.Loggin;
using Dominio_SISV.Permisos;
using Dominio_SISV.Services.OrdenesServicio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Union_Formularios_SISV.Controls.Usuarios.Permisos;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Notificacion
{
    public sealed class OrdenesNotificacionPresenter
    {
        private readonly IOrdenesNotificacionView _view;
        private readonly IOrdenesNotificacionService _svc;
        private readonly PermissionContext _perms;

        private int _ordenSeleccionadaId = 0;
        private string _codigoOrdenSeleccionada = "";

        private readonly bool _puedeAcceder;
        private readonly bool _puedeGuardarDiag;
        private readonly bool _puedeCambiarEstado;
        private readonly bool _puedeEnviarCorreo;
        private readonly bool _puedeVerRecepcion;
        private readonly bool _puedeVerEquipos;

        public OrdenesNotificacionPresenter(IOrdenesNotificacionView view, IOrdenesNotificacionService svc = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _svc = svc ?? new OrdenesNotificacionService();

            _perms = new PermissionContext(
                Session.Permisos ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));

            _puedeAcceder = _perms.HasAny(
                OpsPermissionCodes.NotifAcceso,
                OpsPermissionCodes.NotifGuardarDiag,
                OpsPermissionCodes.NotifCambiarEstado,
                OpsPermissionCodes.NotifEnviarCorreo);

            _puedeGuardarDiag = _perms.HasAny(OpsPermissionCodes.NotifGuardarDiag);
            _puedeCambiarEstado = _perms.HasAny(OpsPermissionCodes.NotifCambiarEstado);
            _puedeEnviarCorreo = _perms.HasAny(OpsPermissionCodes.NotifEnviarCorreo);

            _puedeVerRecepcion = _perms.HasAny(
                OpsPermissionCodes.RecepcionAcceso,
                OpsPermissionCodes.RecepcionCrearOrden,
                OpsPermissionCodes.RecepcionAsignarTecnico,
                OpsPermissionCodes.RecepcionEditar);

            _puedeVerEquipos = _perms.HasAny(
                OpsPermissionCodes.EquiposAcceso,
                OpsPermissionCodes.EquiposRegistrar,
                OpsPermissionCodes.EquiposActualizar,
                OpsPermissionCodes.EquiposDesactivar);
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
                    _view.ShowWarning("Acceso denegado. No tiene permisos para NOTIFICACIÓN.");
                    _view.CloseView();
                    return;
                }

                _view.SetVisibilidadNavegacion(_puedeVerRecepcion, _puedeVerEquipos);
                _view.SetPermisosAcciones(_puedeGuardarDiag, _puedeCambiarEstado, _puedeEnviarCorreo);

                await CargarEstadosAsync();
                LimpiarPantallaOrden();
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo inicializar Notificación.", ex);
            }
        }

        public async Task SeleccionarOrdenAsync()
        {
            try
            {
                if (!_view.TrySeleccionarOrden(out int ordenServicioId))
                    return;

                if (ordenServicioId <= 0)
                    return;

                _ordenSeleccionadaId = ordenServicioId;
                await CargarDetalleOrdenAsync(_ordenSeleccionadaId);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo seleccionar la orden.", ex);
            }
        }

        public async Task GuardarDiagnosticoAsync()
        {
            try
            {
                if (!_puedeGuardarDiag)
                {
                    _view.ShowWarning("No tiene permiso para GUARDAR DIAGNÓSTICO.");
                    return;
                }

                if (_ordenSeleccionadaId <= 0)
                {
                    _view.ShowWarning("Primero seleccione una orden.");
                    return;
                }

                string diag = (_view.Diagnostico ?? "").Trim();
                if (string.IsNullOrWhiteSpace(diag))
                {
                    _view.ShowWarning("Ingrese el diagnóstico.");
                    return;
                }

                await Task.Run(() => _svc.GuardarDiagnostico(_view.UsuarioId, _ordenSeleccionadaId, diag));

                _view.ShowInfo("Diagnóstico guardado correctamente.");
                _view.SetActualizacionDiagnostico(DateTime.Now);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo guardar el diagnóstico.", ex);
            }
        }

        public async Task GuardarEstadoAsync()
        {
            try
            {
                if (!_puedeCambiarEstado)
                {
                    _view.ShowWarning("No tiene permiso para CAMBIAR ESTADO.");
                    return;
                }

                if (_ordenSeleccionadaId <= 0)
                {
                    _view.ShowWarning("Primero seleccione una orden.");
                    return;
                }

                if (!_view.TieneEstadoSeleccionado)
                {
                    _view.ShowWarning("Seleccione un estado.");
                    return;
                }

                int nuevoEstadoId = _view.EstadoSeleccionadoId;
                await Task.Run(() => _svc.ActualizarEstado(_view.UsuarioId, _ordenSeleccionadaId, nuevoEstadoId));

                await CargarDetalleOrdenAsync(_ordenSeleccionadaId);

                _view.ShowInfo("Estado actualizado correctamente.");
                _view.SetActualizacionDiagnostico(DateTime.Now);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo actualizar el estado.", ex);
            }
        }

        public void PrevisualizarNotificacion()
        {
            if (_ordenSeleccionadaId <= 0)
            {
                _view.ShowWarning("Primero seleccione una orden.");
                return;
            }

            string correo = (_view.CorreoNotificacion ?? "").Trim();
            string asunto = (_view.AsuntoNotificacion ?? "").Trim();
            string mensaje = (_view.MensajeNotificacion ?? "").Trim();

            _view.MostrarPrevisualizacion(correo, asunto, mensaje);
        }

        public async Task EnviarNotificacionAsync()
        {
            try
            {
                if (!_puedeEnviarCorreo)
                {
                    _view.ShowWarning("No tiene permiso para ENVIAR CORREO.");
                    return;
                }

                if (_ordenSeleccionadaId <= 0)
                {
                    _view.ShowWarning("Primero seleccione una orden.");
                    return;
                }

                string correo = (_view.CorreoNotificacion ?? "").Trim();
                string asunto = (_view.AsuntoNotificacion ?? "").Trim();
                string mensaje = (_view.MensajeNotificacion ?? "").Trim();

                if (string.IsNullOrWhiteSpace(correo) ||
                    string.IsNullOrWhiteSpace(asunto) ||
                    string.IsNullOrWhiteSpace(mensaje))
                {
                    _view.ShowWarning("Complete Correo, Asunto y Mensaje.");
                    return;
                }

                string estadoEnvio = "ENVIADO";
                string errorDetalle = null;

                try
                {
                    await Task.Run(() =>
                    {
                        var sender = new SmtpEmailSender(SmtpSettings.FromAppConfig());
                        sender.Send(correo, asunto, mensaje);
                    });
                }
                catch (Exception ex)
                {
                    estadoEnvio = "ERROR";
                    errorDetalle = ex.Message;
                }

                int notifId = await Task.Run(() => _svc.RegistrarNotificacion(
                    _view.UsuarioId,
                    _ordenSeleccionadaId,
                    correo,
                    asunto,
                    mensaje,
                    estadoEnvio,
                    errorDetalle
                ));

                if (estadoEnvio == "ENVIADO")
                {
                    _view.ShowInfo("Notificación enviada y registrada. (ID: " + notifId + ")");
                }
                else
                {
                    _view.ShowWarning(
                        "No se pudo enviar el correo.\n\nSe registró el intento (ID: " + notifId + ").");
                }

                _view.SetActualizacionNotificacion(DateTime.Now);
            }
            catch (Exception ex)
            {
                _view.ShowError("No se pudo enviar o registrar la notificación.", ex);
            }
        }

        public void LimpiarPantallaOrden()
        {
            _ordenSeleccionadaId = 0;
            _codigoOrdenSeleccionada = "";
            _view.ClearOrden();
        }

        private async Task CargarEstadosAsync()
        {
            var dt = await Task.Run(() => _svc.EstadosListar());
            _view.BindEstados(dt);
        }

        private async Task CargarDetalleOrdenAsync(int ordenServicioId)
        {
            var dt = await Task.Run(() => _svc.GetDetalle(ordenServicioId));

            if (dt == null || dt.Rows.Count == 0)
            {
                _view.ShowWarning("No se encontró la orden seleccionada.");
                LimpiarPantallaOrden();
                return;
            }

            var r = dt.Rows[0];

            _codigoOrdenSeleccionada = S(r, "CodigoOrden");

            string cliente = S(r, "Cliente");
            string equipo = S(r, "Equipo");
            string diagnostico = S(r, "Diagnostico");
            string estadoNombre = S(r, "EstadoNombre");
            string correo = S(r, "CorreoCliente");
            int? estadoId = I(r, (int?)null, "EstadoID");

            _view.SetOrdenDetalle(
                _codigoOrdenSeleccionada,
                cliente,
                equipo,
                diagnostico,
                estadoNombre,
                correo,
                estadoId);

            if (string.IsNullOrWhiteSpace(_view.AsuntoNotificacion))
                _view.AsuntoNotificacion = "Actualización de orden " + _codigoOrdenSeleccionada;

            _view.SetActualizacionDiagnostico(DateTime.Now);
            _view.SetActualizacionNotificacion(DateTime.Now);
        }

        private static string S(DataRow row, params string[] cols)
        {
            foreach (var c in cols)
                if (row.Table.Columns.Contains(c) && row[c] != DBNull.Value)
                    return Convert.ToString(row[c]);

            return "";
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