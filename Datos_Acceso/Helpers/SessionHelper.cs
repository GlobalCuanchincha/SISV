using System;
using System.Linq;
using System.Reflection;

namespace Capa_Corte_Transversal.Helpers
{
    public static class SessionHelper
    {
        public static int GetUsuarioID(object session)
        {
            if (session == null) return 0;

            var t = session.GetType();

            string[] candidatos =
            {
                "UsuarioID", "UsuarioId",
                "UserId", "UserID",
                "IdUsuario", "IDUsuario",
                "UsuarioID_Usuarios", "UsuarioID_Usuario",
                "Id"
            };

            var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var name in candidatos)
            {
                var p = props.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (p != null)
                {
                    var v = p.GetValue(session);
                    if (v != null) return Convert.ToInt32(v);
                }
            }

            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var name in candidatos)
            {
                var f = fields.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (f != null)
                {
                    var v = f.GetValue(session);
                    if (v != null) return Convert.ToInt32(v);
                }
            }

            return 0;
        }
    }
}
