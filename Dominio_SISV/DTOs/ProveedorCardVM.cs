using System;

namespace Dominio_SISV.DTOs
{
    public sealed class ProveedorCardVM
    {
        public int ProveedorId { get; set; }

        public string Ruc { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }

        public bool Activo { get; set; }

        // Útil para UI (chips "Activo/Inactivo")
        public string EstadoTexto => Activo ? "Activo" : "Inactivo";

        // Mostrar en el panel derecho (txt_UltimaAct_Proveedor)
        public DateTime? UltimaActualizacion { get; set; }

        public override string ToString()
            => $"{Nombre} ({Ruc}) - {EstadoTexto}";
    }
}