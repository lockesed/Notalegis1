
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Notalegis.Datos.Repositorios
{
    public class EstadoClienteDAO
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public void RegistrarEstado(EstadoCliente estado)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("INSERT INTO EstadoCliente (IdCliente, Estado, FechaRegistro) VALUES (@IdCliente, @Estado, @FechaRegistro)", conn);
                cmd.Parameters.AddWithValue("@IdCliente", estado.IdCliente);
                cmd.Parameters.AddWithValue("@Estado", estado.Estado);
                cmd.Parameters.AddWithValue("@FechaRegistro", estado.FechaRegistro);
                cmd.ExecuteNonQuery();
            }
        }

        public List<EstadoCliente> ObtenerHistorialPorCliente(int idCliente)
        {
            List<EstadoCliente> historial = new List<EstadoCliente>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM EstadoCliente WHERE IdCliente = @IdCliente ORDER BY FechaRegistro DESC", conn);
                cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historial.Add(new EstadoCliente
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            IdCliente = Convert.ToInt32(reader["IdCliente"]),
                            Estado = reader["Estado"].ToString(),
                            FechaRegistro = Convert.ToDateTime(reader["FechaRegistro"])
                        });
                    }
                }
            }
            return historial;
        }
    }
}
