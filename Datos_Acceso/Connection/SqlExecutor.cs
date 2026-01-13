using System;
using System.Data;
using System.Data.SqlClient;
using Datos_Acceso.Connection;

namespace Datos_Acceso.Common
{
    public static class SqlExecutor
    {
        public static int ExecuteNonQuery(string spName, params SqlParameter[] parameters)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(spName, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string spName, params SqlParameter[] parameters)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(spName, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                cn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static T ExecuteReaderSingle<T>(string spName, Func<SqlDataReader, T> map, params SqlParameter[] parameters)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(spName, cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                cn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return default(T);
                    return map(rd);
                }
            }
        }
        public static DataTable ExecuteDataTable(string spName, params SqlParameter[] parameters)
        {
            using (var cn = DbConnection.Create())
            using (var cmd = new SqlCommand(spName, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                var dt = new DataTable();
                cn.Open();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
