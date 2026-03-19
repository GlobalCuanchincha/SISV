using System;
using System.Collections.Generic;
using Dominio_SISV.DTOs;

namespace Union_Formularios_SISV.Logica_Presentacion.Proveedores
{
    public interface IProveedoresView
    {
        int UsuarioId { get; }

        bool PuedeAcceder { get; }
        bool PuedeRegistrar { get; }
        bool PuedeActualizar { get; }

        string TextoBusqueda { get; }
        string FiltroTexto { get; }
        string EstadoFiltroTexto { get; }

        ProveedorDetalleVM ReadForm();
        void ShowDetalle(ProveedorDetalleVM det);
        void ClearForm();

        void SetSelectedLabel(string text);
        void SetResultados(int total);
        void SetModoActualizar(bool actualizar);
        void SetGuardarHabilitado(bool enabled);
        void RenderCards(List<ProveedorDetalleVM> proveedores, int? selectedId);

        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex);
    }
}