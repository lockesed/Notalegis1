using System;
using System.IO;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Negocio.Servicios
{
    public class BackupService
    {
        private readonly BackupDAO _backupDAO;

        public BackupService()
        {
            _backupDAO = new BackupDAO();
        }

        /// <summary>
        /// Genera una copia de seguridad física de la base de datos.
        /// Retorna la ruta completa donde se guardó el archivo en el servidor.
        /// </summary>
        public string GenerarCopiaSeguridad(string rutaBase = @"C:\BackupsNotalegis")
        {
            try
            {
                // Asegurar que el directorio exista
                if (!Directory.Exists(rutaBase))
                {
                    Directory.CreateDirectory(rutaBase);
                }

                // Generar nombre de archivo con timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string nombreArchivo = $"Notalegis_Backup_{timestamp}.bak";
                string rutaCompleta = Path.Combine(rutaBase, nombreArchivo);

                // Ejecutar backup
                _backupDAO.GenerarBackup(rutaCompleta);

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                throw new Exception("Error interno al generar el backup: " + ex.Message, ex);
            }
        }
    }
}
