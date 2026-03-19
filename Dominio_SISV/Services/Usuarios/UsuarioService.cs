using Capa_Corte_Transversal.Security;
using Datos_Acceso.Repositories.Usuarios;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Dominio_SISV.Services.Usuarios
{
    public sealed class UsuarioService : IUsuarioService
    {
        private readonly UsuarioRepository _repo = new UsuarioRepository();

        public async Task<byte> GetRolSesionAsync(int usuarioSesionId)
        {
            var dt = await _repo.UsuarioGetByIdAsync(usuarioSesionId, usuarioSesionId);
            if (dt.Rows.Count == 0) return 0;

            if (dt.Columns.Contains("RolID") && dt.Rows[0]["RolID"] != DBNull.Value)
                return Convert.ToByte(dt.Rows[0]["RolID"]);

            var o = dt.Rows[0][0];
            if (o == null || o == DBNull.Value) return 0;
            return Convert.ToByte(o);
        }

        public Task<DataTable> ListarRolesAsync(bool soloActivos) => _repo.RolesListarAsync(soloActivos);

        public Task<DataTable> ListarEstadosAsync(string modo) => _repo.UsuarioEstadosListarAsync(modo);

        public Task<DataTable> ListarRecientesAsync(int usuarioSesionId, int top) => _repo.UsuarioListarRecientesAsync(usuarioSesionId, top);

        public Task<DataTable> BuscarAsync(int usuarioSesionId, string texto, string filtroKey, string rolFiltro, string estadoFiltro, int top)
            => _repo.UsuarioBuscarAsync(usuarioSesionId, texto, filtroKey, rolFiltro, estadoFiltro, top);

        public Task<DataTable> GetByIdAsync(int usuarioSesionId, int usuarioTargetId) => _repo.UsuarioGetByIdAsync(usuarioSesionId, usuarioTargetId);

        public Task<byte[]> GetFotoAsync(int usuarioSesionId, int usuarioTargetId) => _repo.UsuarioFotoGetAsync(usuarioSesionId, usuarioTargetId);

        public async Task<int> GuardarAsync(GuardarUsuarioRequest req)
        {
            if (req == null) throw new ArgumentNullException("req");

            if (string.IsNullOrWhiteSpace(req.Username))
                throw new Exception("Ingrese el nombre de usuario.");

            if (string.IsNullOrWhiteSpace(req.Nombres))
                throw new Exception("Ingrese los nombres.");

            if (string.IsNullOrWhiteSpace(req.Apellidos))
                throw new Exception("Ingrese los apellidos.");

            if (req.RolId <= 0)
                throw new Exception("Seleccione el rol.");

            byte rolSesion = await GetRolSesionAsync(req.UsuarioSesionId);

            // 1 = SuperAdministrador
            // 2 = Administrador
            if (rolSesion == 2 && (req.RolId == 1 || req.RolId == 2))
                throw new Exception("El Administrador no puede crear ni actualizar usuarios con rol SuperAdministrador o Administrador.");

            bool esNuevo = !req.UsuarioTargetId.HasValue;

            byte[] hash = null, salt = null;
            int? iterations = null;

            if (esNuevo)
            {
                if (string.IsNullOrWhiteSpace(req.PasswordPlain))
                    throw new Exception("Ingrese la contraseña para registrar.");

                var created = PasswordHasher.Create(req.PasswordPlain, 10000);
                hash = ToBytes(created.Hash);
                salt = ToBytes(created.Salt);
                iterations = created.Iterations;
            }

            var dt = await _repo.UsuarioGuardarAsync(
                req.UsuarioSesionId,
                req.UsuarioTargetId,
                req.Username.Trim(),
                req.Nombres.Trim(),
                req.Apellidos.Trim(),
                string.IsNullOrWhiteSpace(req.Email) ? null : req.Email.Trim(),
                string.IsNullOrWhiteSpace(req.Telefono) ? null : req.Telefono.Trim(),
                req.RolId,
                req.Activo,
                hash,
                salt,
                iterations
            );

            int idGuardado = req.UsuarioTargetId ?? 0;

            if (esNuevo)
            {
                if (dt.Rows.Count == 0) throw new Exception("El SP no devolvió el UsuarioID creado.");

                var row = dt.Rows[0];
                if (dt.Columns.Contains("UsuarioID") && row["UsuarioID"] != DBNull.Value) idGuardado = Convert.ToInt32(row["UsuarioID"]);
                else if (dt.Columns.Contains("Id") && row["Id"] != DBNull.Value) idGuardado = Convert.ToInt32(row["Id"]);
                else if (dt.Columns.Contains("UsuarioIDTarget") && row["UsuarioIDTarget"] != DBNull.Value) idGuardado = Convert.ToInt32(row["UsuarioIDTarget"]);
                else throw new Exception("No se encontró columna de ID devuelto (UsuarioID/Id/UsuarioIDTarget).");
            }

            if (!esNuevo && !string.IsNullOrWhiteSpace(req.PasswordPlain))
            {
                var created2 = PasswordHasher.Create(req.PasswordPlain, 10000);
                byte[] hash2 = ToBytes(created2.Hash);
                byte[] salt2 = ToBytes(created2.Salt);

                await _repo.UsuarioSetPasswordAsync(req.UsuarioSesionId, idGuardado, hash2, salt2, created2.Iterations);
            }

            if (req.FotoBytes != null && req.FotoBytes.Length > 0)
            {
                await _repo.UsuarioFotoSetAsync(req.UsuarioSesionId, idGuardado, req.FotoBytes);
            }

            return idGuardado;
        }

        private static byte[] ToBytes(object v)
        {
            if (v == null || v == DBNull.Value) return null;
            if (v is byte[] b) return b;

            // Si tu PasswordHasher devolviera string en Base64/Hex, aquí podrías convertir.
            throw new ArgumentException("Tipo de Hash/Salt no soportado: " + v.GetType().FullName);
        }
    }
}