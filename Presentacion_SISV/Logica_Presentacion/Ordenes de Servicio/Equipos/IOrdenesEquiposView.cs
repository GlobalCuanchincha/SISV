using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Equipos
{
    public interface IOrdenesEquiposView
    {
        int UsuarioId { get; }

        string BuscarTexto { get; }
        string FiltroSeleccionado { get; }
        int TipoEquipoSeleccionadoId { get; }
        string ConectividadSeleccionada { get; }

        string CodigoInterno { get; set; }
        string ClienteNombre { get; set; }
        string Marca { get; set; }
        string Modelo { get; set; }
        string Serie { get; set; }
        string ColorEquipo { get; set; }
        string Accesorios { get; set; }
        string Observaciones { get; set; }

        void BindFiltros(DataTable dt);
        void BindTiposEquipo(DataTable dt);
        void BindConectividades(DataTable dt);

        void RenderEquipos(DataTable dt, int selectedEquipoId);
        void SetResultados(int total);

        void SetTipoEquipoSeleccionado(object value);
        void SetConectividadSeleccionada(object value);

        void SetModoActualizar(bool actualizar);
        void SetPermisosAcciones(bool puedeGuardar, bool puedeElegirCliente);
        void SetVisibilidadNavegacion(bool verRecepcion, bool verNotificacion);

        bool TrySeleccionarCliente(out int? clienteId, out string clienteNombre);

        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);
        void CloseView();
    }
}