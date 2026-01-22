using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Union_Formularios_SISV.Forms.Clientes
{
    public sealed class ClienteCardVM
    {
        public string Cedula { get; set; }
        public string Cliente { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }

        public int? EstadoKey { get; set; }
        public string EstadoNombre { get; set; }
        public bool? EsActivo { get; set; }

        public int TotalCoincidencias { get; set; }
    }

    public sealed class ClienteDetalleVM
    {
        public string Cedula { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Cliente { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }

        public int? EstadoKey { get; set; }
        public string EstadoNombre { get; set; }
        public bool? EsActivo { get; set; }
    }

    public sealed class ClienteCardSelectedEventArgs : EventArgs
    {
        public ClienteCardSelectedEventArgs(ClienteCardVM cliente) => Cliente = cliente;
        public ClienteCardVM Cliente { get; }
    }
}
