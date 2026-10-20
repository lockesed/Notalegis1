using System;

namespace Notalegis.Datos.Modelos
{
    public class EstadoCliente
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public string Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}

