using System;

namespace Dominio_SISV.DTOs.Facturacion
{
    public sealed class FacturaConsultaCardVM
    {
        public int FacturaID { get; set; }
        public string CodigoFactura { get; set; }
        public string Cliente { get; set; }
        public DateTime? FechaFactura { get; set; }
        public decimal Total { get; set; }
        public string EstadoTexto { get; set; }
        public bool IsAnulada { get; set; }
    }
}