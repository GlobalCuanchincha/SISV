using System.Data;
using Datos_Acceso.Repositories.OrdenesServicio;

namespace Dominio_SISV.Services.OrdenesServicio
{
    public sealed class OrdenesEquipoService : IOrdenesEquipoService
    {
        private readonly OrdenesEquipoRepository _repo = new OrdenesEquipoRepository();

        public DataTable FiltrosListar(int usuarioId) => _repo.FiltrosListar(usuarioId);
        public DataTable TipoEquipoListar(int usuarioId) => _repo.TipoEquipoListar(usuarioId);
        public DataTable ConectividadListar(int usuarioId) => _repo.ConectividadListar(usuarioId);

        public DataTable Buscar(int usuarioId, int? clienteId, string filtroPor, string buscar, bool? soloActivos, int top)
            => _repo.Buscar(usuarioId, clienteId, filtroPor, buscar, soloActivos, top);

        public DataTable GetById(int usuarioId, int equipoId) => _repo.GetById(usuarioId, equipoId);

        public DataTable GenerarCodigoInterno(int usuarioId) => _repo.GenerarCodigoInterno(usuarioId);

        public DataTable Guardar(int usuarioId, int? equipoId, int clienteId, int tipoEquipoId, string codigoInterno,
            string marca, string modelo, string serie, string color, string conectividad, string accesorios, string observaciones, bool activo)
            => _repo.Guardar(usuarioId, equipoId, clienteId, tipoEquipoId, codigoInterno, marca, modelo, serie, color, conectividad, accesorios, observaciones, activo);
    }
}