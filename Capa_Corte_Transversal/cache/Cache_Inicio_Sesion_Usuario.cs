using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Corte_Transversal.Cache
{
    public static class Cache_Inicio_Sesion_Usuario
    {
        public static int UserID {  get; set; }
        public static string UserName{ get; set; }
        public static string FirstName { get; set; }
        public static string LastName { get; set; }
        public static string Position { get; set; }
        public static string Email { get; set; }
    }
}
