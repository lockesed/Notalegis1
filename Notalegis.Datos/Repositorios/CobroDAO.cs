using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class CobroDAO
    {
        private ConexionBD conexion;

        public CobroDAO()
        {
            conexion = new ConexionBD();
        }

        public bool Insertar(Cobro cobro)
        {
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = @"
                        INSERT INTO Cobros (
                            ClienteId, NumeroExpediente, FechaCobro, TipoTramite,
                            ImpGananciasPorc, ImpGananciasBase, ImpGananciasImporte,
                            ITIPorc, ITIBase, ITIImporte,
                            ImpSellosPorc, ImpSellosBase, ImpSellosImporte,
                            TasaGeneralPorc, TasaGeneralBase, TasaGeneralImporte,
                            SelloMatrizPorc, SelloMatrizBase, SelloMatrizImporte,
                            CertificadosPorc, CertificadosBase, CertificadosImporte,
                            DiligenciamientoPorc, DiligenciamientoBase, DiligenciamientoImporte,
                            HonorariosPorc, HonorariosBase, HonorariosImporte,
                            IVAPorc, IVABase, IVAImporte,
                            OtraGestionNombre, OtraGestionImporte,
                            SubtotalImpuestos, SubtotalCertificados, TotalGeneral,
                            Estado, MetodoPago, Observaciones, UsuarioCreacion
                        ) VALUES (
                            @ClienteId, @NumeroExpediente, @FechaCobro, @TipoTramite,
                            @ImpGananciasPorc, @ImpGananciasBase, @ImpGananciasImporte,
                            @ITIPorc, @ITIBase, @ITIImporte,
                            @ImpSellosPorc, @ImpSellosBase, @ImpSellosImporte,
                            @TasaGeneralPorc, @TasaGeneralBase, @TasaGeneralImporte,
                            @SelloMatrizPorc, @SelloMatrizBase, @SelloMatrizImporte,
                            @CertificadosPorc, @CertificadosBase, @CertificadosImporte,
                            @DiligenciamientoPorc, @DiligenciamientoBase, @DiligenciamientoImporte,
                            @HonorariosPorc, @HonorariosBase, @HonorariosImporte,
                            @IVAPorc, @IVABase, @IVAImporte,
                            @OtraGestionNombre, @OtraGestionImporte,
                            @SubtotalImpuestos, @SubtotalCertificados, @TotalGeneral,
                            @Estado, @MetodoPago, @Observaciones, @UsuarioCreacion
                        )";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        AgregarParametros(cmd, cobro);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar cobro: {ex.Message}");
            }
        }

        public List<Cobro> ObtenerHistorialPorCliente(int clienteId)
        {
            var cobros = new List<Cobro>();
            
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = @"
                        SELECT * FROM Cobros 
                        WHERE ClienteId = @ClienteId 
                        ORDER BY FechaCobro DESC";
                    
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cobros.Add(MapearCobro(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historial de cobros: {ex.Message}");
            }
            
            return cobros;
        }

        public List<Cobro> ObtenerCobrosPorRangoFechas(DateTime fechaDesde, DateTime fechaHasta)
        {
            var cobros = new List<Cobro>();
            
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = @"
                        SELECT * FROM Cobros 
                        WHERE FechaCobro >= @FechaDesde AND FechaCobro <= @FechaHasta 
                        ORDER BY FechaCobro DESC";
                    
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde);
                        cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta);
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cobros.Add(MapearCobro(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cobros por rango de fechas: {ex.Message}");
            }
            
            return cobros;
        }

        public List<Cobro> ObtenerTodos()
        {
            var cobros = new List<Cobro>();
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = "SELECT * FROM Cobros ORDER BY FechaCobro DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cobros.Add(MapearCobro(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cobros: {ex.Message}");
            }
            return cobros;
        }

        public Cobro ObtenerPorId(int id)
        {
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = "SELECT * FROM Cobros WHERE Id = @Id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapearCobro(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cobro: {ex.Message}");
            }
            return null;
        }

        public bool Actualizar(Cobro cobro)
        {
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = @"
                        UPDATE Cobros SET
                            Estado = @Estado,
                            MetodoPago = @MetodoPago,
                            Observaciones = @Observaciones,
                            FechaActualizacion = GETDATE(),
                            UsuarioActualizacion = @UsuarioActualizacion
                        WHERE Id = @Id";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", cobro.Id);
                        cmd.Parameters.AddWithValue("@Estado", cobro.Estado ?? "Pendiente");
                        cmd.Parameters.AddWithValue("@MetodoPago", cobro.MetodoPago ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Observaciones", cobro.Observaciones ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UsuarioActualizacion", cobro.UsuarioActualizacion ?? (object)DBNull.Value);
                        
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar cobro: {ex.Message}");
            }
        }

        public List<Cobro> ObtenerPorFechas(int clienteId, DateTime fechaDesde, DateTime fechaHasta)
        {
            var cobros = new List<Cobro>();
            try
            {
                using (var conn = conexion.ObtenerConexion())
                {
                    string query = @"
                        SELECT * FROM Cobros 
                        WHERE ClienteId = @ClienteId 
                        AND FechaCobro >= @FechaDesde 
                        AND FechaCobro <= @FechaHasta
                        ORDER BY FechaCobro DESC";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                        cmd.Parameters.AddWithValue("@FechaDesde", fechaDesde);
                        cmd.Parameters.AddWithValue("@FechaHasta", fechaHasta);
                        
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cobros.Add(MapearCobro(reader));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cobros por fechas: {ex.Message}");
            }
            return cobros;
        }

        private void AgregarParametros(SqlCommand cmd, Cobro cobro)
        {
            cmd.Parameters.AddWithValue("@ClienteId", cobro.ClienteId);
            cmd.Parameters.AddWithValue("@NumeroExpediente", cobro.NumeroExpediente ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FechaCobro", cobro.FechaCobro);
            cmd.Parameters.AddWithValue("@TipoTramite", cobro.TipoTramite ?? (object)DBNull.Value);

            // Impuestos y Honorarios
            cmd.Parameters.AddWithValue("@ImpGananciasPorc", cobro.ImpGananciasPorc);
            cmd.Parameters.AddWithValue("@ImpGananciasBase", cobro.ImpGananciasBase);
            cmd.Parameters.AddWithValue("@ImpGananciasImporte", cobro.ImpGananciasImporte);
            cmd.Parameters.AddWithValue("@ITIPorc", cobro.ITIPorc);
            cmd.Parameters.AddWithValue("@ITIBase", cobro.ITIBase);
            cmd.Parameters.AddWithValue("@ITIImporte", cobro.ITIImporte);
            cmd.Parameters.AddWithValue("@ImpSellosPorc", cobro.ImpSellosPorc);
            cmd.Parameters.AddWithValue("@ImpSellosBase", cobro.ImpSellosBase);
            cmd.Parameters.AddWithValue("@ImpSellosImporte", cobro.ImpSellosImporte);
            cmd.Parameters.AddWithValue("@TasaGeneralPorc", cobro.TasaGeneralPorc);
            cmd.Parameters.AddWithValue("@TasaGeneralBase", cobro.TasaGeneralBase);
            cmd.Parameters.AddWithValue("@TasaGeneralImporte", cobro.TasaGeneralImporte);
            cmd.Parameters.AddWithValue("@SelloMatrizPorc", cobro.SelloMatrizPorc);
            cmd.Parameters.AddWithValue("@SelloMatrizBase", cobro.SelloMatrizBase);
            cmd.Parameters.AddWithValue("@SelloMatrizImporte", cobro.SelloMatrizImporte);
            cmd.Parameters.AddWithValue("@CertificadosPorc", cobro.CertificadosPorc);
            cmd.Parameters.AddWithValue("@CertificadosBase", cobro.CertificadosBase);
            cmd.Parameters.AddWithValue("@CertificadosImporte", cobro.CertificadosImporte);
            cmd.Parameters.AddWithValue("@DiligenciamientoPorc", cobro.DiligenciamientoPorc);
            cmd.Parameters.AddWithValue("@DiligenciamientoBase", cobro.DiligenciamientoBase);
            cmd.Parameters.AddWithValue("@DiligenciamientoImporte", cobro.DiligenciamientoImporte);
            cmd.Parameters.AddWithValue("@HonorariosPorc", cobro.HonorariosPorc);
            cmd.Parameters.AddWithValue("@HonorariosBase", cobro.HonorariosBase);
            cmd.Parameters.AddWithValue("@HonorariosImporte", cobro.HonorariosImporte);
            cmd.Parameters.AddWithValue("@IVAPorc", cobro.IVAPorc);
            cmd.Parameters.AddWithValue("@IVABase", cobro.IVABase);
            cmd.Parameters.AddWithValue("@IVAImporte", cobro.IVAImporte);

            // Otra gestión
            cmd.Parameters.AddWithValue("@OtraGestionNombre", cobro.OtraGestionNombre ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@OtraGestionImporte", cobro.OtraGestionImporte);

            // Totales
            cmd.Parameters.AddWithValue("@SubtotalImpuestos", cobro.SubtotalImpuestos);
            cmd.Parameters.AddWithValue("@SubtotalCertificados", cobro.SubtotalCertificados);
            cmd.Parameters.AddWithValue("@TotalGeneral", cobro.TotalGeneral);

            // Información adicional
            cmd.Parameters.AddWithValue("@Estado", cobro.Estado ?? "Pendiente");
            cmd.Parameters.AddWithValue("@MetodoPago", cobro.MetodoPago ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Observaciones", cobro.Observaciones ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UsuarioCreacion", cobro.UsuarioCreacion ?? (object)DBNull.Value);
        }

        private Cobro MapearCobro(SqlDataReader reader)
        {
            return new Cobro
            {
                Id = reader.GetInt32("Id"),
                ClienteId = reader.GetInt32("ClienteId"),
                NumeroExpediente = reader.IsDBNull("NumeroExpediente") ? null : reader.GetString("NumeroExpediente"),
                FechaCobro = reader.GetDateTime("FechaCobro"),
                TipoTramite = reader.IsDBNull("TipoTramite") ? null : reader.GetString("TipoTramite"),

                // Impuestos y Honorarios
                ImpGananciasPorc = reader.GetDecimal("ImpGananciasPorc"),
                ImpGananciasBase = reader.GetDecimal("ImpGananciasBase"),
                ImpGananciasImporte = reader.GetDecimal("ImpGananciasImporte"),
                ITIPorc = reader.GetDecimal("ITIPorc"),
                ITIBase = reader.GetDecimal("ITIBase"),
                ITIImporte = reader.GetDecimal("ITIImporte"),
                ImpSellosPorc = reader.GetDecimal("ImpSellosPorc"),
                ImpSellosBase = reader.GetDecimal("ImpSellosBase"),
                ImpSellosImporte = reader.GetDecimal("ImpSellosImporte"),
                TasaGeneralPorc = reader.GetDecimal("TasaGeneralPorc"),
                TasaGeneralBase = reader.GetDecimal("TasaGeneralBase"),
                TasaGeneralImporte = reader.GetDecimal("TasaGeneralImporte"),
                SelloMatrizPorc = reader.GetDecimal("SelloMatrizPorc"),
                SelloMatrizBase = reader.GetDecimal("SelloMatrizBase"),
                SelloMatrizImporte = reader.GetDecimal("SelloMatrizImporte"),
                CertificadosPorc = reader.GetDecimal("CertificadosPorc"),
                CertificadosBase = reader.GetDecimal("CertificadosBase"),
                CertificadosImporte = reader.GetDecimal("CertificadosImporte"),
                DiligenciamientoPorc = reader.GetDecimal("DiligenciamientoPorc"),
                DiligenciamientoBase = reader.GetDecimal("DiligenciamientoBase"),
                DiligenciamientoImporte = reader.GetDecimal("DiligenciamientoImporte"),
                HonorariosPorc = reader.GetDecimal("HonorariosPorc"),
                HonorariosBase = reader.GetDecimal("HonorariosBase"),
                HonorariosImporte = reader.GetDecimal("HonorariosImporte"),
                IVAPorc = reader.GetDecimal("IVAPorc"),
                IVABase = reader.GetDecimal("IVABase"),
                IVAImporte = reader.GetDecimal("IVAImporte"),

                // Otra gestión
                OtraGestionNombre = reader.IsDBNull("OtraGestionNombre") ? null : reader.GetString("OtraGestionNombre"),
                OtraGestionImporte = reader.IsDBNull("OtraGestionImporte") ? 0 : reader.GetDecimal("OtraGestionImporte"),

                // Totales
                SubtotalImpuestos = reader.GetDecimal("SubtotalImpuestos"),
                SubtotalCertificados = reader.GetDecimal("SubtotalCertificados"),
                TotalGeneral = reader.GetDecimal("TotalGeneral"),

                // Información adicional
                Estado = reader.IsDBNull("Estado") ? "Pendiente" : reader.GetString("Estado"),
                MetodoPago = reader.IsDBNull("MetodoPago") ? null : reader.GetString("MetodoPago"),
                Observaciones = reader.IsDBNull("Observaciones") ? null : reader.GetString("Observaciones"),

                // Auditoría
                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                FechaActualizacion = reader.IsDBNull("FechaActualizacion") ? null : reader.GetDateTime("FechaActualizacion"),
                UsuarioCreacion = reader.IsDBNull("UsuarioCreacion") ? null : reader.GetString("UsuarioCreacion"),
                UsuarioActualizacion = reader.IsDBNull("UsuarioActualizacion") ? null : reader.GetString("UsuarioActualizacion")
            };
        }
    }
}
