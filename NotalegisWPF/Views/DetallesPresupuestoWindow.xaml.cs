using System;
using System.Windows;
using System.Windows.Media;

namespace Notalegis.Views
{
    public partial class DetallesPresupuestoWindow : Window
    {
        private PresupuestoPago presupuesto;

        public DetallesPresupuestoWindow(PresupuestoPago presupuesto)
        {
            InitializeComponent();
            this.presupuesto = presupuesto;
            InicializarVentana();
        }

        private void InicializarVentana()
        {
            // Header
            ExpedienteHeaderText.Text = $"Expediente: {presupuesto.NumeroExpediente}";

            // Información del cliente
            ClienteText.Text = presupuesto.ClienteNombre;
            ExpedienteText.Text = presupuesto.NumeroExpediente;
            TipoTramiteText.Text = presupuesto.TipoTramite;

            // Resumen financiero
            TotalPresupuestoText.Text = presupuesto.Total.ToString("C");
            MontoAbonadoText.Text = presupuesto.MontoAbonado.ToString("C");
            SaldoPendienteText.Text = presupuesto.SaldoPendiente.ToString("C");
            SubtotalImpuestosText.Text = presupuesto.SubtotalImpuestos.ToString("C");
            SubtotalCertificadosText.Text = presupuesto.SubtotalCertificados.ToString("C");

            // Estado con color
            EstadoText.Text = presupuesto.EstadoPago.ToUpper();
            switch (presupuesto.EstadoPago)
            {
                case "Pagado":
                    EstadoBorder.Background = Brushes.LightGreen;
                    break;
                case "Parcial":
                    EstadoBorder.Background = Brushes.LightYellow;
                    break;
                default:
                    EstadoBorder.Background = Brushes.LightCoral;
                    break;
            }

            // Detalles de pago
            MetodoPagoText.Text = string.IsNullOrEmpty(presupuesto.MetodoPago) ? "No especificado" : presupuesto.MetodoPago;
            FechaPresupuestoText.Text = presupuesto.Fecha.ToString("dd/MM/yyyy");
            UltimaActualizacionText.Text = presupuesto.UltimaActualizacion.ToString("dd/MM/yyyy HH:mm");

            // Observaciones
            ObservacionesText.Text = string.IsNullOrEmpty(presupuesto.Observaciones) ? "Sin observaciones" : presupuesto.Observaciones;
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Configuraciones adicionales al cargar
        }
    }
}
