using Datos_Acceso.Repositories.Clientes;
using Dominio_SISV.DTOs.Clientes;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dominio_SISV.Services.Clientes
{
    public sealed class ClienteService : IClienteService
    {
        private readonly ClienteRepository _repo = new ClienteRepository();

        public List<ClienteEstadoVM> ListarEstados()
        {
            var dt = _repo.ListarEstados();
            var list = new List<ClienteEstadoVM>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new ClienteEstadoVM
                {
                    EstadoKey = ToIntNullable(r, "EstadoKey"),
                    EstadoNombre = ToStr(r, "EstadoNombre"),
                    EsActivo = ToBoolNullable(r, "EsActivo")
                });
            }

            return list;
        }

        public List<ClienteCardVM> Buscar(string filtroPor, string buscar, int? estadoKey, int top)
        {
            var dt = _repo.Buscar(filtroPor, buscar, estadoKey, top);
            var list = new List<ClienteCardVM>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new ClienteCardVM
                {
                    Cedula = ToStr(r, "Cedula"),
                    Cliente = ToStr(r, "Cliente"),
                    Correo = ToStr(r, "Correo"),
                    Telefono = ToStr(r, "Telefono"),
                    EstadoKey = ToIntNullable(r, "EstadoKey"),
                    EstadoNombre = ToStr(r, "EstadoNombre"),
                    EsActivo = ToBoolNullable(r, "EsActivo"),
                    TotalCoincidencias = ToInt(r, "TotalCoincidencias")
                });
            }

            return list;
        }

        public ClienteDetalleVM GetByCedula(string cedula)
        {
            var dt = _repo.GetByCedula(cedula);
            return FirstDetalle(dt);
        }

        public ClienteDetalleVM Crear(ClienteDetalleVM input)
        {
            var dt = _repo.Crear(
                input.Cedula,
                input.Nombres,
                input.Apellidos,
                input.Correo,
                input.Telefono,
                input.Direccion,
                input.EstadoKey
            );

            return FirstDetalle(dt);
        }

        public ClienteDetalleVM Actualizar(string cedula, ClienteDetalleVM input)
        {
            var dt = _repo.Actualizar(
                cedula,
                input.Nombres,
                input.Apellidos,
                input.Correo,
                input.Telefono,
                input.Direccion,
                input.EstadoKey
            );

            return FirstDetalle(dt);
        }

        private static ClienteDetalleVM FirstDetalle(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0) return null;
            var r = dt.Rows[0];

            return new ClienteDetalleVM
            {
                Cedula = ToStr(r, "Cedula"),
                Nombres = ToStr(r, "Nombres"),
                Apellidos = ToStr(r, "Apellidos"),
                Cliente = ToStr(r, "Cliente"),
                Correo = ToStr(r, "Correo"),
                Telefono = ToStr(r, "Telefono"),
                Direccion = ToStr(r, "Direccion"),
                EstadoKey = ToIntNullable(r, "EstadoKey"),
                EstadoNombre = ToStr(r, "EstadoNombre"),
                EsActivo = ToBoolNullable(r, "EsActivo")
            };
        }

        private static string ToStr(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return "";
            return Convert.ToString(r[col]) ?? "";
        }

        private static int ToInt(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return 0;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) ? x : 0;
        }

        private static int? ToIntNullable(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return null;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) ? (int?)x : null;
        }

        private static bool? ToBoolNullable(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return null;

            bool b;
            if (bool.TryParse(Convert.ToString(r[col]), out b)) return b;

            int x;
            if (int.TryParse(Convert.ToString(r[col]), out x)) return x != 0;

            return null;
        }
    }
}