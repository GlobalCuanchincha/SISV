using System;

namespace Dominio_SISV.DTOs.Reportes
{
    public sealed class FiltroReporteClientesDto
    {
        public int UsuarioID { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string Estado { get; set; }
        public string ConFacturas { get; set; }
        public string FiltrarPor { get; set; }
        public string Texto { get; set; }
        public string Ordenar { get; set; }
    }
}