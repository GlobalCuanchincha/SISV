namespace Dominio_SISV.Services.Usuarios
{
    public sealed class GuardarUsuarioRequest
    {
        public int UsuarioSesionId { get; set; }

        // null => nuevo
        public int? UsuarioTargetId { get; set; }

        public string Username { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }

        public string Email { get; set; }
        public string Telefono { get; set; }

        public int RolId { get; set; }
        public bool Activo { get; set; }

        // Si viene: en nuevo obligatorio, en update opcional
        public string PasswordPlain { get; set; }

        // opcional (si se seleccionó foto en UI)
        public byte[] FotoBytes { get; set; }
    }
}