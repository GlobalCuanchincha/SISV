using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Notificacion
{
    public interface IOrdenesNotificacionView
    {
        int UsuarioId { get; }

        string Diagnostico { get; set; }
        string CorreoNotificacion { get; set; }
        string AsuntoNotificacion { get; set; }
        string MensajeNotificacion { get; set; }

        int EstadoSeleccionadoId { get; }
        bool TieneEstadoSeleccionado { get; }

        void BindEstados(DataTable dt);

        bool TrySeleccionarOrden(out int ordenServicioId);

        void SetOrdenDetalle(
            string codigoOrden,
            string cliente,
            string equipo,
            string diagnostico,
            string estadoNombre,
            string correoCliente,
            int? estadoId);

        void ClearOrden();

        void SetPermisosAcciones(bool puedeGuardarDiag, bool puedeCambiarEstado, bool puedeEnviarCorreo);
        void SetVisibilidadNavegacion(bool verRecepcion, bool verEquipos);

        void SetActualizacionDiagnostico(DateTime dt);
        void SetActualizacionNotificacion(DateTime dt);

        void MostrarPrevisualizacion(string correo, string asunto, string mensaje);

        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);

        void CloseView();
    }
}