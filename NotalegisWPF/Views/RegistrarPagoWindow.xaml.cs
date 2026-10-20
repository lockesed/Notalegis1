using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Notalegis.Views
{
    public partial class RegistrarPagoWindow : Window
    {
        public PresupuestoPago PresupuestoOriginal { get; private set; }
        public PresupuestoPago PresupuestoActualizado { get; private set; }
        public decimal MontoNuevoPago { get; private set; }
        public DateTime FechaNuevoPago { get; private set; }
        public string ObservacionesNuevoPago { get; private set; }

        public RegistrarPagoWindow(PresupuestoPago presupuesto)
        {
            InitializeComponent();
            PresupuestoOriginal = presupuesto;
            PresupuestoActualizado = ClonePresupuesto(presupuesto);
            InicializarVentana();
        }

        private PresupuestoPago ClonePresupuesto(PresupuestoPago original)
        {
            return new PresupuestoPago
            {
                Id = original.Id,
                ClienteId = original.ClienteId,
                ClienteNombre = original.ClienteNombre,
                Fecha = original.Fecha,
                NumeroExpediente = original.NumeroExpediente,
                TipoTramite = original.TipoTramite,
                Total = original.Total,
                MontoAbonado = original.MontoAbonado,
                SaldoPendiente = original.SaldoPendiente,
                EstadoPago = original.EstadoPago,
                MetodoPago = original.MetodoPago,
                UltimaActualizacion = original.UltimaActualizacion,
                Observaciones = original.Observaciones,
                SubtotalImpuestos = original.SubtotalImpuestos,
                SubtotalCertificados = original.SubtotalCertificados
            };
        }

        private void InicializarVentana()
        {
            // Header
            ExpedienteHeaderText.Text = $"Expediente: {PresupuestoOriginal.NumeroExpediente}";

            // Información del presupuesto
            ClienteText.Text = PresupuestoOriginal.ClienteNombre;
            TipoTramiteText.Text = PresupuestoOriginal.TipoTramite;
            TotalPresupuestoText.Text = PresupuestoOriginal.Total.ToString("C");
            SaldoActualText.Text = PresupuestoOriginal.SaldoPendiente.ToString("C");

            // Configurar fecha por defecto
            FechaPagoDatePicker.SelectedDate = DateTime.Now;

            // Configurar método de pago por defecto si ya existe
            if (!string.IsNullOrEmpty(PresupuestoOriginal.MetodoPago))
            {
                foreach (ComboBoxItem item in MetodoPagoComboBox.Items)
                {
                    if (item.Content.ToString() == PresupuestoOriginal.MetodoPago)
                    {
                        MetodoPagoComboBox.SelectedItem = item;
                        break;
                    }
                }
            }

            ActualizarResumen();
        }

        private void MontoPagoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidarFormulario();
            ActualizarResumen();
        }

        private void MetodoPagoComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidarFormulario();
        }

        private void ActualizarResumen()
        {
            // Mostrar monto actual abonado
            MontoActualAbonadoText.Text = PresupuestoOriginal.MontoAbonado.ToString("C");

            // Obtener nuevo monto
            decimal nuevoMonto = 0;
            if (decimal.TryParse(MontoPagoTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out nuevoMonto))
            {
                NuevoMontoText.Text = nuevoMonto.ToString("C");

                // Calcular totales
                var totalQuedaraAbonado = PresupuestoOriginal.MontoAbonado + nuevoMonto;
                var saldoRestante = PresupuestoOriginal.Total - totalQuedaraAbonado;

                TotalQuedaraAbonadoText.Text = totalQuedaraAbonado.ToString("C");
                SaldoRestanteText.Text = saldoRestante.ToString("C");

                // Determinar nuevo estado
                string nuevoEstado;
                Brush colorEstado;

                if (saldoRestante <= 0)
                {
                    nuevoEstado = "PAGADO";
                    colorEstado = Brushes.LightGreen;
                }
                else if (totalQuedaraAbonado > 0)
                {
                    nuevoEstado = "PARCIAL";
                    colorEstado = Brushes.LightYellow;
                }
                else
                {
                    nuevoEstado = "PENDIENTE";
                    colorEstado = Brushes.LightCoral;
                }

                NuevoEstadoText.Text = nuevoEstado;
                NuevoEstadoBorder.Background = colorEstado;
            }
            else
            {
                NuevoMontoText.Text = "$ 0,00";
                TotalQuedaraAbonadoText.Text = PresupuestoOriginal.MontoAbonado.ToString("C");
                SaldoRestanteText.Text = PresupuestoOriginal.SaldoPendiente.ToString("C");
                NuevoEstadoText.Text = PresupuestoOriginal.EstadoPago.ToUpper();
                
                // Color según estado actual
                switch (PresupuestoOriginal.EstadoPago)
                {
                    case "Pagado":
                        NuevoEstadoBorder.Background = Brushes.LightGreen;
                        break;
                    case "Parcial":
                        NuevoEstadoBorder.Background = Brushes.LightYellow;
                        break;
                    default:
                        NuevoEstadoBorder.Background = Brushes.LightCoral;
                        break;
                }
            }
        }

        private void ValidarFormulario()
        {
            bool esValido = true;
            string motivoInvalido = "";

            // Validar monto con cultura específica
            if (string.IsNullOrWhiteSpace(MontoPagoTextBox.Text))
            {
                esValido = false;
                motivoInvalido = "Campo vacío";
            }
            else if (!decimal.TryParse(MontoPagoTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto))
            {
                esValido = false;
                motivoInvalido = "No es un número válido";
                MontoPagoTextBox.BorderBrush = Brushes.Red;
            }
            else if (monto <= 0)
            {
                esValido = false;
                motivoInvalido = "Monto debe ser mayor a 0";
                MontoPagoTextBox.BorderBrush = Brushes.Red;
            }
            else if (monto > PresupuestoOriginal.SaldoPendiente)
            {
                esValido = false;
                motivoInvalido = "Excede saldo pendiente";
                MontoPagoTextBox.BorderBrush = Brushes.Red;
            }
            else
            {
                MontoPagoTextBox.ClearValue(BorderBrushProperty);
            }

            // Validar método de pago
            if (MetodoPagoComboBox.SelectedItem == null || MetodoPagoComboBox.SelectedIndex < 0)
            {
                esValido = false;
                if (string.IsNullOrEmpty(motivoInvalido)) motivoInvalido = "Método de pago no seleccionado";
            }

            // Validar fecha
            if (!FechaPagoDatePicker.SelectedDate.HasValue)
            {
                esValido = false;
                if (string.IsNullOrEmpty(motivoInvalido)) motivoInvalido = "Fecha no seleccionada";
            }

            GuardarButton.IsEnabled = esValido;
            
            // Debug: mostrar estado de validación detallado
            var metodoPagoSeleccionado = MetodoPagoComboBox.SelectedItem?.ToString() ?? "null";
            System.Diagnostics.Debug.WriteLine($"Validación - Texto: '{MontoPagoTextBox.Text}', Saldo: {PresupuestoOriginal.SaldoPendiente}, Método: {metodoPagoSeleccionado}, Index: {MetodoPagoComboBox.SelectedIndex}, Válido: {esValido}, Motivo: {motivoInvalido}");
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidarDatos())
                    return;

                // Actualizar el presupuesto
                var nuevoMonto = decimal.Parse(MontoPagoTextBox.Text, CultureInfo.CurrentCulture);
                var totalAbonado = PresupuestoOriginal.MontoAbonado + nuevoMonto;
                var saldoRestante = PresupuestoOriginal.Total - totalAbonado;

                // Guardar el monto, fecha y observaciones del nuevo pago para usar en la base de datos
                MontoNuevoPago = nuevoMonto;
                FechaNuevoPago = FechaPagoDatePicker.SelectedDate.Value;
                ObservacionesNuevoPago = ObservacionesTextBox.Text.Trim();

                PresupuestoActualizado.MontoAbonado = totalAbonado;
                PresupuestoActualizado.SaldoPendiente = saldoRestante;
                PresupuestoActualizado.UltimaActualizacion = DateTime.Now;

                // Determinar nuevo estado
                if (saldoRestante <= 0)
                {
                    PresupuestoActualizado.EstadoPago = "Pagado";
                }
                else if (totalAbonado > 0)
                {
                    PresupuestoActualizado.EstadoPago = "Parcial";
                }
                else
                {
                    PresupuestoActualizado.EstadoPago = "Pendiente";
                }

                // Actualizar método de pago y observaciones
                PresupuestoActualizado.MetodoPago = (MetodoPagoComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
                
                if (!string.IsNullOrWhiteSpace(ObservacionesTextBox.Text))
                {
                    var nuevaObservacion = $"[{DateTime.Now:dd/MM/yyyy}] Pago: {nuevoMonto:C} - {ObservacionesTextBox.Text}";
                    if (!string.IsNullOrEmpty(PresupuestoActualizado.Observaciones))
                    {
                        PresupuestoActualizado.Observaciones += " | " + nuevaObservacion;
                    }
                    else
                    {
                        PresupuestoActualizado.Observaciones = nuevaObservacion;
                    }
                }

                MessageBox.Show($"Pago registrado exitosamente.\n\nMonto: {nuevoMonto:C}\nNuevo Estado: {PresupuestoActualizado.EstadoPago}", 
                              "Pago Registrado", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar el pago:\n{ex.Message}", 
                              "Error", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private bool ValidarDatos()
        {
            // Validar monto con cultura específica
            if (!decimal.TryParse(MontoPagoTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal monto) || monto <= 0)
            {
                MessageBox.Show("Debe ingresar un monto válido mayor a cero.", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                MontoPagoTextBox.Focus();
                return false;
            }

            // Validar que no exceda el saldo
            if (monto > PresupuestoOriginal.SaldoPendiente)
            {
                MessageBox.Show($"El monto no puede ser mayor al saldo pendiente ({PresupuestoOriginal.SaldoPendiente:C}).", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                MontoPagoTextBox.Focus();
                return false;
            }

            // Validar método de pago
            if (MetodoPagoComboBox.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un método de pago.", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                MetodoPagoComboBox.Focus();
                return false;
            }

            // Validar fecha
            if (!FechaPagoDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha para el pago.", 
                              "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                FechaPagoDatePicker.Focus();
                return false;
            }

            return true;
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MontoPagoTextBox.Focus();
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
