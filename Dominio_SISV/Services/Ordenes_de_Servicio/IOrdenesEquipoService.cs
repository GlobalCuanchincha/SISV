using System.Data;

namespace Dominio_SISV.Services.OrdenesServicio
{
    public interface IOrdenesEquipoService
    {
        DataTable FiltrosListar(int usuarioId);
        DataTable TipoEquipoListar(int usuarioId);
        DataTable ConectividadListar(int usuarioId);

        DataTable Buscar(int usuarioId, int? clienteId, string filtroPor, string buscar, bool? soloActivos, int top);

        DataTable GetById(int usuarioId, int equipoId);

        DataTable GenerarCodigoInterno(int usuarioId);

        DataTable Guardar(
            int usuarioId,
            int? equipoId,
            int clienteId,
            int tipoEquipoId,
            string codigoInterno,
            string marca,
            string modelo,
            string serie,
            string color,
            string conectividad,
            string accesorios,
            string observaciones,
            bool activo
        );
    }
}