using System;

namespace Notalegis.Datos.Modelos
{
    public class Pago
    {
        public int Id { get; set; }
        public int CobroId { get; set; }
        public decimal MontoPagado { get; set; }
        public DateTime FechaPago { get; set; }
        public string MetodoPago { get; set; }
        public string NumeroComprobante { get; set; }
        public string Observaciones { get; set; }
        public string UsuarioRegistro { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedades calculadas para la UI
        public string FechaPagoFormateada => FechaPago.ToString("dd/MM/yyyy");
        public string MontoFormateado => MontoPagado.ToString("C");
        public string FechaRegistroFormateada => FechaRegistro.ToString("dd/MM/yyyy HH:mm");
    }
}
