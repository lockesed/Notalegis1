using System.Collections.Generic;
using System.Data.SqlClient;
using System;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    internal static class SqlDataReaderExtensions
    {
        public static bool HasColumn(this SqlDataReader r, string columnName)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }
    }
}

namespace Notalegis.Datos.Repositorios
{
    public class UsuarioDAO
    {
        private readonly string connectionString = ConfiguracionSegura.Cadena;

        public Usuario ValidarCredenciales(string usuario, string contraseña)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios WHERE NombreUsuario = @Usuario AND Contraseña = @Contraseña", con);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                cmd.Parameters.AddWithValue("@Contraseña", contraseña);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapearUsuario(reader);
                    }
                }
            }
            return null;
        }

        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios WHERE NombreUsuario = @Usuario", con);
                cmd.Parameters.AddWithValue("@Usuario", nombreUsuario);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapearUsuario(reader);
                    }
                }
            }
            return null;
        }

        public List<Usuario> ObtenerTodos()
        {
            List<Usuario> lista = new List<Usuario>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios", con);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(MapearUsuario(reader));
                }
            }
            return lista;
        }

        private static Usuario MapearUsuario(SqlDataReader r)
        {
            return new Usuario
            {
                Id             = Convert.ToInt32(r["Id"]),
                NombreUsuario  = r["NombreUsuario"].ToString(),
                Contraseña     = r["Contraseña"].ToString(),
                Rol            = r["Rol"].ToString(),
                IntentosLogin  = r.HasColumn("IntentosLogin") ? Convert.ToInt32(r["IntentosLogin"]) : 0,
                BloqueadoHasta = r.HasColumn("BloqueadoHasta") && r["BloqueadoHasta"] != DBNull.Value
                                 ? (DateTime?)Convert.ToDateTime(r["BloqueadoHasta"]) : null
            };
        }

        public void RegistrarIntentoFallido(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuarios SET IntentosLogin = IntentosLogin + 1, " +
                    "BloqueadoHasta = CASE WHEN IntentosLogin + 1 >= 5 THEN DATEADD(MINUTE, 30, GETDATE()) ELSE BloqueadoHasta END " +
                    "WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void ResetearIntentos(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Usuarios SET IntentosLogin = 0, BloqueadoHasta = NULL WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Agregar(Usuario usuario)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Usuarios (NombreUsuario, Contraseña, Rol) VALUES (@NombreUsuario, @Contraseña, @Rol)", con);
                cmd.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
                cmd.Parameters.AddWithValue("@Rol", usuario.Rol);
                cmd.ExecuteNonQuery();
            }
        }

        public void Actualizar(Usuario usuario)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Usuarios SET NombreUsuario = @NombreUsuario, Contraseña = @Contraseña, Rol = @Rol WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@NombreUsuario", usuario.NombreUsuario);
                cmd.Parameters.AddWithValue("@Contraseña", usuario.Contraseña);
                cmd.Parameters.AddWithValue("@Rol", usuario.Rol);
                cmd.Parameters.AddWithValue("@Id", usuario.Id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Usuarios WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}