using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Notalegis.Datos.Repositorios
{
    public class PedidoInsumoDAO
    {
        private readonly string connectionString = ConfiguracionSegura.Cadena;

        public List<PedidoInsumo> ObtenerTodos()
        {
            var lista = new List<PedidoInsumo>();
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM PedidoInsumo WHERE ISNULL(Eliminado, 0) = 0 ORDER BY FechaPedido DESC", con);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new PedidoInsumo
                    {
                        Id = reader.GetInt32(0),
                        FechaPedido = reader.GetDateTime(1),
                        TipoInsumo = reader.GetString(2),
                        Cantidad = reader.GetInt32(3),
                        Solicitante = reader.GetString(4),
                        Observaciones = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Estado = reader.GetString(6),
                        EmailDestino = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Firmado = reader.GetBoolean(8),
                        FechaEnvio = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                    });
                }
            }

            return lista;
        }

        public PedidoInsumo ObtenerPorId(int id)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM PedidoInsumo WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new PedidoInsumo
                    {
                        Id = reader.GetInt32(0),
                        FechaPedido = reader.GetDateTime(1),
                        TipoInsumo = reader.GetString(2),
                        Cantidad = reader.GetInt32(3),
                        Solicitante = reader.GetString(4),
                        Observaciones = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Estado = reader.GetString(6),
                        EmailDestino = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Firmado = reader.GetBoolean(8),
                        FechaEnvio = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                    };
                }
            }

            return null;
        }

        public void Insertar(PedidoInsumo pedido)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string query = @"INSERT INTO PedidoInsumo
                    (FechaPedido, TipoInsumo, Cantidad, Solicitante, Observaciones, Estado, EmailDestino, Firmado, FechaEnvio)
                    VALUES (@FechaPedido, @TipoInsumo, @Cantidad, @Solicitante, @Observaciones, @Estado, @EmailDestino, @Firmado, @FechaEnvio)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FechaPedido", pedido.FechaPedido);
                cmd.Parameters.AddWithValue("@TipoInsumo", pedido.TipoInsumo);
                cmd.Parameters.AddWithValue("@Cantidad", pedido.Cantidad);
                cmd.Parameters.AddWithValue("@Solicitante", pedido.Solicitante);
                cmd.Parameters.AddWithValue("@Observaciones", pedido.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@Estado", pedido.Estado);
                cmd.Parameters.AddWithValue("@EmailDestino", pedido.EmailDestino ?? "");
                cmd.Parameters.AddWithValue("@Firmado", pedido.Firmado);
                cmd.Parameters.AddWithValue("@FechaEnvio", (object)pedido.FechaEnvio ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public void Modificar(PedidoInsumo pedido)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string query = @"UPDATE PedidoInsumo SET
                    FechaPedido = @FechaPedido,
                    TipoInsumo = @TipoInsumo,
                    Cantidad = @Cantidad,
                    Solicitante = @Solicitante,
                    Observaciones = @Observaciones,
                    Estado = @Estado,
                    EmailDestino = @EmailDestino,
                    Firmado = @Firmado,
                    FechaEnvio = @FechaEnvio
                    WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", pedido.Id);
                cmd.Parameters.AddWithValue("@FechaPedido", pedido.FechaPedido);
                cmd.Parameters.AddWithValue("@TipoInsumo", pedido.TipoInsumo);
                cmd.Parameters.AddWithValue("@Cantidad", pedido.Cantidad);
                cmd.Parameters.AddWithValue("@Solicitante", pedido.Solicitante);
                cmd.Parameters.AddWithValue("@Observaciones", pedido.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@Estado", pedido.Estado);
                cmd.Parameters.AddWithValue("@EmailDestino", pedido.EmailDestino ?? "");
                cmd.Parameters.AddWithValue("@Firmado", pedido.Firmado);
                cmd.Parameters.AddWithValue("@FechaEnvio", (object)pedido.FechaEnvio ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }
        }

        public void Eliminar(int id)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("UPDATE PedidoInsumo SET Eliminado = 1 WHERE Id = @Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public List<PedidoInsumo> ObtenerPorMes(int mes, int anio)
        {
            var lista = new List<PedidoInsumo>();
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string query = @"SELECT * FROM PedidoInsumo 
                    WHERE MONTH(FechaPedido) = @Mes AND YEAR(FechaPedido) = @Anio 
                    ORDER BY FechaPedido DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Mes", mes);
                cmd.Parameters.AddWithValue("@Anio", anio);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new PedidoInsumo
                    {
                        Id = reader.GetInt32(0),
                        FechaPedido = reader.GetDateTime(1),
                        TipoInsumo = reader.GetString(2),
                        Cantidad = reader.GetInt32(3),
                        Solicitante = reader.GetString(4),
                        Observaciones = reader.IsDBNull(5) ? "" : reader.GetString(5),
                        Estado = reader.GetString(6),
                        EmailDestino = reader.IsDBNull(7) ? "" : reader.GetString(7),
                        Firmado = reader.GetBoolean(8),
                        FechaEnvio = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9)
                    });
                }
            }

            return lista;
        }
    }
}
