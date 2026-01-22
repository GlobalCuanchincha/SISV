namespace Dominio_SISV.DTOs
{
    public sealed class ProveedorVM
    {
        public int ProveedorId { get; set; }
        public string Nombre { get; set; }
        public string Ruc { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public bool Activo { get; set; } = true;

        public override string ToString() => Nombre ?? base.ToString();
    }
}
