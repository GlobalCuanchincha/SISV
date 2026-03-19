namespace Dominio_SISV.DTOs.Servicios
{
    public sealed class ServicioDto
    {
        public int ServicioID { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
    }
}