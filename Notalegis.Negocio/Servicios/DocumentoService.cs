using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Notalegis.Negocio.Servicios
{
    public class DocumentoService
    {
        private readonly string _basePath;

        public DocumentoService()
        {
            // Auto-create base folder in MyDocuments/Notalegis/Documentos
            string myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _basePath = Path.Combine(myDocuments, "Notalegis", "Documentos");

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public string ObtenerRutaCarpeta(string tipoEntidad, string nombreCarpeta)
        {
            // Sanitize folder name just in case
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                nombreCarpeta = nombreCarpeta.Replace(c, '_');
            }

            string ruta = Path.Combine(_basePath, tipoEntidad, nombreCarpeta);

            if (!Directory.Exists(ruta))
            {
                Directory.CreateDirectory(ruta);
            }

            return ruta;
        }

        public List<FileInfo> ListarArchivos(string tipoEntidad, string nombreCarpeta)
        {
            string ruta = ObtenerRutaCarpeta(tipoEntidad, nombreCarpeta);
            DirectoryInfo di = new DirectoryInfo(ruta);
            return di.GetFiles().OrderByDescending(f => f.CreationTime).ToList();
        }

        public string GuardarArchivo(string rutaOrigen, string tipoEntidad, string nombreCarpeta)
        {
            string rutaDestinoCarpeta = ObtenerRutaCarpeta(tipoEntidad, nombreCarpeta);
            string nombreArchivo = Path.GetFileName(rutaOrigen);
            string rutaDestinoCompleta = Path.Combine(rutaDestinoCarpeta, nombreArchivo);

            // If file exists, append timestamp to make it unique
            if (File.Exists(rutaDestinoCompleta))
            {
                string nombreSinExt = Path.GetFileNameWithoutExtension(nombreArchivo);
                string ext = Path.GetExtension(nombreArchivo);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                nombreArchivo = $"{nombreSinExt}_{timestamp}{ext}";
                rutaDestinoCompleta = Path.Combine(rutaDestinoCarpeta, nombreArchivo);
            }

            File.Copy(rutaOrigen, rutaDestinoCompleta);
            return rutaDestinoCompleta;
        }

        public void AbrirArchivo(string rutaCompleta)
        {
            if (File.Exists(rutaCompleta))
            {
                Process.Start(new ProcessStartInfo(rutaCompleta) { UseShellExecute = true });
            }
            else
            {
                throw new FileNotFoundException("El archivo no existe.");
            }
        }

        public void EliminarArchivo(string rutaCompleta)
        {
            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
            }
        }
        
        public void AbrirCarpeta(string tipoEntidad, string nombreCarpeta)
        {
             string ruta = ObtenerRutaCarpeta(tipoEntidad, nombreCarpeta);
             Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }
    }
}
