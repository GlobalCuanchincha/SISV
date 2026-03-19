using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Inventario
{
    public interface IInventarioView
    {
        int UsuarioId { get; }

        // Filtros
        string TextoBusqueda { get; }
        int? CategoriaFiltroId { get; }
        string EstadoFiltroTexto { get; }

        // Formulario
        string CodigoProducto { get; set; }
        string NombreProducto { get; set; }
        string DescripcionProducto { get; set; }

        int? CategoriaProductoId { get; set; }
        int? ProveedorIdSeleccionado { get; set; }
        string ProveedorNombreSeleccionado { get; set; }

        decimal StockProducto { get; set; }
        decimal StockMinimoProducto { get; set; }
        decimal PrecioProducto { get; set; }
        decimal CostoProducto { get; set; }
        bool ActivoProducto { get; set; }

        // UI
        void BindCategorias(DataTable dtCategorias);
        void RenderCards(DataTable dtProductos, int? selectedId);
        void ClearCardSelection();
        void SetResultados(int total);
        void SetModoActualizar(bool actualizar);
        void SetTextoBotonToggle(string text);
        void SetAccionesHabilitadas(bool guardar, bool toggleActivo, bool elegirProveedor);

        // Selección de proveedor
        bool TryElegirProveedor(int usuarioId, out int? proveedorId, out string proveedorNombre);

        // Mensajes
        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex);

        void CloseView();
    }
}