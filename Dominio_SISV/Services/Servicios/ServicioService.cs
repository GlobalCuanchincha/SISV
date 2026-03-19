using System;
using System.Data;
using Datos_Acceso.Repositories.Servicios;

namespace Dominio_SISV.Services.Servicios
{
    public sealed class ServicioService : IServicioService
    {
        private readonly ServicioRepository _repo = new ServicioRepository();

        public DataTable ListarCategorias(int usuarioId) => _repo.ListarCategorias(usuarioId);
        public DataTable Buscar(int usuarioId, string texto, int? categoriaServicioId, string estado)
            => _repo.Buscar(usuarioId, texto, categoriaServicioId, estado);

        public (int ServicioID, string Codigo) Guardar(int usuarioId, int? servicioId, string codigo,
            int categoriaServicioId, string nombre, decimal precio, bool activo)
        {
            if (categoriaServicioId <= 0) throw new InvalidOperationException("Selecciona una categoría.");
            if (string.IsNullOrWhiteSpace(nombre)) throw new InvalidOperationException("El nombre del servicio es obligatorio.");
            if (precio < 0) throw new InvalidOperationException("El precio no puede ser negativo.");

            return _repo.Upsert(usuarioId, servicioId, codigo, categoriaServicioId, nombre, precio, activo);
        }

        public void Desactivar(int usuarioId, int servicioId) => _repo.SetActivo(usuarioId, servicioId, false);

        public string GetNextCodigo(int usuarioId) => _repo.GetNextCodigo(usuarioId);
    }
}