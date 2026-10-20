using System;

namespace Notalegis.Datos.Modelos
{
    public class Escritura
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int TipoEscrituraId { get; set; }
        public string NumeroProtocolo { get; set; }
        public DateTime Fecha { get; set; }
        public string RutaArchivo { get; set; }
        public string FormatoArchivo { get; set; }
        public string Estado { get; set; }
        public int? TramiteId { get; set; }
        public int? CobroId { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaProtocolizacion { get; set; }
        public string UsuarioCreacion { get; set; }
        
        // Propiedades de navegación (para mostrar en UI)
        public string NombreCliente { get; set; }
        public string ClienteDNI { get; set; }
        public string TipoEscritura { get; set; }
        
        // Propiedades calculadas
        public string EstadoDescripcion
        {
            get
            {
                return Estado switch
                {
                    "Borrador" => "📝 Borrador",
                    "Finalizada" => "✅ Finalizada",
                    "Protocolizada" => "📋 Protocolizada",
                    "Archivada" => "📦 Archivada",
                    _ => Estado
                };
            }
        }
        
        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy");
        public bool TieneArchivo => !string.IsNullOrEmpty(RutaArchivo);
    }
}
