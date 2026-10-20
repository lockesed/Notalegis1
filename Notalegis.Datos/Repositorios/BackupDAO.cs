using System;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;

namespace Notalegis.Datos.Repositorios
{
    public class BackupDAO
    {
        private readonly ConexionBD _conexionBD;

        public BackupDAO()
        {
            _conexionBD = new ConexionBD();
        }

        public void GenerarBackup(string rutaDestino)
        {
            string query = @"
                BACKUP DATABASE [NotalegisDB] 
                TO DISK = @RutaDestino 
                WITH FORMAT, INIT, NAME = 'Full Backup NotalegisDB';";

            using (SqlConnection conexion = _conexionBD.ObtenerConexion())
            {
                using (SqlCommand command = new SqlCommand(query, conexion))
                {
                    // Dar un timeout amplio por si la base de datos es grande
                    command.CommandTimeout = 300; 
                    command.Parameters.AddWithValue("@RutaDestino", rutaDestino);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
