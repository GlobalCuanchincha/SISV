using System.Data;
using Datos_Acceso.Repositories.OrdenesServicio;

namespace Dominio_SISV.Services.OrdenesServicio
{
    public sealed class OrdenesRecepcionService : IOrdenesRecepcionService
    {
        private readonly OrdenesRecepcionRepository _repo = new OrdenesRecepcionRepository();

        public DataTable EstadosListar() => _repo.EstadosListar();
        public DataTable TecnicosListarActivos(int usuarioIdActor) => _repo.TecnicosListarActivos(usuarioIdActor);
        public DataTable EquiposListarPorCliente(int clienteId) => _repo.EquiposListarPorCliente(clienteId);

        public DataTable GenerarCodigoOrden() => _repo.GenerarCodigoOrden();
        public DataTable Buscar(string buscar, short estadoValor, int top) => _repo.Buscar(buscar, estadoValor, top);

        public DataTable GetById(int ordenServicioId) => _repo.GetById(ordenServicioId);

        public void SetTecnico(int usuarioIdActor, int ordenServicioId, int tecnicoId)
            => _repo.SetTecnico(usuarioIdActor, ordenServicioId, tecnicoId);

        public DataTable GuardarRecepcion(
            int usuarioIdActor,
            int? ordenServicioId,
            int clienteId,
            int equipoId,
            int? tecnicoId,
            string problemaReportado,
            string accesoriosRecibidos)
            => _repo.GuardarRecepcion(usuarioIdActor, ordenServicioId, clienteId, equipoId, tecnicoId, problemaReportado, accesoriosRecibidos);

        public DataTable ClienteFiltrosListar(int usuarioIdActor)
            => _repo.ClienteFiltrosListar(usuarioIdActor);

        public DataTable ClientesActivosBuscar(int usuarioIdActor, string filtroPor, string buscar, int top)
            => _repo.ClientesActivosBuscar(usuarioIdActor, filtroPor, buscar, top);
    }
}