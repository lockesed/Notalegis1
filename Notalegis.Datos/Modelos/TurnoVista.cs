using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notalegis.Datos.Modelos
{
    public class TurnoVista
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string TipoTramite { get; set; }
        public string Estado { get; set; }
        public bool Confirmado { get; set; }
    }

}
