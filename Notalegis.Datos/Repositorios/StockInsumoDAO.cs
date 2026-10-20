using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Notalegis.Datos.Repositorios
{
    public class StockInsumoDAO
    {
        private readonly string connectionString = ConfiguracionSegura.Cadena;

        public List<StockInsumo> ObtenerTodos()
        {
            var lista = new List<StockInsumo>();
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM StockInsumo", con);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new StockInsumo
                    {
                        Id = reader.GetInt32(0),
                        TipoInsumo = reader.GetString(1),
                        CantidadActual = reader.GetInt32(2),
                        StockMinimo = reader.GetInt32(3),
                        UltimaActualizacion = reader.GetDateTime(4)
                    });
                }
            }

            return lista;
        }

        public StockInsumo ObtenerPorTipo(string tipoInsumo)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM StockInsumo WHERE TipoInsumo = @TipoInsumo", con);
                cmd.Parameters.AddWithValue("@TipoInsumo", tipoInsumo);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new StockInsumo
                    {
                        Id = reader.GetInt32(0),
                        TipoInsumo = reader.GetString(1),
                        CantidadActual = reader.GetInt32(2),
                        StockMinimo = reader.GetInt32(3),
                        UltimaActualizacion = reader.GetDateTime(4)
                    };
                }
            }

            return null;
        }

        public void ActualizarStock(string tipoInsumo, int nuevaCantidad)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                string query = @"UPDATE StockInsumo SET 
                    CantidadActual = @CantidadActual, 
                    UltimaActualizacion = @UltimaActualizacion 
                    WHERE TipoInsumo = @TipoInsumo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@TipoInsumo", tipoInsumo);
                cmd.Parameters.AddWithValue("@CantidadActual", nuevaCantidad);
                cmd.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);

                cmd.ExecuteNonQuery();
            }
        }

        public void InsertarOActualizar(StockInsumo stock)
        {
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                // Verificar si existe
                SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM StockInsumo WHERE TipoInsumo = @TipoInsumo", con);
                cmdCheck.Parameters.AddWithValue("@TipoInsumo", stock.TipoInsumo);
                int existe = (int)cmdCheck.ExecuteScalar();

                if (existe > 0)
                {
                    // Actualizar
                    string query = @"UPDATE StockInsumo SET 
                        CantidadActual = @CantidadActual, 
                        StockMinimo = @StockMinimo, 
                        UltimaActualizacion = @UltimaActualizacion 
                        WHERE TipoInsumo = @TipoInsumo";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@TipoInsumo", stock.TipoInsumo);
                    cmd.Parameters.AddWithValue("@CantidadActual", stock.CantidadActual);
                    cmd.Parameters.AddWithValue("@StockMinimo", stock.StockMinimo);
                    cmd.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Insertar
                    string query = @"INSERT INTO StockInsumo 
                        (TipoInsumo, CantidadActual, StockMinimo, UltimaActualizacion) 
                        VALUES (@TipoInsumo, @CantidadActual, @StockMinimo, @UltimaActualizacion)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@TipoInsumo", stock.TipoInsumo);
                    cmd.Parameters.AddWithValue("@CantidadActual", stock.CantidadActual);
                    cmd.Parameters.AddWithValue("@StockMinimo", stock.StockMinimo);
                    cmd.Parameters.AddWithValue("@UltimaActualizacion", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<StockInsumo> ObtenerStockBajo()
        {
            var lista = new List<StockInsumo>();
            ConexionBD conexionBD = new ConexionBD();

            using (SqlConnection con = conexionBD.ObtenerConexion())
            {
                // Solo "Hojas Protocolares" tiene una interfaz para gestionar sus pedidos y stock;
                // otros tipos (p. ej. "Sellos Fiscales") no deben generar alertas sin acción posible.
                SqlCommand cmd = new SqlCommand("SELECT * FROM StockInsumo WHERE TipoInsumo = 'Hojas Protocolares' AND CantidadActual <= StockMinimo", con);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new StockInsumo
                    {
                        Id = reader.GetInt32(0),
                        TipoInsumo = reader.GetString(1),
                        CantidadActual = reader.GetInt32(2),
                        StockMinimo = reader.GetInt32(3),
                        UltimaActualizacion = reader.GetDateTime(4)
                    });
                }
            }

            return lista;
        }
    }
}
