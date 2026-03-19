using System;

namespace Dominio_SISV.DTOs
{
    public sealed class ProductoInventarioListItemVM
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string ProveedorNombre { get; set; }
        public string CategoriaNombre { get; set; }
        public int Stock { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Activo { get; set; }
    }
}