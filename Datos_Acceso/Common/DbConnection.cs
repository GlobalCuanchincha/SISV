using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;


namespace Datos_Acceso.Connection
{
    internal class DbConnection
    {
        public static SqlConnection Create()
        {
            string cs = ConfigurationManager.ConnectionStrings["SISV"].ConnectionString;
            return new SqlConnection(cs);
        }
    }
}
