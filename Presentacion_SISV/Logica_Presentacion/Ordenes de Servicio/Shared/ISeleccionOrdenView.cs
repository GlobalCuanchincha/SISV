using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Shared
{
    public interface ISeleccionOrdenView
    {
        int UsuarioId { get; }

        string TextoBusqueda { get; }
        string FiltroSeleccionado { get; }

        void BindFiltros(DataTable dt);
        void RenderOrdenes(DataTable dt, int? selectedOrdenId);
        void SetResultados(int total);

        void SetOrdenSeleccionada(int ordenServicioId);
        void CloseWithOk();
        void CloseView();

        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);
    }
}