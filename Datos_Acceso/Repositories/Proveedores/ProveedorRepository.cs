using Datos_Acceso.Common;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Proveedores
{
    public sealed class ProveedorRepository
    {
        public DataTable Buscar(int usuarioId, string texto, string filtro, string estadoTexto, int top)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Proveedor_Buscar",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto),
                new SqlParameter("@Filtro", string.IsNullOrWhiteSpace(filtro) ? (object)DBNull.Value : filtro),
                new SqlParameter("@EstadoTexto", string.IsNullOrWhiteSpace(estadoTexto) ? (object)DBNull.Value : estadoTexto),
                new SqlParameter("@Top", top)
            );
        }

        public DataTable GetById(int usuarioId, int proveedorId)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Proveedor_GetById",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@ProveedorID", proveedorId)
            );
        }

        public DataTable Upsert(int usuarioId, int? proveedorId, string nombre, string ruc, string telefono, string correo, string direccion, string estadoTexto)
        {
            return SqlExecutor.ExecuteDataTable(
                "inv.usp_Proveedor_Upsert",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@ProveedorID", proveedorId.HasValue ? (object)proveedorId.Value : DBNull.Value),
                new SqlParameter("@Nombre", nombre ?? ""),
                new SqlParameter("@RUC", ruc ?? ""),
                new SqlParameter("@Telefono", string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono),
                new SqlParameter("@Correo", string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo),
                new SqlParameter("@Direccion", string.IsNullOrWhiteSpace(direccion) ? (object)DBNull.Value : direccion),
                new SqlParameter("@EstadoTexto", string.IsNullOrWhiteSpace(estadoTexto) ? (object)DBNull.Value : estadoTexto)
            );
        }
    }
}