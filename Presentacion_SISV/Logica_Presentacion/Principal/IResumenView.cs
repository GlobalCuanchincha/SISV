using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion
{
    public interface IResumenView
    {
        void SetVentasHoy(decimal value);
        void SetTotalFacturasHoy(int value);
        void SetOrdenesPendientes(int value);
        void SetOrdenesHoy(int value);
        void SetStockBajo(int value);
        void SetClientesNuevos7(int value);
        void SetPromedioIngresos7(decimal value);

        void BindIngresos(DataTable dt);
        void BindStockBajo(DataTable dt);
        void BindActividadReciente(DataTable dt);

        void ShowWarning(string msg);
    }
}