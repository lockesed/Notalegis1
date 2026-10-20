using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notalegis.Datos.Modelos
{
    public class Cobro
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string NumeroExpediente { get; set; }
        public DateTime FechaCobro { get; set; }
        public string TipoTramite { get; set; }

        // Impuestos y Honorarios (Sección 1)
        public decimal ImpGananciasPorc { get; set; }
        public decimal ImpGananciasBase { get; set; }
        public decimal ImpGananciasImporte { get; set; }

        public decimal ITIPorc { get; set; }
        public decimal ITIBase { get; set; }
        public decimal ITIImporte { get; set; }

        public decimal ImpSellosPorc { get; set; }
        public decimal ImpSellosBase { get; set; }
        public decimal ImpSellosImporte { get; set; }

        public decimal TasaGeneralPorc { get; set; }
        public decimal TasaGeneralBase { get; set; }
        public decimal TasaGeneralImporte { get; set; }

        public decimal SelloMatrizPorc { get; set; }
        public decimal SelloMatrizBase { get; set; }
        public decimal SelloMatrizImporte { get; set; }

        public decimal CertificadosPorc { get; set; }
        public decimal CertificadosBase { get; set; }
        public decimal CertificadosImporte { get; set; }

        public decimal DiligenciamientoPorc { get; set; }
        public decimal DiligenciamientoBase { get; set; }
        public decimal DiligenciamientoImporte { get; set; }

        public decimal HonorariosPorc { get; set; }
        public decimal HonorariosBase { get; set; }
        public decimal HonorariosImporte { get; set; }

        public decimal IVAPorc { get; set; }
        public decimal IVABase { get; set; }
        public decimal IVAImporte { get; set; }

        // Escrituras y Documentos
        public decimal EscrituraCompraventa { get; set; }
        public decimal ConstitucionSociedad { get; set; }
        public decimal PoderGeneral { get; set; }
        public decimal CertificadoDominio { get; set; }
        public decimal Testamento { get; set; }
        public decimal Donacion { get; set; }
        public decimal Hipoteca { get; set; }
        public decimal CancelacionHipoteca { get; set; }
        public decimal Usufructo { get; set; }
        public decimal Servidumbre { get; set; }

        // Otra gestión (trámite personalizado, no predefinido)
        public string OtraGestionNombre { get; set; }
        public decimal OtraGestionImporte { get; set; }

        // Totales
        public decimal SubtotalImpuestos { get; set; }
        public decimal SubtotalCertificados { get; set; }
        public decimal TotalGeneral { get; set; }

        // Información adicional
        public string Estado { get; set; } = "Pendiente";
        public string MetodoPago { get; set; }
        public string Observaciones { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public string UsuarioCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }

        // Propiedades calculadas
        public string EstadoDescripcion
        {
            get
            {
                return Estado switch
                {
                    "Pendiente" => "Pendiente de Pago",
                    "Pagado" => "Pagado Completo",
                    "Parcial" => "Pago Parcial",
                    _ => Estado
                };
            }
        }

        public string FechaCobroFormateada => FechaCobro.ToString("dd/MM/yyyy");
        public string TotalFormateado => TotalGeneral.ToString("C");
        
        // Propiedad para mostrar el nombre del cliente en el DataGrid
        public string Cliente { get; set; }
    }
}
