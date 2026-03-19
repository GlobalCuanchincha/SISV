using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Servicio
{
    public interface IServicioView
    {
        int UsuarioId { get; }

        string TextoBusqueda { get; }
        int? CategoriaFiltroId { get; }
        string EstadoFiltroTexto { get; }

        string CodigoServicio { get; set; }
        string NombreServicio { get; set; }
        int CategoriaServicioId { get; }
        decimal PrecioServicio { get; set; }
        bool ActivoServicio { get; set; }

        bool PuedeAcceder { get; }
        bool PuedeRegistrar { get; }
        bool PuedeActualizar { get; }
        bool PuedeDesactivar { get; }

        void BindCategorias(DataTable dtCategorias);
        void RenderServicios(DataTable dt, int? selectedServicioId);
        void SetResultados(int total);

        void SetModoActualizar(bool actualizar);
        void SetGuardarEnabled(bool enabled);
        void SetDesactivarEnabled(bool enabled);
        void SetCodigoLabel(string text);

        void ClearFormInputs();
        void FocusNombre();

        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);
        void CloseView();
    }
}