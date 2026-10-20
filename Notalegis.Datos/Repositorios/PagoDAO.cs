using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class PagoDAO
    {
        private ConexionBD conexion;

        public PagoDAO()
        {
            conexion = new ConexionBD();
        }

        public void Insertar(Pago pago)
        {
            string query = @"INSERT INTO Pagos (CobroId, MontoPagado, FechaPago, MetodoPago, NumeroComprobante, 
                            Observaciones, UsuarioRegistro, FechaRegistro) 
                            VALUES (@CobroId, @MontoPagado, @FechaPago, @MetodoPago, @NumeroComprobante, 
                            @Observaciones, @UsuarioRegistro, @FechaRegistro)";

            try
            {
                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CobroId", pago.CobroId);
                        cmd.Parameters.AddWithValue("@MontoPagado", pago.MontoPagado);
                        cmd.Parameters.AddWithValue("@FechaPago", pago.FechaPago);
                        cmd.Parameters.AddWithValue("@MetodoPago", pago.MetodoPago ?? "");
                        cmd.Parameters.AddWithValue("@NumeroComprobante", pago.NumeroComprobante ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Observaciones", pago.Observaciones ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UsuarioRegistro", pago.UsuarioRegistro ?? "");
                        cmd.Parameters.AddWithValue("@FechaRegistro", pago.FechaRegistro);

                        // Debug: mostrar datos del pago que se va a insertar
                        System.Diagnostics.Debug.WriteLine($"PagoDAO.Insertar - CobroId: {pago.CobroId}, Monto: {pago.MontoPagado}, Fecha: {pago.FechaPago}");

                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        // Debug: confirmar inserción
                        System.Diagnostics.Debug.WriteLine($"PagoDAO.Insertar - Filas afectadas: {rowsAffected}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PagoDAO.Insertar - Error: {ex.Message}");
                throw new Exception($"Error al insertar pago: {ex.Message}", ex);
            }
        }

        public decimal ObtenerTotalPagadoPorCobro(int cobroId)
        {
            string query = "SELECT ISNULL(SUM(MontoPagado), 0) FROM Pagos WHERE CobroId = @CobroId";

            try
            {
                using (var connection = conexion.ObtenerConexion())
                {
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@CobroId", cobroId);
                        var result = cmd.ExecuteScalar();
                        decimal total = result != null ? (decimal)result : 0m;
                        
                        System.Diagnostics.Debug.WriteLine($"PagoDAO.ObtenerTotalPagadoPorCobro - CobroId: {cobroId}, Total calculado: {total}");
                        return total;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PagoDAO.ObtenerTotalPagadoPorCobro - Error para CobroId {cobroId}: {ex.Message}");
                return 0m;
            }
        }

        public List<Pago> ObtenerTodos()
        {
            List<Pago> pagos = new List<Pago>();
            string query = @"SELECT Id, CobroId, MontoPagado, FechaPago, MetodoPago, NumeroComprobante,
                            Observaciones, UsuarioRegistro, FechaRegistro
                            FROM Pagos ORDER BY FechaPago DESC";

            try
            {
                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pagos.Add(MapearPago(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return new List<Pago>();
            }
            return pagos;
        }

        public List<Pago> ObtenerPorCobro(int cobroId)
        {
            List<Pago> pagos = new List<Pago>();
            string query = @"SELECT Id, CobroId, MontoPagado, FechaPago, MetodoPago, NumeroComprobante, 
                            Observaciones, UsuarioRegistro, FechaRegistro 
                            FROM Pagos WHERE CobroId = @CobroId ORDER BY FechaPago DESC";

            try
            {
                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CobroId", cobroId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pagos.Add(MapearPago(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Si hay error, retornar lista vacía
                return new List<Pago>();
            }
            return pagos;
        }

        public Pago ObtenerPorId(int id)
        {
            string query = @"SELECT Id, CobroId, MontoPagado, FechaPago, MetodoPago, NumeroComprobante, 
                            Observaciones, UsuarioRegistro, FechaRegistro 
                            FROM Pagos WHERE Id = @Id";

            try
            {
                using (SqlConnection conn = conexion.ObtenerConexion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapearPago(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Si hay error, retornar null
                return null;
            }
            return null;
        }

        private Pago MapearPago(SqlDataReader reader)
        {
            return new Pago
            {
                Id = reader.GetInt32(0),
                CobroId = reader.GetInt32(1),
                MontoPagado = reader.GetDecimal(2),
                FechaPago = reader.GetDateTime(3),
                MetodoPago = reader.GetString(4),
                NumeroComprobante = reader.IsDBNull(5) ? null : reader.GetString(5),
                Observaciones = reader.IsDBNull(6) ? null : reader.GetString(6),
                UsuarioRegistro = reader.GetString(7),
                FechaRegistro = reader.GetDateTime(8)
            };
        }
    }
}
