using System;
using System.Data.SqlClient;


namespace Notalegis.Datos.Conexion
{
    public class ConexionBD
    {
        private readonly string cadenaConexion;

        public ConexionBD()
        {
            cadenaConexion = ConfiguracionSegura.Cadena;
        }

        public SqlConnection ObtenerConexion()
        {
            SqlConnection conexion = new SqlConnection(cadenaConexion);

            try
            {
                conexion.Open();
                return conexion;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar con la base de datos: " + ex.Message);
            }
        }
    }
}

