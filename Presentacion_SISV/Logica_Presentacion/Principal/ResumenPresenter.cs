using Dominio_SISV.Services.Resumen;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Union_Formularios_SISV.Logica_Presentacion
{
    public sealed class ResumenPresenter
    {
        private readonly IResumenView _view;
        private readonly IResumenService _service;

        public ResumenPresenter(IResumenView view, IResumenService service = null)
        {
            _view = view;
            _service = service ?? new ResumenService();
        }

        public async Task CargarAsync()
        {
            try
            {
                var dtKpis = await _service.GetKpisAsync();
                var dtIngresos = await _service.GetIngresosUltimos7DiasAsync();
                var dtStock = await _service.GetStockBajoAsync();
                var dtActividad = await _service.GetActividadRecienteAsync(50);

                AplicarKpis(dtKpis);
                _view.BindIngresos(dtIngresos);
                _view.BindStockBajo(dtStock);
                _view.BindActividadReciente(dtActividad);
            }
            catch (Exception ex)
            {
                _view.ShowWarning("No se pudo cargar el resumen.\n\n" + ex.Message);
            }
        }

        private void AplicarKpis(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                _view.SetVentasHoy(0);
                _view.SetTotalFacturasHoy(0);
                _view.SetOrdenesPendientes(0);
                _view.SetOrdenesHoy(0);
                _view.SetStockBajo(0);
                _view.SetClientesNuevos7(0);
                _view.SetPromedioIngresos7(0);
                return;
            }

            var r = dt.Rows[0];

            _view.SetVentasHoy(D(r, "VentasHoy"));
            _view.SetTotalFacturasHoy(I(r, "TotalFacturasHoy"));
            _view.SetOrdenesPendientes(I(r, "OrdenesPendientes"));
            _view.SetOrdenesHoy(I(r, "OrdenesHoy"));
            _view.SetStockBajo(I(r, "StockBajo"));
            _view.SetClientesNuevos7(I(r, "ClientesNuevos7"));
            _view.SetPromedioIngresos7(D(r, "PromedioIngresos7"));
        }

        private static int I(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col) || row[col] == DBNull.Value)
                return 0;

            return Convert.ToInt32(row[col]);
        }

        private static decimal D(DataRow row, string col)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(col) || row[col] == DBNull.Value)
                return 0m;

            return Convert.ToDecimal(row[col]);
        }
    }
}