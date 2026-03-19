using System.Data;

namespace Dominio_SISV.Services.OrdenesServicio
{
    public interface IOrdenesRecepcionService
    {
        DataTable EstadosListar();
        DataTable TecnicosListarActivos(int usuarioIdActor);
        DataTable EquiposListarPorCliente(int clienteId);

        DataTable GenerarCodigoOrden();
        DataTable Buscar(string buscar, short estadoValor, int top);

        DataTable GetById(int ordenServicioId);

        void SetTecnico(int usuarioIdActor, int ordenServicioId, int tecnicoId);

        DataTable GuardarRecepcion(
            int usuarioIdActor,
            int? ordenServicioId,
            int clienteId,
            int equipoId,
            int? tecnicoId,
            string problemaReportado,
            string accesoriosRecibidos
        );
        
        DataTable ClienteFiltrosListar(int usuarioIdActor);

        DataTable ClientesActivosBuscar(
            int usuarioIdActor,
            string filtroPor,
            string buscar,
            int top
        );
    }
}