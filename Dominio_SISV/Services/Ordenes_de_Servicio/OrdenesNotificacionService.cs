using System.Data;
using Datos_Acceso.Repositories.OrdenesServicio;

namespace Dominio_SISV.Services.OrdenesServicio
{
    public sealed class OrdenesNotificacionService : IOrdenesNotificacionService
    {
        private readonly OrdenesNotificacionRepository _repo = new OrdenesNotificacionRepository();

        public DataTable EstadosListar() => _repo.EstadosListar();
        public DataTable GetDetalle(int ordenServicioId) => _repo.GetDetalle(ordenServicioId);

        public DataTable ListarParaSeleccion(string filtro, string busqueda)
            => _repo.ListarParaSeleccion(filtro, busqueda);

        public void GuardarDiagnostico(int usuarioId, int ordenServicioId, string diagnostico)
            => _repo.GuardarDiagnostico(usuarioId, ordenServicioId, diagnostico);

        public void ActualizarEstado(int usuarioId, int ordenServicioId, int nuevoEstadoId)
            => _repo.ActualizarEstado(usuarioId, ordenServicioId, nuevoEstadoId);

        public int RegistrarNotificacion(
            int usuarioId,
            int ordenServicioId,
            string correo,
            string asunto,
            string mensaje,
            string estadoEnvio,
            string errorDetalle)
            => _repo.RegistrarNotificacion(usuarioId, ordenServicioId, correo, asunto, mensaje, estadoEnvio, errorDetalle);
    }
}