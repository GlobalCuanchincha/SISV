using System;

namespace Dominio_SISV.DTOs
{
   
    public sealed class ProveedorPickVM
    {
        public int ProveedorId { get; set; }
        public string NombreProveedor { get; set; }
        public string Ruc { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public bool Activo { get; set; }

        public override string ToString() => NombreProveedor ?? base.ToString();
    }
}
