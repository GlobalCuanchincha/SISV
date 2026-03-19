using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Shared
{
    public interface ISeleccionClienteView
    {
        int UsuarioId { get; }

        string TextoBusqueda { get; }
        string FiltroSeleccionado { get; }

        void BindFiltros(DataTable dt);
        void RenderClientes(DataTable dt, int? selectedClienteId);
        void SetResultados(int total);

        void SetClienteSeleccionado(int clienteId, string nombreCompleto);
        void CloseWithOk();
        void CloseView();

        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);
    }
}