using System;

namespace Dominio_SISV.DTOs.Reportes
{
    public sealed class FiltroReporteServicioDto
    {
        public int UsuarioID { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }

        public string Texto { get; set; }

        public int? MetodoPagoID { get; set; }
        public int? EstadoID { get; set; }

        public string Contenido { get; set; }

        public int? ClienteID { get; set; }
        public int? TecnicoID { get; set; }
        public int? OrdenServicioID { get; set; }

        public decimal? TotalMin { get; set; }
        public decimal? TotalMax { get; set; }
    }
}