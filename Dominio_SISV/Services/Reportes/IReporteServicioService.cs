using Dominio_SISV.DTOs.Reportes;
using System.Data;

namespace Dominio_SISV.Services.Reportes
{
    public interface IReporteServicioService
    {
        DataTable ListarMetodosPago(int usuarioId);
        DataTable ListarEstadosFactura(int usuarioId);
        DataTable ListarContenido();
        DataTable BuscarReporte(FiltroReporteServicioDto filtro);
        DataTable BuscarClientes(int usuarioId, string buscar);
        DataTable BuscarTecnicos(int usuarioId, string buscar);
        DataTable BuscarOrdenes(int usuarioId, string buscar, int? tecnicoId);
    }
}