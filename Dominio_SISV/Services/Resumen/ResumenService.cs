using System.Data;
using System.Threading.Tasks;
using Datos_Acceso.Repositories.Resumen;

namespace Dominio_SISV.Services.Resumen
{
    public sealed class ResumenService : IResumenService
    {
        private readonly ResumenRepository _repo = new ResumenRepository();

        public Task<DataTable> GetKpisAsync()
            => Task.Run(() => _repo.GetKpis());

        public Task<DataTable> GetIngresosUltimos7DiasAsync()
            => Task.Run(() => _repo.GetIngresosUltimos7Dias());

        public Task<DataTable> GetStockBajoAsync()
            => Task.Run(() => _repo.GetStockBajo());

        public Task<DataTable> GetActividadRecienteAsync(int top)
            => Task.Run(() => _repo.GetActividadReciente(top));
    }
}