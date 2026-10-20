using System;

namespace Notalegis.Datos.Modelos
{
    public class RegistroAuditoria
    {
        public int Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }     // "ALTA", "MODIFICACION", "BAJA"
        public string Modulo { get; set; }     // "Clientes", "Tramites", "Turnos", etc.
        public int? EntidadId { get; set; }
        public string Descripcion { get; set; }
    }
}
