using System;

namespace Notalegis.Datos.Modelos
{
    public class StockInsumo
    {
        public int Id { get; set; }
        public string TipoInsumo { get; set; } // "Hojas Protocolares", "Sellos", etc.
        public int CantidadActual { get; set; }
        public int StockMinimo { get; set; } // Para alertas
        public DateTime UltimaActualizacion { get; set; }
    }
}
