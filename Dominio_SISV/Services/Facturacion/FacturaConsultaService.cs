using System;
using System.Collections.Generic;
using System.Data;
using Datos_Acceso.Repositories.Facturacion;
using Dominio_SISV.DTOs.Facturacion;

namespace Dominio_SISV.Services.Facturacion
{
    public sealed class FacturaConsultaService : IFacturaConsultaService
    {
        private readonly FacturaConsultaRepository _repo = new FacturaConsultaRepository();

        public List<FacturaConsultaCardVM> Buscar(string texto, string estadoFiltro, int top = 200)
        {
            var dt = _repo.Buscar(texto, estadoFiltro, top);
            var list = new List<FacturaConsultaCardVM>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new FacturaConsultaCardVM
                {
                    FacturaID = ToInt(r, "FacturaID"),
                    CodigoFactura = ToStr(r, "CodigoFactura"),
                    Cliente = ToStr(r, "Cliente"),
                    FechaFactura = ToDate(r, "FechaFactura"),
                    Total = ToDec(r, "Total"),
                    EstadoTexto = ToStr(r, "EstadoTexto"),
                    IsAnulada = ToBool(r, "IsAnulada")
                });
            }

            return list;
        }

        public void Anular(int usuarioId, int facturaId, string motivo)
        {
            if (facturaId <= 0) throw new InvalidOperationException("Seleccione una factura.");
            if (string.IsNullOrWhiteSpace(motivo)) throw new InvalidOperationException("Ingrese el motivo de anulación.");
            _repo.Anular(usuarioId, facturaId, motivo);
        }

        public DataSet GetReporteDataSet(int facturaId)
        {
            var dtEmp = _repo.GetEmpresa();
            var dtCab = _repo.GetCabecera(facturaId);
            var dtDet = _repo.GetDetalle(facturaId);

            dtEmp.TableName = "dsEmpresa";
            dtCab.TableName = "dsFacturaCabecera";
            dtDet.TableName = "dsFacturaDetalle";

            var ds = new DataSet("FacturaDS");
            ds.Tables.Add(dtEmp.Copy());
            ds.Tables.Add(dtCab.Copy());
            ds.Tables.Add(dtDet.Copy());

            return ds;
        }

        private static string ToStr(DataRow r, string col) =>
            (r.Table.Columns.Contains(col) && r[col] != DBNull.Value) ? Convert.ToString(r[col]) ?? "" : "";

        private static int ToInt(DataRow r, string col)
        {
            int x;
            return (r.Table.Columns.Contains(col) && r[col] != DBNull.Value && int.TryParse(Convert.ToString(r[col]), out x)) ? x : 0;
        }

        private static decimal ToDec(DataRow r, string col)
        {
            decimal d;
            return (r.Table.Columns.Contains(col) && r[col] != DBNull.Value && decimal.TryParse(Convert.ToString(r[col]), out d)) ? d : 0m;
        }

        private static bool ToBool(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return false;
            bool b;
            if (bool.TryParse(Convert.ToString(r[col]), out b)) return b;
            int x;
            return int.TryParse(Convert.ToString(r[col]), out x) && x != 0;
        }

        private static DateTime? ToDate(DataRow r, string col)
        {
            if (!r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return null;
            DateTime d;
            return DateTime.TryParse(Convert.ToString(r[col]), out d) ? (DateTime?)d : null;
        }
    }
}