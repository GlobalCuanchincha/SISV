using System;

namespace Dominio_SISV.DTOs
{
    public sealed class ProveedorDetalleVM
    {
        public int ProveedorId { get; set; }

        public string Nombre { get; set; }
        public string Ruc { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }

        public bool Activo { get; set; }
        public string EstadoTexto => Activo ? "Activo" : "Inactivo";

        public DateTime? UltimaActualizacion { get; set; }
    }
}