using System.Data;

namespace Dominio_SISV.Services.Servicios
{
    public interface IServicioService
    {
        DataTable ListarCategorias(int usuarioId);

        DataTable Buscar(int usuarioId, string texto, int? categoriaServicioId, string estado);

        (int ServicioID, string Codigo) Guardar(
            int usuarioId,
            int? servicioId,
            string codigo,
            int categoriaServicioId,
            string nombre,
            decimal precio,
            bool activo
        );

        void Desactivar(int usuarioId, int servicioId);
        string GetNextCodigo(int usuarioId);
    }
}