using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notalegis.Datos.Modelos
{
    public class Turno
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public DateTime FechaHora { get; set; }
        public string Motivo { get; set; }
        public string TipoTramite { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public bool Confirmado { get; set; }
    }
}
