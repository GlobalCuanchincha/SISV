using System;
using Dominio_SISV.DTOs.Clientes;

namespace Union_Formularios_SISV.Forms.Clientes
{
    public sealed class ClienteCardSelectedEventArgs : EventArgs
    {
        public ClienteCardSelectedEventArgs(ClienteCardVM cliente)
        {
            Cliente = cliente;
        }

        public ClienteCardVM Cliente { get; private set; }
    }
}