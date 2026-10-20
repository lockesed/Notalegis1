using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    using global::Notalegis.Datos.Conexion;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

        public class ClienteDAO
        {
            private readonly string connectionString = ConfiguracionSegura.Cadena;

            public List<Cliente> ObtenerTodos()
            {
                var lista = new List<Cliente>();
                ConexionBD conexionBD = new ConexionBD();

                using (SqlConnection con = conexionBD.ObtenerConexion())
                {
                    SqlCommand cmd = new SqlCommand("SELECT * FROM Cliente WHERE ISNULL(Eliminado, 0) = 0", con);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Cliente
                        {
                            Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                            Nombre = reader["Nombre"] != DBNull.Value ? reader["Nombre"].ToString() : "",
                            Apellido = reader["Apellido"] != DBNull.Value ? reader["Apellido"].ToString() : "",
                            DNI = reader["DNI"] != DBNull.Value ? reader["DNI"].ToString() : "",
                            Edad = reader["Edad"] != DBNull.Value ? Convert.ToInt32(reader["Edad"]) : 0,
                            Telefono = reader["Telefono"] != DBNull.Value ? reader["Telefono"].ToString() : "",
                            Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                            Direccion = reader["Direccion"] != DBNull.Value ? reader["Direccion"].ToString() : "",
                            Estado = reader["Estado"] != DBNull.Value ? reader["Estado"].ToString() : "",
                            Observaciones = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : "",
                            Eliminado = reader["Eliminado"] != DBNull.Value && Convert.ToBoolean(reader["Eliminado"])
                        });
                    }
                }

                return lista;
            }

            public void Insertar(Cliente cliente)
            {
                ConexionBD conexionBD = new ConexionBD();
                using (SqlConnection con = conexionBD.ObtenerConexion())
                {
                    string query = @"INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES (@Nombre, @Apellido, @DNI, @Edad, @Telefono, @Email, @Direccion, @Estado, @Observaciones)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", cliente.Apellido);
                    cmd.Parameters.AddWithValue("@DNI", cliente.DNI);
                    cmd.Parameters.AddWithValue("@Edad", cliente.Edad);
                    cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                    cmd.Parameters.AddWithValue("@Email", cliente.Email);
                    cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                    cmd.Parameters.AddWithValue("@Estado", cliente.Estado); // 👈 agregado
                    cmd.Parameters.AddWithValue("@Observaciones", cliente.Observaciones ?? "");

                    cmd.ExecuteNonQuery();
                }
            }
            public void Modificar(Cliente cliente)
            {
                ConexionBD conexionBD = new ConexionBD();
                using (var con = conexionBD.ObtenerConexion())
                {
                    string query = @"UPDATE Cliente SET 
Nombre = @Nombre,
Apellido = @Apellido,
DNI = @DNI,
Edad = @Edad,
Telefono = @Telefono,
Email = @Email,
Direccion = @Direccion,
Estado = @Estado,
Observaciones = @Observaciones
WHERE Id = @Id";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", cliente.Apellido);
                    cmd.Parameters.AddWithValue("@DNI", cliente.DNI);
                    cmd.Parameters.AddWithValue("@Edad", cliente.Edad);
                    cmd.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                    cmd.Parameters.AddWithValue("@Email", cliente.Email);
                    cmd.Parameters.AddWithValue("@Direccion", cliente.Direccion);
                    cmd.Parameters.AddWithValue("@Id", cliente.Id);
                    cmd.Parameters.AddWithValue("@Estado", cliente.Estado);
                    cmd.Parameters.AddWithValue("@Observaciones", cliente.Observaciones ?? "");

                    cmd.ExecuteNonQuery();
                }
            }

            public bool ExisteDNI(string dni, int idExcluir = 0)
            {
                ConexionBD conexionBD = new ConexionBD();
                using (SqlConnection con = conexionBD.ObtenerConexion())
                {
                    string query = "SELECT COUNT(*) FROM Cliente WHERE DNI = @DNI AND Id <> @IdExcluir";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@DNI", dni);
                    cmd.Parameters.AddWithValue("@IdExcluir", idExcluir);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }

            public Cliente ObtenerPorId(int id)
            {
                Cliente cliente = null;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM Cliente WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", id);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            DNI = reader["DNI"].ToString(),
                            Edad = reader["Edad"] != DBNull.Value ? Convert.ToInt32(reader["Edad"]) : 0,
                            Telefono = reader["Telefono"].ToString(),
                            Email = reader["Email"].ToString(),
                            Direccion = reader["Direccion"].ToString(),
                            Estado = reader["Estado"].ToString(),
                            Observaciones = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : ""

                        };
                    }
                }

                return cliente;
            }
            public void Eliminar(int id)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    // Baja lógica: no se toca el Estado real del cliente (Activo/Suspendido/etc.),
                    // así al restaurar vuelve exactamente como estaba antes de darlo de baja.
                    string query = "UPDATE Cliente SET Eliminado = 1 WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();
                }
            }

            public void Restaurar(int id)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "UPDATE Cliente SET Eliminado = 0 WHERE Id = @Id";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();
                }
            }

            public List<Cliente> ObtenerInactivos()
            {
                var lista = new List<Cliente>();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT * FROM Cliente WHERE Eliminado = 1";
                    SqlCommand cmd = new SqlCommand(query, con);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Cliente
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Apellido = reader["Apellido"].ToString(),
                            DNI = reader["DNI"].ToString(),
                            Edad = reader["Edad"] != DBNull.Value ? Convert.ToInt32(reader["Edad"]) : 0,
                            Telefono = reader["Telefono"].ToString(),
                            Email = reader["Email"].ToString(),
                            Direccion = reader["Direccion"].ToString(),
                            Estado = reader["Estado"].ToString(),
                            Observaciones = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : ""
                        });
                    }
                }
                return lista;
            }

        }

    }
