using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Ordenes_de_Servicio.Recepcion
{
    public interface IOrdenesRecepcionView
    {
        int UsuarioId { get; }

        string BuscarTexto { get; }
        short EstadoFiltroValor { get; }
        int TecnicoSeleccionadoId { get; }
        int EquipoSeleccionadoId { get; }

        string ProblemaReportado { get; set; }
        string AccesoriosRecibidos { get; set; }

        void BindEstadosFiltro(DataTable dt);
        void BindTecnicos(DataTable dt);
        void BindEquiposCliente(DataTable dt);

        void RenderOrdenes(DataTable dt, int selectedOrderId);
        void SetResultados(int total);

        void SetCodigoOrden(string codigo);
        void SetClienteSeleccionado(int? clienteId, string clienteNombre);
        void ClearClienteSeleccionado();
        void ClearEquiposCliente();
        void SetEquipoSeleccionado(int equipoId);
        void SetTecnicoSeleccionado(int tecnicoId);

        void SetModoActualizar(bool actualizar);
        void SetPermisosAcciones(bool puedeGuardar, bool puedeAsignarTecnico);
        void SetVisibilidadNavegacion(bool verEquipos, bool verNotificacion);

        bool TrySeleccionarCliente(out int? clienteId, out string clienteNombre);
        void SetTecnicoHabilitado(bool enabled);
        void BindTecnicosNoDisponible();
        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);
        void CloseView();
    }
}