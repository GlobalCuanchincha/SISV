using System;
using System.Collections.Generic;

namespace Capa_Corte_Transversal.Loggin
{
    public static class Session
    {
        public static int UsuarioId { get; set; }
        public static string NombreUsuario { get; set; }
        public static string Cargo { get; set; }
        public static string Rol { get; set; }

        public static HashSet<string> Permisos { get; set; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}