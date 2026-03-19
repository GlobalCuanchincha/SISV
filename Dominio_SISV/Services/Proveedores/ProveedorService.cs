using Datos_Acceso.Repositories.Proveedores;
using Dominio_SISV.DTOs;
using System;
using System.Collections.Generic;
using System.Data;

namespace Dominio_SISV.Services.Proveedores
{
    public sealed class ProveedorService : IProveedorService
    {
        private readonly ProveedorRepository _repo = new ProveedorRepository();

        public List<ProveedorDetalleVM> Buscar(int usuarioId, string texto, string filtro, string estadoTexto, int top)
        {
            DataTable dt = _repo.Buscar(usuarioId, texto, filtro, estadoTexto, top);
            var list = new List<ProveedorDetalleVM>();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new ProveedorDetalleVM
                {
                    ProveedorId = ToInt(r, "ProveedorID"),
                    Ruc = ToStr(r, "RUC"),
                    Nombre = ToStr(r, "Nombre"),
                    Telefono = ToStr(r, "Telefono"),
                    Activo = ToBool(r, "Activo"),
                    UltimaActualizacion = ToDateTimeNullable(r, "UltimaActualizacion")
                });
            }

            return list;
        }

        public ProveedorDetalleVM GetById(int usuarioId, int proveedorId)
        {
            DataTable dt = _repo.GetById(usuarioId, proveedorId);
            if (dt == null || dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new ProveedorDetalleVM
            {
                ProveedorId = ToInt(r, "ProveedorID"),
                Nombre = ToStr(r, "Nombre"),
                Ruc = ToStr(r, "RUC"),
                Telefono = ToStr(r, "Telefono"),
                Correo = ToStr(r, "Correo"),
                Direccion = ToStr(r, "Direccion"),
                Activo = ToBool(r, "Activo"),
                UltimaActualizacion = ToDateTimeNullable(r, "UltimaActualizacion")
            };
        }

        public ProveedorDetalleVM Guardar(int usuarioId, int? proveedorId, ProveedorDetalleVM input)
        {
            if (input == null) throw new InvalidOperationException("Datos inválidos.");
            if (string.IsNullOrWhiteSpace(input.Nombre)) throw new InvalidOperationException("Ingrese el nombre del proveedor.");
            if (string.IsNullOrWhiteSpace(input.Ruc)) throw new InvalidOperationException("Ingrese el RUC.");

            string estadoTexto = input.Activo ? "Activo" : "Inactivo";

            DataTable dt = _repo.Upsert(usuarioId, proveedorId, input.Nombre, input.Ruc, input.Telefono, input.Correo, input.Direccion, estadoTexto);
            if (dt == null || dt.Rows.Count == 0)
                return null;

            DataRow r = dt.Rows[0];

            return new ProveedorDetalleVM
            {
                ProveedorId = ToInt(r, "ProveedorID"),
                Nombre = ToStr(r, "Nombre"),
                Ruc = ToStr(r, "RUC"),
                Telefono = ToStr(r, "Telefono"),
                Correo = ToStr(r, "Correo"),
                Direccion = ToStr(r, "Direccion"),
                Activo = ToBool(r, "Activo"),
                UltimaActualizacion = ToDateTimeNullable(r, "UltimaActualizacion")
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

        private static bool ToBool(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return false;
            bool b; if (bool.TryParse(Convert.ToString(r[col]), out b)) return b;
            int x; return int.TryParse(Convert.ToString(r[col]), out x) && x != 0;
        }

        private static DateTime? ToDateTimeNullable(DataRow r, string col)
        {
            if (r == null || !r.Table.Columns.Contains(col) || r[col] == DBNull.Value) return null;

            DateTime dt;
            if (DateTime.TryParse(Convert.ToString(r[col]), out dt)) return dt;
            return null;
        }
    }
}