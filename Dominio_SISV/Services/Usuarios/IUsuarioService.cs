using System.Data;
using System.Threading.Tasks;

namespace Dominio_SISV.Services.Usuarios
{
    public interface IUsuarioService
    {
        Task<byte> GetRolSesionAsync(int usuarioSesionId);

        Task<DataTable> ListarRolesAsync(bool soloActivos);
        Task<DataTable> ListarEstadosAsync(string modo);

        Task<DataTable> ListarRecientesAsync(int usuarioSesionId, int top);
        Task<DataTable> BuscarAsync(int usuarioSesionId, string texto, string filtroKey, string rolFiltro, string estadoFiltro, int top);

        Task<DataTable> GetByIdAsync(int usuarioSesionId, int usuarioTargetId);

        Task<int> GuardarAsync(GuardarUsuarioRequest req);

        Task<byte[]> GetFotoAsync(int usuarioSesionId, int usuarioTargetId);
    }
}