using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class TurnoDAO
    {
        private readonly string connectionString = ConfiguracionSegura.Cadena;

        public List<Turno> ObtenerTodos()
        {
            var lista = new List<Turno>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Turno WHERE ISNULL(Eliminado, 0) = 0", con);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Turno
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                        Motivo = reader["Motivo"].ToString(),
                        TipoTramite = reader["TipoTramite"]?.ToString(),
                        Estado = reader["Estado"]?.ToString(),
                        Observaciones = reader["Observaciones"]?.ToString(),
                        Confirmado = Convert.ToBoolean(reader["Confirmado"])
                    });
                }
            }
            return lista;
        }

        public bool ExisteTurnoParaCliente(int idCliente)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Turno WHERE IdCliente = @Id", con);
                cmd.Parameters.AddWithValue("@Id", idCliente);

                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        public void EliminarTurno(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Turno SET Eliminado = 1 WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Turno ObtenerPorId(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Turno WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Turno
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                        TipoTramite = reader["TipoTramite"].ToString(),
                        Estado = reader["Estado"].ToString()
                    };
                }
            }
            return null;
        }
        public void Insertar(Turno turno)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO Turno (IdCliente, FechaHora, Motivo, TipoTramite, Estado, Observaciones, Confirmado)
                    VALUES (@IdCliente, @FechaHora, @Motivo, @TipoTramite, @Estado, @Observaciones, @Confirmado)", con);

                cmd.Parameters.AddWithValue("@IdCliente", turno.IdCliente);
                cmd.Parameters.AddWithValue("@FechaHora", turno.FechaHora);
                cmd.Parameters.AddWithValue("@Motivo", turno.Motivo);
                cmd.Parameters.AddWithValue("@TipoTramite", turno.TipoTramite ?? "");
                cmd.Parameters.AddWithValue("@Estado", turno.Estado ?? "Pendiente");
                cmd.Parameters.AddWithValue("@Observaciones", turno.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@Confirmado", turno.Confirmado);

                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Turno SET Eliminado = 1 WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Confirmar(int id)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Turno SET Confirmado = 1, Estado = 'Confirmado' WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Modificar(Turno turno)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                    UPDATE Turno
                    SET FechaHora = @FechaHora,
                        Motivo = @Motivo,
                        TipoTramite = @TipoTramite,
                        Estado = @Estado,
                        Observaciones = @Observaciones,
                        Confirmado = @Confirmado
                    WHERE Id = @Id", con);

                cmd.Parameters.AddWithValue("@Id", turno.Id);
                cmd.Parameters.AddWithValue("@FechaHora", turno.FechaHora);
                cmd.Parameters.AddWithValue("@Motivo", turno.Motivo);
                cmd.Parameters.AddWithValue("@TipoTramite", turno.TipoTramite ?? "");
                cmd.Parameters.AddWithValue("@Estado", turno.Estado ?? "Pendiente");
                cmd.Parameters.AddWithValue("@Observaciones", turno.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@Confirmado", turno.Confirmado);

                cmd.ExecuteNonQuery();
            }
        }

        public List<Turno> BuscarPorTexto(string texto)
        {
            var lista = new List<Turno>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT * FROM Turno
                    WHERE ISNULL(Eliminado, 0) = 0
                      AND (Motivo LIKE @Texto OR TipoTramite LIKE @Texto OR Estado LIKE @Texto)", con);
                cmd.Parameters.AddWithValue("@Texto", $"%{texto}%");

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new Turno
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        FechaHora = Convert.ToDateTime(reader["FechaHora"]),
                        Motivo = reader["Motivo"].ToString(),
                        TipoTramite = reader["TipoTramite"]?.ToString(),
                        Estado = reader["Estado"]?.ToString(),
                        Observaciones = reader["Observaciones"]?.ToString(),
                        Confirmado = Convert.ToBoolean(reader["Confirmado"])
                    });
                }
            }
            return lista;
        }
    }
}