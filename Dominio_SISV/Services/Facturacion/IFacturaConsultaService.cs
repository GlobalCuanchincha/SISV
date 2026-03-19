using System.Collections.Generic;
using System.Data;
using Dominio_SISV.DTOs.Facturacion;

namespace Dominio_SISV.Services.Facturacion
{
    public interface IFacturaConsultaService
    {
        List<FacturaConsultaCardVM> Buscar(string texto, string estadoFiltro, int top = 200);
        void Anular(int usuarioId, int facturaId, string motivo);
        DataSet GetReporteDataSet(int facturaId);
    }
}