using System;

namespace Notalegis.Datos.Modelos
{
    public class PedidoInsumo
    {
        public int Id { get; set; }
        public DateTime FechaPedido { get; set; }
        public string TipoInsumo { get; set; } // "Hojas Protocolares", "Sellos", etc.
        public int Cantidad { get; set; }
        public string Solicitante { get; set; } // Nombre del escribano que solicita
        public string Observaciones { get; set; }
        public string Estado { get; set; } // "Pendiente", "Enviado", "Recibido", "Cancelado"
        public string EmailDestino { get; set; } // Email del colegio de escribanos
        public bool Firmado { get; set; }
        public DateTime? FechaEnvio { get; set; }
    }
}
