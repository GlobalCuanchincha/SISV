using Dominio_SISV.Services.Usuarios;
using System;
using System.Data;

namespace Union_Formularios_SISV.Logica_Presentacion.Administracion
{
    public interface IUsuariosView
    {
        int UsuarioSesionId { get; }

        string TextoBusqueda { get; }
        string FiltroTexto { get; }
        string RolFiltroTexto { get; }
        string EstadoFiltroTexto { get; }

        void EnsureFiltroCombo();
        void BindRoles(DataTable dtRoles);
        void BindEstados(DataTable dtFiltro, DataTable dtForm);

        void RenderUsuarios(DataTable dt, int? selectedUsuarioId);
        void ShowUsuarioDetalle(DataRow row);

        void SetFotoFromBytes(byte[] bytes);
        void SetDefaultFoto();
        void ClearPendingFoto();

        GuardarUsuarioRequest BuildGuardarRequest(int? usuarioTargetId);

        void ResetForm();
        void SetModeActualizar(bool actualizar);

        void SetEditingEnabled(bool enabled);
        void SetGuardarEnabled(bool enabled);
        void SetGestionarPermisosEnabled(bool enabled);

        void ShowInfo(string msg);
        void ShowWarning(string msg);
        void ShowError(string msg, Exception ex = null);

        void CloseView();
    }
}
