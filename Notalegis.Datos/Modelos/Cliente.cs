using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notalegis.Datos.Modelos
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string DNI { get; set; }
        public int Edad { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
        public bool Eliminado { get; set; }
        public string NombreCompleto => string.IsNullOrWhiteSpace(DNI)
            ? $"{Nombre} {Apellido}".Trim()
            : $"{Nombre} {Apellido} - DNI: {DNI}";
    }

}