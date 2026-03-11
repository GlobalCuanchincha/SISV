using Dominio_SISV.DTOs.Reportes;
using System.Data;

namespace Dominio_SISV.Services.Reportes
{
    public interface IReporteClientesService
    {
        DataTable ListarOpcionesFiltrarPor(int usuarioId);
        DataTable BuscarReporte(FiltroReporteClientesDto filtro);
    }
}