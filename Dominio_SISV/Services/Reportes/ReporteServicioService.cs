using Datos_Acceso.Repositories.Reportes;
using Dominio_SISV.DTOs.Reportes;
using System;
using System.Data;

namespace Dominio_SISV.Services.Reportes
{
    public sealed class ReporteServicioService : IReporteServicioService
    {
        private readonly ReporteServicioRepository _repo = new ReporteServicioRepository();

        public DataTable ListarMetodosPago(int usuarioId) => _repo.ListarMetodosPago(usuarioId);
        public DataTable ListarEstadosFactura(int usuarioId) => _repo.ListarEstadosFactura(usuarioId);
        public DataTable ListarContenido() => _repo.ListarContenido();
        public DataTable BuscarClientes(int usuarioId, string buscar) => _repo.BuscarClientes(usuarioId, buscar);
        public DataTable BuscarTecnicos(int usuarioId, string buscar) => _repo.BuscarTecnicos(usuarioId, buscar);
        public DataTable BuscarOrdenes(int usuarioId, string buscar, int? tecnicoId) => _repo.BuscarOrdenes(usuarioId, buscar, tecnicoId);

        public DataTable BuscarReporte(FiltroReporteServicioDto filtro)
        {
            if (filtro == null) throw new ArgumentNullException(nameof(filtro));

            return _repo.BuscarReporte(
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.Texto,
                filtro.MetodoPagoID,
                filtro.EstadoID,
                filtro.Contenido,
                filtro.ClienteID,
                filtro.TecnicoID,
                filtro.OrdenServicioID,
                filtro.TotalMin,
                filtro.TotalMax
            );
        }
    }
}