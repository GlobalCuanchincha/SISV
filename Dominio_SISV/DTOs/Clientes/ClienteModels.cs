using System;

namespace Dominio_SISV.DTOs.Clientes
{
    public sealed class ClienteCardVM
    {
        public int ClienteID { get; set; }
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

    public sealed class ClienteEstadoVM
    {
        public int? EstadoKey { get; set; }
        public string EstadoNombre { get; set; }
        public bool? EsActivo { get; set; }
    }
}