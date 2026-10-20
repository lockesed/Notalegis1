using System;
using System.Windows;
using System.Windows.Controls;
using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;

namespace NotalegisWPF.Views
{
    public partial class FacturaModal : Window
    {
        public Cliente Cliente { get; set; }
        public decimal ImporteTotalValue { get; set; }
        public string NumeroExpediente { get; set; }
        public bool GenerarPresupuesto { get; private set; }

        // Propiedades para los datos del escribano
        public string EscribanoNombreValue => EscribanoNombre.Text;
        public string EscribanoDireccionValue => EscribanoDireccion.Text;
        public string EscribanoLocalidadValue => EscribanoLocalidad.Text;
        public string EscribanoMatriculaValue => EscribanoMatricula.Text;

        // Propiedades para datos del cliente y operación
        public DateTime? FechaFacturaValue => FechaFactura.SelectedDate;
        public string TipoOperacionValue => TipoOperacion.Text;
        public string ObservacionesFacturaValue => ObservacionesFactura.Text;

        // Propiedades para montos principales
        public decimal ImporteOperacionValue => decimal.TryParse(ImporteOperacion.Text, out decimal imp) ? imp : 0;

        // Propiedades para conceptos gravados
        public string ConceptoFojaElaboradaValue => ConceptoFojaElaborada.Text;
        public decimal PrecioFojaElaboradaValue => decimal.TryParse(PrecioFojaElaborada.Text, out decimal pf) ? pf : 0;
        public int CantidadFojaElaboradaValue => int.TryParse(CantidadFojaElaborada.Text, out int cf) ? cf : 0;

        public string ConceptoAporteNotarialValue => ConceptoAporteNotarial.Text;
        public decimal PrecioAporteNotarialValue => decimal.TryParse(PrecioAporteNotarial.Text, out decimal pa) ? pa : 0;
        public int CantidadAporteNotarialValue => int.TryParse(CantidadAporteNotarial.Text, out int ca) ? ca : 0;

        public string ConceptoHonorariosValue => ConceptoHonorarios.Text;
        public decimal PrecioHonorariosValue => decimal.TryParse(PrecioHonorarios.Text, out decimal ph) ? ph : 0;
        public int CantidadHonorariosValue => int.TryParse(CantidadHonorarios.Text, out int ch) ? ch : 0;

        public FacturaModal()
        {
            InitializeComponent();
        }

        public FacturaModal(Cliente cliente, decimal importeTotal, string numeroExpediente)
        {
            InitializeComponent();
            Cliente = cliente;
            ImporteTotalValue = importeTotal;
            NumeroExpediente = numeroExpediente;
            ClienteNombre.Text = cliente.NombreCompleto;
            ImporteOperacion.Text = importeTotal.ToString("N2", System.Globalization.CultureInfo.CurrentCulture);
            FechaFactura.SelectedDate = DateTime.Today;

            CargarDatosDelEscribanoDesdeConfiguracion();
        }

        private void CargarDatosDelEscribanoDesdeConfiguracion()
        {
            var config = new ConfiguracionService();

            EscribanoNombre.Text = config.ObtenerValorString(ClavesConfiguracion.EscribaniaEscribano);
            EscribanoDireccion.Text = config.ObtenerValorString(ClavesConfiguracion.EscribaniaDireccion);
            EscribanoMatricula.Text = config.ObtenerValorString(ClavesConfiguracion.EscribaniaNumeroRegistro);

            string ciudad = config.ObtenerValorString(ClavesConfiguracion.EscribaniaCiudad);
            string provincia = config.ObtenerValorString(ClavesConfiguracion.EscribaniaProvincia);
            EscribanoLocalidad.Text = !string.IsNullOrWhiteSpace(ciudad) && !string.IsNullOrWhiteSpace(provincia)
                ? $"{ciudad}, {provincia}"
                : (ciudad + provincia);
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void GenerarPresupuestoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarDatos())
            {
                GenerarPresupuesto = true;
                DialogResult = true;
                Close();
            }
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(EscribanoNombre.Text))
            {
                MessageBox.Show("Debe ingresar el nombre del escribano", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                EscribanoNombre.Focus();
                return false;
            }

            if (!FechaFactura.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar la fecha del presupuesto", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                FechaFactura.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(TipoOperacion.Text))
            {
                MessageBox.Show("Debe ingresar el tipo de operación", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                TipoOperacion.Focus();
                return false;
            }

            if (ImporteOperacionValue <= 0)
            {
                MessageBox.Show("Debe ingresar un importe de operación válido", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                ImporteOperacion.Focus();
                return false;
            }

            return true;
        }
    }
}
