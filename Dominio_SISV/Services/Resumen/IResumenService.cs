using System.Data;
using System.Threading.Tasks;

namespace Dominio_SISV.Services.Resumen
{
    public interface IResumenService
    {
        Task<DataTable> GetKpisAsync();
        Task<DataTable> GetIngresosUltimos7DiasAsync();
        Task<DataTable> GetStockBajoAsync();
        Task<DataTable> GetActividadRecienteAsync(int top);
    }
}