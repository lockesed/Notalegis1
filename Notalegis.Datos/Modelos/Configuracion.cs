using System;

namespace Notalegis.Datos.Modelos
{
    public class Configuracion
    {
        public int Id { get; set; }
        public string Clave { get; set; }
        public string Valor { get; set; }
        public string TipoConfiguracion { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
