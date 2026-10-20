using System;



namespace Notalegis.Models
{
    public class Tramite
{
    public int Id { get; set; }
    public string Cliente { get; set; }
    public string TipoTramite { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; }
    public string Prioridad { get; set; }
    public bool DocumentacionCompleta { get; set; }
    public string Observaciones { get; set; }
    // Nuevos campos:
    public string NumeroExpediente { get; set; }
    public DateTime FechaCreacion { get; set; }
    public decimal? MontoEstimado { get; set; }
}
}
