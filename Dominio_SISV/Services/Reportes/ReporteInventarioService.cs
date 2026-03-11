using Datos_Acceso.Repositories.Reportes;
using Dominio_SISV.DTOs.Reportes;
using System;
using System.Data;

namespace Dominio_SISV.Services.Reportes
{
    public sealed class ReporteInventarioService : IReporteInventarioService
    {
        private readonly ReporteInventarioRepository _repo = new ReporteInventarioRepository();

        public DataTable ListarCategorias(int usuarioId) => _repo.ListarCategorias(usuarioId);
        public DataTable ListarProveedores(int usuarioId) => _repo.ListarProveedores(usuarioId);

        public DataTable BuscarReporte(FiltroReporteInventarioDto filtro)
        {
            if (filtro == null) throw new ArgumentNullException("filtro");

            return _repo.BuscarInventario(
                filtro.UsuarioID,
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.FiltrarFecha,
                filtro.Texto,
                filtro.SKU,
                filtro.CategoriaID,
                filtro.ProveedorID,
                filtro.Nombre,
                filtro.StockFiltro,
                filtro.Estado,
                filtro.CostoMin,
                filtro.CostoMax,
                filtro.PrecioMin,
                filtro.PrecioMax,
                filtro.Ordenar
            );
        }
    }
}