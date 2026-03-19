using System.Collections.Generic;
using System.Data;

namespace Dominio_SISV.Services.Permisos
{
    public interface IPermisosService
    {
        void SeedCatalogo();

        DataTable ListarUsuariosBasico();

        HashSet<string> GetCodigosByRol(int roleId);
        HashSet<string> GetCodigosByUsuario(int usuarioId);

        void SaveByRol(int actorUsuarioId, int roleIdTarget, IEnumerable<string> codigos);
        void SaveByUsuario(int actorUsuarioId, int usuarioIdTarget, IEnumerable<string> codigos);
    }
}