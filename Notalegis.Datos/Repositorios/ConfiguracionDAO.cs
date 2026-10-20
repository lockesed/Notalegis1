using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class ConfiguracionDAO
    {
        private readonly ConexionBD _conexionBD = new ConexionBD();

        public List<Configuracion> ObtenerTodas()
        {
            var configuraciones = new List<Configuracion>();
            try
            {
                using (var cn = _conexionBD.ObtenerConexion())
                {
                    string query = "SELECT Id, Clave, Valor, TipoConfiguracion, Descripcion, FechaActualizacion FROM Configuracion ORDER BY TipoConfiguracion, Clave";
                    
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                configuraciones.Add(MapearDesdeReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener las configuraciones: " + ex.Message, ex);
            }
            return configuraciones;
        }

        public Configuracion ObtenerPorClave(string clave)
        {
            Configuracion configuracion = null;
            try
            {
                using (var cn = _conexionBD.ObtenerConexion())
                {
                    string query = "SELECT Id, Clave, Valor, TipoConfiguracion, Descripcion, FechaActualizacion FROM Configuracion WHERE Clave = @Clave";
                    
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                configuracion = MapearDesdeReader(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener la configuración con clave {clave}: " + ex.Message, ex);
            }
            return configuracion;
        }

        public string ObtenerValorPorClave(string clave, string valorPorDefecto = "")
        {
            try
            {
                using (var cn = _conexionBD.ObtenerConexion())
                {
                    string query = "SELECT Valor FROM Configuracion WHERE Clave = @Clave";
                    
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        var result = cmd.ExecuteScalar();
                        
                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de error, preferimos devolver el valor por defecto antes que romper la aplicación
                System.Diagnostics.Debug.WriteLine($"Error al obtener valor para clave {clave}: " + ex.Message);
            }
            return valorPorDefecto;
        }

        public List<Configuracion> ObtenerPorCategoria(string tipoConfiguracion)
        {
            var configuraciones = new List<Configuracion>();
            try
            {
                using (var cn = _conexionBD.ObtenerConexion())
                {
                    string query = "SELECT Id, Clave, Valor, TipoConfiguracion, Descripcion, FechaActualizacion FROM Configuracion WHERE TipoConfiguracion = @TipoConfiguracion ORDER BY Clave";
                    
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@TipoConfiguracion", tipoConfiguracion);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                configuraciones.Add(MapearDesdeReader(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener configuraciones por categoría {tipoConfiguracion}: " + ex.Message, ex);
            }
            return configuraciones;
        }

        public void ActualizarConfiguracion(Configuracion configuracion, SqlTransaction transaction = null)
        {
            string query = @"
IF EXISTS (SELECT 1 FROM Configuracion WHERE Clave = @Clave)
BEGIN
    UPDATE Configuracion 
    SET Valor = @Valor, 
        FechaActualizacion = GETDATE()
    WHERE Clave = @Clave
END
ELSE
BEGIN
    INSERT INTO Configuracion (Clave, Valor, TipoConfiguracion, Descripcion, FechaActualizacion)
    VALUES (@Clave, @Valor, @TipoConfiguracion, @Descripcion, GETDATE())
END";
                           
            if (transaction != null)
            {
                using (var cmd = new SqlCommand(query, transaction.Connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@Valor", (object)configuracion.Valor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Clave", configuracion.Clave);
                    cmd.Parameters.AddWithValue("@TipoConfiguracion", (object)configuracion.TipoConfiguracion ?? "General");
                    cmd.Parameters.AddWithValue("@Descripcion", (object)configuracion.Descripcion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (var cn = _conexionBD.ObtenerConexion())
                {
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@Valor", (object)configuracion.Valor ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Clave", configuracion.Clave);
                        cmd.Parameters.AddWithValue("@TipoConfiguracion", (object)configuracion.TipoConfiguracion ?? "General");
                        cmd.Parameters.AddWithValue("@Descripcion", (object)configuracion.Descripcion ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void ActualizarConfiguraciones(List<Configuracion> configuraciones)
        {
            using (var cn = _conexionBD.ObtenerConexion())
            {
                using (var transaction = cn.BeginTransaction())
                {
                    try
                    {
                        foreach (var conf in configuraciones)
                        {
                            ActualizarConfiguracion(conf, transaction);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Error al actualizar la lista de configuraciones: " + ex.Message, ex);
                    }
                }
            }
        }

        private Configuracion MapearDesdeReader(SqlDataReader reader)
        {
            return new Configuracion
            {
                Id = Convert.ToInt32(reader["Id"]),
                Clave = reader["Clave"].ToString(),
                Valor = reader["Valor"] != DBNull.Value ? reader["Valor"].ToString() : null,
                TipoConfiguracion = reader["TipoConfiguracion"].ToString(),
                Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : null,
                FechaActualizacion = Convert.ToDateTime(reader["FechaActualizacion"])
            };
        }
    }
}
