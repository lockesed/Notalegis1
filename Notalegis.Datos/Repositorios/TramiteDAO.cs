using Notalegis.Datos.Conexion;
using Notalegis.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Notalegis.Datos.Repositorios
{
    public class TramiteDAO
    {
        private readonly ConexionBD conexion = new ConexionBD();

        public List<Tramite> ObtenerTodos()
        {
            List<Tramite> lista = new List<Tramite>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM tramite WHERE ISNULL(Eliminado, 0) = 0", conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Tramite
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Cliente = reader["Cliente"].ToString(),
                            TipoTramite = reader["TipoTramite"].ToString(),
                            NumeroExpediente = reader["NumeroExpediente"] == DBNull.Value ? "" : reader["NumeroExpediente"].ToString(),
                            FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["FechaCreacion"]),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                            Estado = reader["Estado"].ToString(),
                            Prioridad = reader["Prioridad"] == DBNull.Value ? "" : reader["Prioridad"].ToString(),
                            MontoEstimado = reader["MontoEstimado"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MontoEstimado"]),
                            DocumentacionCompleta = reader["DocumentacionCompleta"] == DBNull.Value ? false : Convert.ToBoolean(reader["DocumentacionCompleta"]),
                            Observaciones = reader["Observaciones"] == DBNull.Value ? "" : reader["Observaciones"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        public void Agregar(Tramite tramite)
{
    using (SqlConnection conn = conexion.ObtenerConexion())
    {
        SqlCommand cmd = new SqlCommand(
            @"INSERT INTO tramite 
            (Cliente, TipoTramite, NumeroExpediente, FechaCreacion, FechaInicio, FechaVencimiento, Estado, Prioridad, MontoEstimado, DocumentacionCompleta, Observaciones)
            VALUES (@Cliente, @TipoTramite, @NumeroExpediente, @FechaCreacion, @FechaInicio, @FechaVencimiento, @Estado, @Prioridad, @MontoEstimado, @DocumentacionCompleta, @Observaciones);
            SELECT CAST(scope_identity() AS int);", conn);

        cmd.Parameters.AddWithValue("@Cliente", tramite.Cliente);
        cmd.Parameters.AddWithValue("@TipoTramite", tramite.TipoTramite);
        cmd.Parameters.AddWithValue("@NumeroExpediente", (object)tramite.NumeroExpediente ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FechaCreacion", (tramite.FechaCreacion < new DateTime(1753,1,1)) ? (object)DBNull.Value : tramite.FechaCreacion);
        cmd.Parameters.AddWithValue("@FechaInicio", (tramite.FechaInicio < new DateTime(1753, 1, 1)) ? (object)DBNull.Value : tramite.FechaInicio);
        cmd.Parameters.AddWithValue("@FechaVencimiento", (tramite.FechaVencimiento < new DateTime(1753, 1, 1)) ? (object)DBNull.Value : tramite.FechaVencimiento);
        cmd.Parameters.AddWithValue("@Estado", tramite.Estado);
        cmd.Parameters.AddWithValue("@Prioridad", tramite.Prioridad ?? "");
        cmd.Parameters.AddWithValue("@MontoEstimado", (object)tramite.MontoEstimado ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DocumentacionCompleta", tramite.DocumentacionCompleta);
        cmd.Parameters.AddWithValue("@Observaciones", (object)tramite.Observaciones ?? DBNull.Value);

        // Obtener el nuevo Id generado
        tramite.Id = (int)cmd.ExecuteScalar();
    }
}

        public void Editar(Tramite tramite)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                if (tramite.FechaCreacion < new DateTime(1753, 1, 1))
                {
                    using (SqlCommand cmdFecha = new SqlCommand("SELECT FechaCreacion FROM tramite WHERE Id = @Id", conn))
                    {
                        cmdFecha.Parameters.AddWithValue("@Id", tramite.Id);
                        var fechaActual = cmdFecha.ExecuteScalar();
                        tramite.FechaCreacion = (fechaActual != null && fechaActual != DBNull.Value) ? (DateTime)fechaActual : DateTime.Now;
                    }
                }

                SqlCommand cmd = new SqlCommand(
                    @"UPDATE tramite SET 
                        Cliente = @Cliente,
                        TipoTramite = @TipoTramite,
                        NumeroExpediente = @NumeroExpediente,
                        FechaCreacion = @FechaCreacion,
                        FechaInicio = @FechaInicio,
                        FechaVencimiento = @FechaVencimiento,
                        Estado = @Estado,
                        Prioridad = @Prioridad,
                        MontoEstimado = @MontoEstimado,
                        DocumentacionCompleta = @DocumentacionCompleta,
                        Observaciones = @Observaciones
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Cliente", tramite.Cliente);
                cmd.Parameters.AddWithValue("@TipoTramite", tramite.TipoTramite);
                cmd.Parameters.AddWithValue("@NumeroExpediente", (object)tramite.NumeroExpediente ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaCreacion", (tramite.FechaCreacion < new DateTime(1753,1,1)) ? (object)DBNull.Value : tramite.FechaCreacion);
                cmd.Parameters.AddWithValue("@FechaInicio", tramite.FechaInicio);
                cmd.Parameters.AddWithValue("@FechaVencimiento", tramite.FechaVencimiento);
                cmd.Parameters.AddWithValue("@Estado", tramite.Estado);
                cmd.Parameters.AddWithValue("@Prioridad", tramite.Prioridad ?? "");
                cmd.Parameters.AddWithValue("@MontoEstimado", (object)tramite.MontoEstimado ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentacionCompleta", tramite.DocumentacionCompleta);
                cmd.Parameters.AddWithValue("@Observaciones", (object)tramite.Observaciones ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", tramite.Id);

                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("UPDATE tramite SET Eliminado = 1 WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Tramite ObtenerPorId(int id)
        {
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM tramite WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Tramite
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Cliente = reader["Cliente"].ToString(),
                            TipoTramite = reader["TipoTramite"].ToString(),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                            Estado = reader["Estado"].ToString(),
                            Prioridad = reader["Prioridad"] == DBNull.Value ? "" : reader["Prioridad"].ToString(),
                            MontoEstimado = reader["MontoEstimado"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MontoEstimado"]),
                            Observaciones = reader["Observaciones"] == DBNull.Value ? "" : reader["Observaciones"].ToString()
                        };
                    }
                }
            }
            return null;
        }

        public List<Tramite> Buscar(string filtro)
        {
            List<Tramite> lista = new List<Tramite>();
            using (SqlConnection conn = conexion.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT * FROM tramite WHERE ISNULL(Eliminado, 0) = 0 AND (Cliente LIKE @filtro OR TipoTramite LIKE @filtro OR Estado LIKE @filtro)", conn);
                cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Tramite
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Cliente = reader["Cliente"].ToString(),
                            TipoTramite = reader["TipoTramite"].ToString(),
                            NumeroExpediente = reader["NumeroExpediente"] == DBNull.Value ? "" : reader["NumeroExpediente"].ToString(),
                            FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["FechaCreacion"]),
                            FechaInicio = Convert.ToDateTime(reader["FechaInicio"]),
                            FechaVencimiento = Convert.ToDateTime(reader["FechaVencimiento"]),
                            Estado = reader["Estado"].ToString(),
                            Prioridad = reader["Prioridad"] == DBNull.Value ? "" : reader["Prioridad"].ToString(),
                            MontoEstimado = reader["MontoEstimado"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["MontoEstimado"]),
                            DocumentacionCompleta = reader["DocumentacionCompleta"] == DBNull.Value ? false : Convert.ToBoolean(reader["DocumentacionCompleta"]),
                            Observaciones = reader["Observaciones"] == DBNull.Value ? "" : reader["Observaciones"].ToString()
                        });
                    }
                }
            }
            return lista;
        }
    }
}
