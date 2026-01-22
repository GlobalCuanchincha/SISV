using System;

namespace Dominio_SISV.DTOs
{
    public sealed class ProductoCardVM
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Proveedor { get; set; }
        public int? ProveedorId { get; set; }
        public string Categoria { get; set; }
        public int? CategoriaId { get; set; }

        public int Stock { get; set; }
        public int StockMinimo { get; set; }

        public decimal Precio { get; set; }
        public decimal? Costo { get; set; }
        public string Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        public bool StockBajo => Stock <= StockMinimo;
    }
}
