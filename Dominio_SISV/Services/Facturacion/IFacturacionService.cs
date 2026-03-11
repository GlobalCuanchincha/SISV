using System.Collections.Generic;
using Dominio_SISV.DTOs;
using Dominio_SISV.DTOs.Facturacion;

namespace Dominio_SISV.Services.Facturacion
{
    public interface IFacturacionService
    {
        ClienteFacturaDto BuscarClientePorCedula(string cedula);
        List<CatalogItemVM> ObtenerCatalogo();
        List<TipoPagoDto> ListarTiposPago();
        string ObtenerSiguienteCodigoFactura();
        CrearFacturaResultDto CrearFactura(CrearFacturaRequestDto request);
    }
}