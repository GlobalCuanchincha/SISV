using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio_SISV.DTOs
{
    public sealed class CatalogItemVM
    {
        public int Id { get; set; }

        public string Codigo { get; set; }

        public string Nombre { get; set; }

        public string Tipo { get; set; }

        public decimal Precio { get; set; }

        public int? Stock { get; set; }

        public bool Activo { get; set; }

        public bool Disponible => (Tipo?.ToUpper() == "SERVICIO") || ((Stock ?? 0) > 0);

        public override string ToString()
            => $"{Nombre} ({Codigo})";
    }
}
