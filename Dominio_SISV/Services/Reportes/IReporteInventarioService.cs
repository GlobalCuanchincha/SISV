using System.Data;
using Dominio_SISV.DTOs.Reportes;

namespace Dominio_SISV.Services.Reportes
{
    public interface IReporteInventarioService
    {
        DataTable ListarCategorias(int usuarioId);
        DataTable ListarProveedores(int usuarioId);
        DataTable BuscarReporte(FiltroReporteInventarioDto filtro);
    }
}