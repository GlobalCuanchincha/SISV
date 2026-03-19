using System.Collections.Generic;
using Dominio_SISV.DTOs;

namespace Dominio_SISV.Services.Proveedores
{
    public interface IProveedorService
    {
        List<ProveedorDetalleVM> Buscar(int usuarioId, string texto, string filtro, string estadoTexto, int top);
        ProveedorDetalleVM GetById(int usuarioId, int proveedorId);
        ProveedorDetalleVM Guardar(int usuarioId, int? proveedorId, ProveedorDetalleVM input);
    }
}