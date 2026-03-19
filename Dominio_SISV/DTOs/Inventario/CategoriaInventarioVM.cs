using System;

namespace Dominio_SISV.DTOs
{
    public sealed class CategoriaInventarioVM
    {
        public int CategoriaId { get; set; }
        public string Nombre { get; set; }

        public override string ToString() => Nombre ?? base.ToString();
    }
}
