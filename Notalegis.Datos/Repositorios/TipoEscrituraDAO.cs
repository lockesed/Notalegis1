using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class TipoEscrituraDAO
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public List<TipoEscritura> ObtenerTodos()
        {
            List<TipoEscritura> lista = new List<TipoEscritura>();
            
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM TipoEscritura WHERE Activo = 1 ORDER BY Nombre", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new TipoEscritura
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                            RutaPlantilla = reader["RutaPlantilla"] == DBNull.Value ? "" : reader["RutaPlantilla"].ToString(),
                            Activo = Convert.ToBoolean(reader["Activo"]),
                            FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                        });
                    }
                }
            }
            
            return lista;
        }

        public void Insertar(TipoEscritura tipo)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = @"INSERT INTO TipoEscritura (Nombre, Descripcion, RutaPlantilla, Activo)
                                VALUES (@Nombre, @Descripcion, @RutaPlantilla, @Activo)";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", tipo.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object)tipo.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RutaPlantilla", (object)tipo.RutaPlantilla ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", tipo.Activo);
                
                cmd.ExecuteNonQuery();
            }
        }

        public void Modificar(TipoEscritura tipo)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = @"UPDATE TipoEscritura SET 
                                Nombre = @Nombre,
                                Descripcion = @Descripcion,
                                RutaPlantilla = @RutaPlantilla,
                                Activo = @Activo
                                WHERE Id = @Id";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nombre", tipo.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object)tipo.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RutaPlantilla", (object)tipo.RutaPlantilla ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", tipo.Activo);
                cmd.Parameters.AddWithValue("@Id", tipo.Id);
                
                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            // Eliminación lógica
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "UPDATE TipoEscritura SET Activo = 0 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public TipoEscritura ObtenerPorId(int id)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM TipoEscritura WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new TipoEscritura
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader["Descripcion"].ToString(),
                            RutaPlantilla = reader["RutaPlantilla"] == DBNull.Value ? "" : reader["RutaPlantilla"].ToString(),
                            Activo = Convert.ToBoolean(reader["Activo"]),
                            FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"])
                        };
                    }
                }
            }
            
            return null;
        }
    }
}
