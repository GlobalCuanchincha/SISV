using System;

namespace Dominio_SISV.DTOs.Reportes
{
    public sealed class FiltroReporteInventarioDto
    {
        public int UsuarioID { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string FiltrarFecha { get; set; }  // todos|creacion|actualizacion|ingreso

        public string Texto { get; set; }
        public string SKU { get; set; }
        public int? CategoriaID { get; set; }
        public int? ProveedorID { get; set; }
        public string Nombre { get; set; }

        public string StockFiltro { get; set; } // todos|constock|sinstock|critico
        public string Estado { get; set; }      // todos|activos|inactivos

        public decimal? CostoMin { get; set; }
        public decimal? CostoMax { get; set; }
        public decimal? PrecioMin { get; set; }
        public decimal? PrecioMax { get; set; }

        public string Ordenar { get; set; } // nombre|stock|costo|precio|valor|fecha|categoria|proveedor
    }
}