using System;
using System.Collections.Generic;
using Dominio_SISV.DTOs.Clientes;

namespace Union_Formularios_SISV.Logica_Presentacion.Clientes
{
    public interface IClientesView
    {
        // filtros (lectura)
        string BuscarTexto { get; }
        string FiltroPorKey { get; }
        int? EstadoFiltroKey { get; }

        // acciones sobre UI
        void BindFiltroPor(List<KeyValuePair<string, string>> opciones, string defaultKey);
        void BindEstados(List<ClienteEstadoVM> estados, int? defaultEstadoKey);
        void BindEstadosFiltro(List<ClienteEstadoVM> estadosFiltro);

        void ShowClientes(List<ClienteCardVM> clientes, string selectedCedula);
        void SetResultados(int count);

        ClienteDetalleVM ReadForm();
        void ShowDetalle(ClienteDetalleVM det);
        void ClearSelectionAndForm();
        void SetCedulaReadOnly(bool readOnly);
        void SetActualizarEnabled(bool enabled);
        void SetSelectedLabel(string text);

        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string title, Exception ex);

        // opcional: setear criterios (para “buscar por cédula” luego de registrar)
        void SetBusqueda(string filtroKey, string texto);
    }
}