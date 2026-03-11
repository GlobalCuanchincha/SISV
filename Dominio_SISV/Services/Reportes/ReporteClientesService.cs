using Datos_Acceso.Repositories.Reportes;
using Dominio_SISV.DTOs.Reportes;
using System;
using System.Data;

namespace Dominio_SISV.Services.Reportes
{
    public sealed class ReporteClientesService : IReporteClientesService
    {
        private readonly ReporteClientesRepository _repo = new ReporteClientesRepository();

        public DataTable ListarOpcionesFiltrarPor(int usuarioId)
        {
            return _repo.ListarOpcionesFiltrarPor(usuarioId);
        }

        public DataTable BuscarReporte(FiltroReporteClientesDto filtro)
        {
            if (filtro == null) throw new ArgumentNullException("filtro");

            return _repo.BuscarReporteClientes(
                filtro.UsuarioID,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.Estado,
                filtro.ConFacturas,
                filtro.FiltrarPor,
                filtro.Texto,
                filtro.Ordenar
            );
        }
    }
}