using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Base_De_Datos_SISV.Repositories.Permisos;

namespace Dominio_SISV.Services.Permisos
{
    public class PermisosService : IPermisosService
    {
        private readonly PermisosRepository _repo = new PermisosRepository();

        public void SeedCatalogo() => _repo.SeedCatalogo();

        public DataTable ListarUsuariosBasico() => _repo.ListarUsuariosBasico();

        public HashSet<string> GetCodigosByRol(int roleId)
        {
            var dt = _repo.GetPermisosByRol(roleId);
            return DtToSet(dt);
        }

        public HashSet<string> GetCodigosByUsuario(int usuarioId)
        {
            var dt = _repo.GetPermisosByUsuario(usuarioId);
            return DtToSet(dt);
        }
        public HashSet<string> GetCodigosEfectivosByUsuario(int usuarioId)
        {
            var dt = _repo.GetPermisosEfectivosByUsuario(usuarioId);
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow r in dt.Rows)
            {
                var v = Convert.ToString(r[0]);
                if (!string.IsNullOrWhiteSpace(v)) set.Add(v.Trim());
            }
            return set;
        }
        public void SaveByRol(int actorUsuarioId, int roleIdTarget, IEnumerable<string> codigos)
            => _repo.SavePermisosByRol(actorUsuarioId, roleIdTarget, codigos);

        public void SaveByUsuario(int actorUsuarioId, int usuarioIdTarget, IEnumerable<string> codigos)
            => _repo.SavePermisosByUsuario(actorUsuarioId, usuarioIdTarget, codigos);

        private static HashSet<string> DtToSet(DataTable dt)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (dt == null || dt.Rows.Count == 0) return set;

            // esperamos 1 columna: Codigo
            foreach (DataRow r in dt.Rows)
            {
                var v = Convert.ToString(r[0]);
                if (!string.IsNullOrWhiteSpace(v)) set.Add(v.Trim());
            }
            return set;
        }
    }
}