using System.Collections.Generic;

namespace Dominio_SISV.DTOs.Facturacion
{
    public sealed class CrearFacturaRequestDto
    {
        public int UsuarioID { get; set; }   

        public int ClienteID { get; set; }
        public string NumeroFactura { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public int TipoPagoID { get; set; }
        public List<FacturaItemVM> Items { get; set; } = new List<FacturaItemVM>();
    }
}