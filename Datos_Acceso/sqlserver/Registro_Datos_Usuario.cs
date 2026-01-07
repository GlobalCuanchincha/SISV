using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using Datos_Acceso.SqlServer;

namespace Datos_Acceso
{
    public class Registro_Datos_Usuario
    {

        private readonly ConexionSQL_Implementacion conexion = new ConexionSQL_Implementacion();
        public string RegistrarTrabajador(string usuario, string contraseña, string nombre, string apellido, string puesto, string correo, string telefono,byte[] foto)
        {
            if (!Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return "El formato del correo electrónico no es válido.";
            try
            {
                using (SqlConnection con = conexion.AbrirConexion())
                {
                    con.Open();

                    string verificarUsuarioQuery = "SELECT COUNT(*) FROM Users WHERE LoginName = @Usuario OR Email = @Correo";
                    using (SqlCommand verificarCmd = new SqlCommand(verificarUsuarioQuery, con))
                    {
                        verificarCmd.Parameters.AddWithValue("@Usuario", usuario);
                        verificarCmd.Parameters.AddWithValue("@Correo", correo);

                        int count = (int)verificarCmd.ExecuteScalar();
                        if (count > 0)
                            return "El nombre de usuario o correo ya existe.";
                    }

                    string insertarQuery = @"INSERT INTO Users (LoginName, Password, FirstName, LastName, Position, Email, Telefono, FotoPerfil) 
                                     VALUES (@Login, @Pass, @Nombre, @Apellido, @Puesto, @Correo, @Telefono, @Foto)";

                    using (SqlCommand cmd = new SqlCommand(insertarQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Login", usuario);
                        cmd.Parameters.AddWithValue("@Pass", contraseña);
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Apellido", apellido);
                        cmd.Parameters.AddWithValue("@Puesto", puesto);
                        cmd.Parameters.AddWithValue("@Correo", correo);
                        cmd.Parameters.AddWithValue("@Telefono", telefono);
                        cmd.Parameters.Add("@Foto", System.Data.SqlDbType.VarBinary).Value = foto ?? (object)DBNull.Value;

                        int filasAfectadas = cmd.ExecuteNonQuery();
                        return filasAfectadas > 0 ? "Registro exitoso" : "No se pudo completar el registro.";
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error al registrar trabajador: " + ex.Message;
            }
        }
    }
}
