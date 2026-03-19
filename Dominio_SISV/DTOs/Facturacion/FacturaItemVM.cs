using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio_SISV.DTOs
{
    public sealed class FacturaItemVM
    {
        public int Id { get; set; }                 
        public bool EsProducto { get; set; }        
        public string Codigo { get; set; }
        public string Nombre { get; set; }

        public decimal PrecioUnitario { get; set; }

        public int? Stock { get; set; }

        // Cantidad (mín 1)
        public int Cantidad { get; set; } = 1;

        public string TipoTexto
        {
            get { return EsProducto ? "PRODUCTO" : "SERVICIO"; }
        }

        public decimal Subtotal
        {
            get { return Math.Round(PrecioUnitario * Cantidad, 2, MidpointRounding.AwayFromZero); }
        }

        public string Key
        {
            get { return (EsProducto ? "P:" : "S:") + Id.ToString(); }
        }

        public int? ItemInventarioID
        {
            get { return EsProducto ? (int?)Id : null; }
        }

        public int? ServicioID
        {
            get { return EsProducto ? null : (int?)Id; }
        }

        public static FacturaItemVM FromCatalog(CatalogItemVM c)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            bool esServicio = string.Equals(c.Tipo ?? "", "SERVICIO", StringComparison.OrdinalIgnoreCase);

            return new FacturaItemVM
            {
                Id = c.Id,
                EsProducto = !esServicio,
                Codigo = c.Codigo,
                Nombre = c.Nombre,
                PrecioUnitario = c.Precio,
                Stock = esServicio ? (int?)null : c.Stock,
                Cantidad = 1
            };
        }
    }
}

