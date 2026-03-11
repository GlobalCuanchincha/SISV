using Dominio_SISV.DTOs;
using Dominio_SISV.DTOs.Facturacion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace Dominio_SISV.Services.Facturacion
{
    public sealed class FacturacionService : IFacturacionService
    {
        private readonly string _connectionString;

        public FacturacionService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("La cadena de conexión no puede estar vacía.", nameof(connectionString));

            _connectionString = connectionString;
        }

        public ClienteFacturaDto BuscarClientePorCedula(string cedula)
        {
            var procCandidates = new[]
            {
                "crm.usp_Cliente_GetByCedula",
                "crm.usp_Clientes_GetByCedula",
                "crm.usp_Cliente_BuscarPorCedula",
                "crm.usp_Cliente_ObtenerPorCedula"
            };

            var paramCandidates = new[]
            {
                "@Cedula",
                "@Cedula_Clientes",
                "@Identificacion",
                "@pCedula"
            };

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                foreach (var proc in procCandidates)
                {
                    foreach (var pName in paramCandidates)
                    {
                        try
                        {
                            using (var cmd = new SqlCommand(proc, con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue(pName, cedula);

                                using (var rd = cmd.ExecuteReader())
                                {
                                    if (!rd.Read())
                                        continue;

                                    return new ClienteFacturaDto
                                    {
                                        ClienteID = ReadInt(rd, "ClienteID", "ClienteID_Clientes", "Id", "ID"),
                                        Cedula = ReadString(rd, "Cedula_Clientes", "Cedula", "Identificacion"),
                                        Telefono = ReadString(rd, "Telefono_Clientes", "Telefono", "Telefono1"),
                                        Nombre = ReadString(rd, "Nombre_Clientes", "Nombre"),
                                        Apellido = ReadString(rd, "Apellido_Clientes", "Apellido"),
                                        Direccion = ReadString(rd, "Direccion_Clientes", "Direccion"),
                                        Email = ReadString(rd, "Email_Clientes", "Email")
                                    };
                                }
                            }
                        }
                        catch (SqlException)
                        {
                            // intenta la siguiente combinación
                        }
                    }
                }
            }

            return null;
        }

        public List<CatalogItemVM> ObtenerCatalogo()
        {
            var all = new List<CatalogItemVM>();

            var productos = GetProductos();
            var servicios = GetServicios();

            if (productos != null) all.AddRange(productos);
            if (servicios != null) all.AddRange(servicios);

            return all;
        }

        public List<TipoPagoDto> ListarTiposPago()
        {
            var list = new List<TipoPagoDto>();

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("bill.usp_TipoPago_Listar", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                con.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new TipoPagoDto
                        {
                            TipoPagoID = ReadInt(rd, "TipoPagoID", "TipoPagoID_TiposPago", "ID", "Id"),
                            Nombre = ReadString(rd, "Nombre", "Nombre_TiposPago", "Descripcion")
                        });
                    }
                }
            }

            return list;
        }

        public string ObtenerSiguienteCodigoFactura()
        {
            var procCandidates = new[]
            {
                "bill.usp_Factura_GetNextCodigo"
            };

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                foreach (var proc in procCandidates)
                {
                    try
                    {
                        using (var cmd = new SqlCommand(proc, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            object value = cmd.ExecuteScalar();
                            string codigo = value == null ? null : value.ToString();

                            if (!string.IsNullOrWhiteSpace(codigo))
                                return codigo.Trim();
                        }
                    }
                    catch (SqlException)
                    {
                        // intenta el siguiente
                    }
                }
            }

            throw new InvalidOperationException("No se encontró un SP válido para generar el código de factura.");
        }

        public CrearFacturaResultDto CrearFactura(CrearFacturaRequestDto request)
        {
            if (request.UsuarioID <= 0)
                throw new InvalidOperationException("UsuarioID inválido.");

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.ClienteID <= 0)
                throw new InvalidOperationException("ClienteID inválido.");

            if (request.TipoPagoID <= 0)
                throw new InvalidOperationException("TipoPagoID inválido.");

            if (request.Items == null || request.Items.Count == 0)
                throw new InvalidOperationException("La factura no tiene ítems.");

            const string proc = "bill.usp_Factura_Crear";
            const string tvpType = "bill.TVP_FacturaItem";

            DataTable tvp = BuildFacturaItemsTvp(request.Items);

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                using (var check = new SqlCommand(
                    "SELECT 1 FROM sys.types WHERE is_table_type = 1 AND SCHEMA_NAME(schema_id) = @schema AND name = @name", con))
                {
                    check.Parameters.AddWithValue("@schema", "bill");
                    check.Parameters.AddWithValue("@name", "TVP_FacturaItem");

                    if (check.ExecuteScalar() == null)
                        throw new InvalidOperationException("No existe el TYPE bill.TVP_FacturaItem en la base de datos.");
                }

                try
                {
                    using (var cmd = new SqlCommand(proc, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 30;

                        cmd.Parameters.AddWithValue("@UsuarioID", request.UsuarioID);
                        cmd.Parameters.AddWithValue("@ClienteID", request.ClienteID);
                        cmd.Parameters.AddWithValue("@NumeroFactura",
                            string.IsNullOrWhiteSpace(request.NumeroFactura)
                                ? (object)DBNull.Value
                                : request.NumeroFactura);

                        cmd.Parameters.AddWithValue("@Subtotal", request.Subtotal);
                        cmd.Parameters.AddWithValue("@Descuento", request.Descuento);
                        cmd.Parameters.AddWithValue("@IVA", request.IVA);
                        cmd.Parameters.AddWithValue("@Total", request.Total);
                        cmd.Parameters.AddWithValue("@TipoPagoID", request.TipoPagoID);

                        var pItems = cmd.Parameters.Add("@Items", SqlDbType.Structured);
                        pItems.TypeName = tvpType;
                        pItems.Value = tvp;

                        var pFacturaID = new SqlParameter("@FacturaID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pFacturaID);

                        var pNumeroFacturaOut = new SqlParameter("@NumeroFacturaOut", SqlDbType.VarChar, 20)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(pNumeroFacturaOut);

                        cmd.ExecuteNonQuery();

                        int facturaId = pFacturaID.Value == DBNull.Value ? 0 : Convert.ToInt32(pFacturaID.Value);
                        string numeroFactura = pNumeroFacturaOut.Value == DBNull.Value
                            ? (request.NumeroFactura ?? string.Empty)
                            : pNumeroFacturaOut.Value.ToString();

                        if (facturaId <= 0)
                            throw new InvalidOperationException("El SP no devolvió un FacturaID válido.");

                        return new CrearFacturaResultDto
                        {
                            FacturaID = facturaId,
                            NumeroFactura = numeroFactura
                        };
                    }
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException(
                        "No se pudo ejecutar bill.usp_Factura_Crear.\n\n" + ex.Message,
                        ex);
                }
            }
        }

        private List<CatalogItemVM> GetProductos()
        {
            var procCandidates = new[]
            {
                "inv.usp_ItemsInventario_Listar",
                "inv.usp_ItemsInventario_ListarActivos",
                "inv.usp_Productos_Listar",
                "inv.usp_Producto_Listar"
            };

            return ExecuteCatalogProc(procCandidates, "PRODUCTO");
        }

        private List<CatalogItemVM> GetServicios()
        {
            var procCandidates = new[]
            {
                "ops.usp_Servicios_Listar",
                "ops.usp_Servicio_Listar",
                "ops.usp_Servicios_Obtener"
            };

            return ExecuteCatalogProc(procCandidates, "SERVICIO");
        }

        private List<CatalogItemVM> ExecuteCatalogProc(string[] procCandidates, string tipoForzado)
        {
            var list = new List<CatalogItemVM>();

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                foreach (var proc in procCandidates)
                {
                    try
                    {
                        using (var cmd = new SqlCommand(proc, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            using (var rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    int id = ReadInt(rd,
                                        "ItemInventarioID", "ItemInventarioID_ItemsInventario",
                                        "ServicioID", "ServicioID_Servicios",
                                        "ProductoID", "ID", "Id");

                                    string codigo = ReadString(rd, "SKU", "Codigo", "CodigoItem", "CodigoServicio");
                                    string nombre = ReadString(rd, "Nombre", "NombreItem", "NombreServicio", "Descripcion");
                                    decimal precio = ReadDecimal(rd, "Precio", "PrecioVenta", "PrecioUnitario", "Precio_Servicios", "Precio_ItemsInventario");
                                    int? stock = ReadNullableInt(rd, "Stock", "StockActual", "Stock_ItemsInventario", "Cantidad");
                                    bool activo = ReadBool(rd, "Activo", "Estado", "IsActive");

                                    if (string.IsNullOrWhiteSpace(codigo))
                                        codigo = tipoForzado == "SERVICIO" ? $"S{id:0000}" : $"PRD-{id:0000}";

                                    if (string.IsNullOrWhiteSpace(nombre))
                                        nombre = tipoForzado == "SERVICIO" ? $"Servicio {id}" : $"Producto {id}";

                                    list.Add(new CatalogItemVM
                                    {
                                        Id = id,
                                        Codigo = codigo,
                                        Nombre = nombre,
                                        Tipo = tipoForzado,
                                        Precio = precio,
                                        Stock = tipoForzado == "SERVICIO" ? (int?)null : (stock ?? 0),
                                        Activo = activo
                                    });
                                }
                            }
                        }

                        if (list.Count > 0)
                            break;
                    }
                    catch (SqlException)
                    {
                        // intenta el siguiente proc
                    }
                }
            }

            return list;
        }

        private static DataTable BuildFacturaItemsTvp(List<FacturaItemVM> items)
        {
            var dt = new DataTable();
            dt.Columns.Add("ItemInventarioID", typeof(int));
            dt.Columns.Add("ServicioID", typeof(int));
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("PrecioUnitario", typeof(decimal));
            dt.Columns.Add("Subtotal", typeof(decimal));

            foreach (var it in items)
            {
                var row = dt.NewRow();
                row["ItemInventarioID"] = it.ItemInventarioID.HasValue ? (object)it.ItemInventarioID.Value : DBNull.Value;
                row["ServicioID"] = it.ServicioID.HasValue ? (object)it.ServicioID.Value : DBNull.Value;
                row["Cantidad"] = it.Cantidad;
                row["PrecioUnitario"] = it.PrecioUnitario;
                row["Subtotal"] = it.Subtotal;
                dt.Rows.Add(row);
            }

            return dt;
        }

        private static bool Has(IDataRecord rd, string name)
        {
            for (int i = 0; i < rd.FieldCount; i++)
            {
                if (string.Equals(rd.GetName(i), name, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static int ReadInt(IDataRecord rd, params string[] names)
        {
            foreach (var name in names)
            {
                if (!Has(rd, name)) continue;

                object value = rd[name];
                if (value == null || value == DBNull.Value) continue;

                if (int.TryParse(value.ToString(), out int x))
                    return x;
            }

            return 0;
        }

        private static int? ReadNullableInt(IDataRecord rd, params string[] names)
        {
            foreach (var name in names)
            {
                if (!Has(rd, name)) continue;

                object value = rd[name];
                if (value == null || value == DBNull.Value) continue;

                if (int.TryParse(value.ToString(), out int x))
                    return x;
            }

            return null;
        }

        private static decimal ReadDecimal(IDataRecord rd, params string[] names)
        {
            foreach (var name in names)
            {
                if (!Has(rd, name)) continue;

                object value = rd[name];
                if (value == null || value == DBNull.Value) continue;

                if (value is decimal d)
                    return d;

                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out decimal x))
                    return x;

                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                    return x;
            }

            return 0m;
        }

        private static string ReadString(IDataRecord rd, params string[] names)
        {
            foreach (var name in names)
            {
                if (!Has(rd, name)) continue;

                object value = rd[name];
                if (value == null || value == DBNull.Value) continue;

                string s = value.ToString();
                if (!string.IsNullOrWhiteSpace(s))
                    return s;
            }

            return string.Empty;
        }

        private static bool ReadBool(IDataRecord rd, params string[] names)
        {
            foreach (var name in names)
            {
                if (!Has(rd, name)) continue;

                object value = rd[name];
                if (value == null || value == DBNull.Value) continue;

                if (value is bool b)
                    return b;

                if (int.TryParse(value.ToString(), out int i))
                    return i != 0;

                string s = (value.ToString() ?? string.Empty).Trim().ToUpperInvariant();

                if (s == "ACTIVO" || s == "A" || s == "TRUE" || s == "SI" || s == "SÍ")
                    return true;

                if (s == "INACTIVO" || s == "I" || s == "FALSE" || s == "NO")
                    return false;
            }

            return true;
        }
    }
}