using Datos_Acceso.Common;
using Datos_Acceso.Connection;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Servicios
{
    public sealed class ServicioRepository
    {
        public DataTable ListarCategorias(int usuarioId)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_Servicio_Categorias_Listar",
                new SqlParameter("@UsuarioID", usuarioId)
            );
        }

        public DataTable Buscar(int usuarioId, string texto, int? categoriaServicioId, string estado)
        {
            return SqlExecutor.ExecuteDataTable(
                "ops.usp_Servicio_Buscar",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@Texto", string.IsNullOrWhiteSpace(texto) ? (object)DBNull.Value : texto),
                new SqlParameter("@CategoriaServicioID", categoriaServicioId.HasValue ? (object)categoriaServicioId.Value : DBNull.Value),
                new SqlParameter("@Estado", string.IsNullOrWhiteSpace(estado) ? "todos" : estado)
            );
        }

        public (int ServicioID, string Codigo) Upsert(
            int usuarioId,
            int? servicioId,
            string codigo,
            int categoriaServicioId,
            string nombre,
            decimal precio,
            bool activo
        )
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand("ops.usp_Servicio_Upsert", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@UsuarioID", SqlDbType.Int).Value = usuarioId;
                cmd.Parameters.Add("@ServicioID", SqlDbType.Int).Value = servicioId.HasValue ? (object)servicioId.Value : DBNull.Value;
                cmd.Parameters.Add("@Codigo", SqlDbType.NVarChar, 30).Value = string.IsNullOrWhiteSpace(codigo) ? (object)DBNull.Value : codigo;
                cmd.Parameters.Add("@CategoriaServicioID", SqlDbType.Int).Value = categoriaServicioId;
                cmd.Parameters.Add("@Nombre", SqlDbType.NVarChar, 150).Value = nombre ?? "";
                cmd.Parameters.Add("@Precio", SqlDbType.Decimal).Value = precio;
                cmd.Parameters["@Precio"].Precision = 18;
                cmd.Parameters["@Precio"].Scale = 2;

                cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = activo;

                var pIdOut = new SqlParameter("@ServicioIDOut", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pCodOut = new SqlParameter("@CodigoOut", SqlDbType.NVarChar, 30) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pIdOut);
                cmd.Parameters.Add(pCodOut);

                cn.Open();
                cmd.ExecuteNonQuery();

                int id = (pIdOut.Value == DBNull.Value) ? 0 : Convert.ToInt32(pIdOut.Value);
                string cod = (pCodOut.Value == DBNull.Value) ? "" : Convert.ToString(pCodOut.Value);

                return (id, cod);
            }
        }

        public void SetActivo(int usuarioId, int servicioId, bool activo)
        {
            SqlExecutor.ExecuteNonQuery(
                "ops.usp_Servicio_SetActivo",
                new SqlParameter("@UsuarioID", usuarioId),
                new SqlParameter("@ServicioID", servicioId),
                new SqlParameter("@Activo", activo)
            );
        }
        public string GetNextCodigo(int usuarioId)
        {
            var dt = SqlExecutor.ExecuteDataTable(
                "ops.usp_Servicio_GetNextCodigo",
                new SqlParameter("@UsuarioID", usuarioId)
            );

            if (dt == null || dt.Rows.Count == 0) return "";
            return Convert.ToString(dt.Rows[0]["Codigo"]) ?? "";
        }
    }
}