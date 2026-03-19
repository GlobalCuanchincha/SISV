namespace Dominio_SISV.DTOs
{
    public class InventarioFiltroVM
    {
        public string Buscar { get; set; }
        public int? CategoriaId { get; set; }
        public string Estado { get; set; } = "Todos";
        public int Top { get; set; } = 200;
    }

    public class InventarioItemVM
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }

        public string CategoriaNombre { get; set; }
        public string ProveedorNombre { get; set; }

        public int Stock { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Activo { get; set; }
    }

    public class InventarioDetalleVM
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        public int? CategoriaId { get; set; }
        public string CategoriaNombre { get; set; }

        public int? ProveedorId { get; set; }
        public string ProveedorNombre { get; set; }

        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal? Costo { get; set; }
        public bool Activo { get; set; }
    }

    public class InventarioGuardarVM
    {
        public int? ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int CategoriaId { get; set; }
        public int? ProveedorId { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal? Costo { get; set; }
        public bool Activo { get; set; }
    }

    public class PermisosInventarioVM
    {
        public bool PuedeEntrar { get; set; }
        public bool PuedeGuardar { get; set; }
        public bool PuedeCambiarEstado { get; set; }
        public bool PuedeBuscarProveedor { get; set; }
    }

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