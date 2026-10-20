using System.Windows;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Views
{
    public partial class HistorialPagosWindow : Window
    {
        private readonly PagoDAO pagoDAO = new PagoDAO();

        public HistorialPagosWindow(PresupuestoPago presupuesto)
        {
            InitializeComponent();

            ExpedienteHeaderText.Text = $"{presupuesto.ClienteNombre} — Expediente: {presupuesto.NumeroExpediente}";

            var pagos = pagoDAO.ObtenerPorCobro(presupuesto.Id);
            dgPagos.ItemsSource = pagos;
            borderSinPagos.Visibility = pagos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
