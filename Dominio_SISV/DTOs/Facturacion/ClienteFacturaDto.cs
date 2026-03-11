using System;

namespace Dominio_SISV.DTOs.Facturacion
{
    public sealed class ClienteFacturaDto
    {
        public int ClienteID { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Direccion { get; set; }
        public string Email { get; set; }
    }
}