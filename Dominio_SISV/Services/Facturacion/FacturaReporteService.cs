using System;
using System.Data;
using Datos_Acceso.Repositories.Facturacion;

namespace Dominio_SISV.Services.Facturacion
{
    public sealed class FacturaReporteService : IFacturaReporteService
    {
        private readonly FacturaReporteRepository _repo = new FacturaReporteRepository();

        public DataSet GetFacturaDataSet(int idFactura)
        {
            if (idFactura <= 0)
                throw new InvalidOperationException("IdFactura inválido.");

            DataTable dtEmpresa = _repo.GetEmpresa();
            DataTable dtCab = _repo.GetCabecera(idFactura);
            DataTable dtDet = _repo.GetDetalle(idFactura);

            if (dtCab == null || dtCab.Rows.Count == 0)
                throw new InvalidOperationException("No existe cabecera para la factura seleccionada.");

            // Importante: los nombres deben coincidir con los DataSet del RDLC
            dtEmpresa.TableName = "dsEmpresa";
            dtCab.TableName = "dsFacturaCabecera";
            dtDet.TableName = "dsFacturaDetalle";

            var ds = new DataSet("FacturaDS");
            ds.Tables.Add(dtEmpresa.Copy());
            ds.Tables.Add(dtCab.Copy());
            ds.Tables.Add(dtDet.Copy());

            return ds;
        }
    }
}