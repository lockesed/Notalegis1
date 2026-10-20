using System;

namespace Notalegis.Views
{
    public class PresupuestoPago
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string NumeroExpediente { get; set; }
        public string TipoTramite { get; set; }
        public decimal Total { get; set; }
        public decimal MontoAbonado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string EstadoPago { get; set; }
        public string MetodoPago { get; set; }
        public DateTime UltimaActualizacion { get; set; }
        public string Observaciones { get; set; }
        public decimal SubtotalImpuestos { get; set; }
        public decimal SubtotalCertificados { get; set; }
    }
}
