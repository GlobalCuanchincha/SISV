using System.Data;

namespace Dominio_SISV.Services.Facturacion
{
    public interface IFacturaReporteService
    {
        DataSet GetFacturaDataSet(int idFactura);
    }
}