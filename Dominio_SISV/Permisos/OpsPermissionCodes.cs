namespace Dominio_SISV.Permisos
{
    public static class OpsPermissionCodes
    {
        public const string EquiposAcceso = "OPS_EQUIPOS_ACCESO";
        public const string EquiposRegistrar = "OPS_EQUIPOS_REGISTRAR";
        public const string EquiposActualizar = "OPS_EQUIPOS_ACTUALIZAR";
        public const string EquiposDesactivar = "OPS_EQUIPOS_DESACTIVAR";

        public const string RecepcionAcceso = "OPS_RECEPCION_ACCESO";
        public const string RecepcionCrearOrden = "OPS_RECEPCION_CREAR_ORDEN";
        public const string RecepcionAsignarTecnico = "OPS_RECEPCION_ASIGNAR_TECNICO";
        public const string RecepcionEditar = "OPS_RECEPCION_EDITAR";

        public const string NotifAcceso = "OPS_NOTIF_ACCESO";
        public const string NotifGuardarDiag = "OPS_NOTIF_GUARDAR_DIAG";
        public const string NotifCambiarEstado = "OPS_NOTIF_CAMBIAR_ESTADO";
        public const string NotifEnviarCorreo = "OPS_NOTIF_ENVIAR_CORREO";
    }
}