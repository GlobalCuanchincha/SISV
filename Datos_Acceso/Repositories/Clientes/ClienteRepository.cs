using Datos_Acceso.Common;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Datos_Acceso.Repositories.Clientes
{
    public sealed class ClienteRepository
    {
        public DataTable ListarEstados()
        {
            return SqlExecutor.ExecuteDataTable("crm.usp_Cliente_Estados_Listar");
        }

        public DataTable Buscar(string filtroPor, string buscar, int? estadoKey, int top)
        {
            return SqlExecutor.ExecuteDataTable(
                "crm.usp_Cliente_Buscar",
                new SqlParameter("@FiltroPor", SqlDbType.NVarChar, 30) { Value = (object)(filtroPor ?? "nombre") },
                new SqlParameter("@Buscar", SqlDbType.NVarChar, 200) { Value = string.IsNullOrWhiteSpace(buscar) ? (object)DBNull.Value : buscar },
                new SqlParameter("@EstadoKey", SqlDbType.Int) { Value = estadoKey.HasValue ? (object)estadoKey.Value : DBNull.Value },
                new SqlParameter("@Top", SqlDbType.Int) { Value = top }
            );
        }

        public DataTable GetByCedula(string cedula)
        {
            return SqlExecutor.ExecuteDataTable(
                "crm.usp_Cliente_GetByCedula",
                new SqlParameter("@Cedula", SqlDbType.NVarChar, 30) { Value = cedula }
            );
        }

        public DataTable Crear(string cedula, string nombres, string apellidos, string correo, string telefono, string direccion, int? estadoKey)
        {
            var pOut = new SqlParameter("@ClienteIDOut", SqlDbType.Int) { Direction = ParameterDirection.Output };

            return SqlExecutor.ExecuteDataTable(
                "crm.usp_Cliente_Crear",
                new SqlParameter("@Cedula", SqlDbType.NVarChar, 30) { Value = cedula },
                new SqlParameter("@Nombres", SqlDbType.NVarChar, 120) { Value = nombres },
                new SqlParameter("@Apellidos", SqlDbType.NVarChar, 120) { Value = apellidos },
                new SqlParameter("@Correo", SqlDbType.NVarChar, 220) { Value = string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo },
                new SqlParameter("@Telefono", SqlDbType.NVarChar, 30) { Value = string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono },
                new SqlParameter("@Direccion", SqlDbType.NVarChar, 220) { Value = string.IsNullOrWhiteSpace(direccion) ? (object)DBNull.Value : direccion },
                new SqlParameter("@EstadoKey", SqlDbType.Int) { Value = estadoKey.HasValue ? (object)estadoKey.Value : DBNull.Value },
                pOut
            );
        }

        public DataTable Actualizar(string cedula, string nombres, string apellidos, string correo, string telefono, string direccion, int? estadoKey)
        {
            return SqlExecutor.ExecuteDataTable(
                "crm.usp_Cliente_Actualizar",
                new SqlParameter("@Cedula", SqlDbType.NVarChar, 30) { Value = cedula },
                new SqlParameter("@Nombres", SqlDbType.NVarChar, 120) { Value = string.IsNullOrWhiteSpace(nombres) ? (object)DBNull.Value : nombres },
                new SqlParameter("@Apellidos", SqlDbType.NVarChar, 120) { Value = string.IsNullOrWhiteSpace(apellidos) ? (object)DBNull.Value : apellidos },
                new SqlParameter("@Correo", SqlDbType.NVarChar, 220) { Value = string.IsNullOrWhiteSpace(correo) ? (object)DBNull.Value : correo },
                new SqlParameter("@Telefono", SqlDbType.NVarChar, 30) { Value = string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono },
                new SqlParameter("@Direccion", SqlDbType.NVarChar, 220) { Value = string.IsNullOrWhiteSpace(direccion) ? (object)DBNull.Value : direccion },
                new SqlParameter("@EstadoKey", SqlDbType.Int) { Value = estadoKey.HasValue ? (object)estadoKey.Value : DBNull.Value }
            );
        }
    }
}