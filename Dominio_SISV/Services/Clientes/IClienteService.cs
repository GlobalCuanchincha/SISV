using System.Collections.Generic;
using Dominio_SISV.DTOs.Clientes;

namespace Dominio_SISV.Services.Clientes
{
    public interface IClienteService
    {
        List<ClienteEstadoVM> ListarEstados();
        List<ClienteCardVM> Buscar(string filtroPor, string buscar, int? estadoKey, int top);

        ClienteDetalleVM GetByCedula(string cedula);
        ClienteDetalleVM Crear(ClienteDetalleVM input);
        ClienteDetalleVM Actualizar(string cedula, ClienteDetalleVM input);
    }
}