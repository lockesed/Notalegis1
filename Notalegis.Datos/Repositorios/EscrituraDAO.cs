using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class EscrituraDAO
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public List<Escritura> ObtenerTodos()
        {
            List<Escritura> lista = new List<Escritura>();
            
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = @"SELECT * FROM vw_EscriturasCompletas ORDER BY Fecha DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearEscritura(reader));
                    }
                }
            }
            
            return lista;
        }

        public void Insertar(Escritura escritura)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = @"INSERT INTO Escritura 
                                (ClienteId, TipoEscrituraId, NumeroProtocolo, Fecha, RutaArchivo, 
                                FormatoArchivo, Estado, TramiteId, CobroId, Observaciones, UsuarioCreacion)
                                VALUES 
                                (@ClienteId, @TipoEscrituraId, @NumeroProtocolo, @Fecha, @RutaArchivo, 
                                @FormatoArchivo, @Estado, @TramiteId, @CobroId, @Observaciones, @UsuarioCreacion);
                                SELECT CAST(scope_identity() AS int);";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", escritura.ClienteId);
                cmd.Parameters.AddWithValue("@TipoEscrituraId", escritura.TipoEscrituraId);
                cmd.Parameters.AddWithValue("@NumeroProtocolo", escritura.NumeroProtocolo);
                cmd.Parameters.AddWithValue("@Fecha", escritura.Fecha);
                cmd.Parameters.AddWithValue("@RutaArchivo", (object)escritura.RutaArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FormatoArchivo", (object)escritura.FormatoArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", escritura.Estado);
                cmd.Parameters.AddWithValue("@TramiteId", (object)escritura.TramiteId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CobroId", (object)escritura.CobroId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object)escritura.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioCreacion", (object)escritura.UsuarioCreacion ?? DBNull.Value);
                
                escritura.Id = (int)cmd.ExecuteScalar();
            }
        }

        public void Modificar(Escritura escritura)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = @"UPDATE Escritura SET 
                                ClienteId = @ClienteId,
                                TipoEscrituraId = @TipoEscrituraId,
                                NumeroProtocolo = @NumeroProtocolo,
                                Fecha = @Fecha,
                                RutaArchivo = @RutaArchivo,
                                FormatoArchivo = @FormatoArchivo,
                                Estado = @Estado,
                                TramiteId = @TramiteId,
                                CobroId = @CobroId,
                                Observaciones = @Observaciones,
                                FechaProtocolizacion = @FechaProtocolizacion
                                WHERE Id = @Id";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", escritura.ClienteId);
                cmd.Parameters.AddWithValue("@TipoEscrituraId", escritura.TipoEscrituraId);
                cmd.Parameters.AddWithValue("@NumeroProtocolo", escritura.NumeroProtocolo);
                cmd.Parameters.AddWithValue("@Fecha", escritura.Fecha);
                cmd.Parameters.AddWithValue("@RutaArchivo", (object)escritura.RutaArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FormatoArchivo", (object)escritura.FormatoArchivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Estado", escritura.Estado);
                cmd.Parameters.AddWithValue("@TramiteId", (object)escritura.TramiteId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CobroId", (object)escritura.CobroId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Observaciones", (object)escritura.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaProtocolizacion", (object)escritura.FechaProtocolizacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", escritura.Id);
                
                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "UPDATE Escritura SET Eliminado = 1 WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Escritura ObtenerPorId(int id)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "SELECT * FROM vw_EscriturasCompletas WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapearEscritura(reader);
                    }
                }
            }
            
            return null;
        }

        public List<Escritura> BuscarPorCliente(int clienteId)
        {
            List<Escritura> lista = new List<Escritura>();
            
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = "SELECT * FROM vw_EscriturasCompletas WHERE ClienteId = @ClienteId ORDER BY Fecha DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearEscritura(reader));
                    }
                }
            }
            
            return lista;
        }

        public List<Escritura> Buscar(string filtro)
        {
            List<Escritura> lista = new List<Escritura>();
            
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                string query = @"SELECT * FROM vw_EscriturasCompletas 
                                WHERE NombreCliente LIKE @filtro 
                                OR NumeroProtocolo LIKE @filtro 
                                OR TipoEscritura LIKE @filtro
                                OR ClienteDNI LIKE @filtro
                                ORDER BY Fecha DESC";
                
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearEscritura(reader));
                    }
                }
            }
            
            return lista;
        }

        private Escritura MapearEscritura(SqlDataReader reader)
        {
            return new Escritura
            {
                Id = Convert.ToInt32(reader["Id"]),
                ClienteId = Convert.ToInt32(reader["ClienteId"]),
                TipoEscrituraId = Convert.ToInt32(reader["TipoEscrituraId"]),
                NumeroProtocolo = reader["NumeroProtocolo"].ToString(),
                Fecha = Convert.ToDateTime(reader["Fecha"]),
                RutaArchivo = reader["RutaArchivo"] == DBNull.Value ? "" : reader["RutaArchivo"].ToString(),
                FormatoArchivo = reader["FormatoArchivo"] == DBNull.Value ? "" : reader["FormatoArchivo"].ToString(),
                Estado = reader["Estado"].ToString(),
                TramiteId = reader["TramiteId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["TramiteId"]),
                CobroId = reader["CobroId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CobroId"]),
                Observaciones = reader["Observaciones"] == DBNull.Value ? "" : reader["Observaciones"].ToString(),
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                FechaProtocolizacion = reader["FechaProtocolizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaProtocolizacion"]),
                UsuarioCreacion = reader["UsuarioCreacion"] == DBNull.Value ? "" : reader["UsuarioCreacion"].ToString(),
                // Propiedades de navegación
                NombreCliente = reader["NombreCliente"].ToString(),
                ClienteDNI = reader["ClienteDNI"].ToString(),
                TipoEscritura = reader["TipoEscritura"].ToString()
            };
        }
    }
}
