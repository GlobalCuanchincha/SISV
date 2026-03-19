using System.Data;

namespace Dominio_SISV.Services.OrdenesServicio
{
    public interface IOrdenesNotificacionService
    {
        DataTable EstadosListar();
        DataTable GetDetalle(int ordenServicioId);

        DataTable ListarParaSeleccion(string filtro, string busqueda);

        void GuardarDiagnostico(int usuarioId, int ordenServicioId, string diagnostico);
        void ActualizarEstado(int usuarioId, int ordenServicioId, int nuevoEstadoId);

        int RegistrarNotificacion(
            int usuarioId,
            int ordenServicioId,
            string correo,
            string asunto,
            string mensaje,
            string estadoEnvio,
            string errorDetalle
        );
    }
}