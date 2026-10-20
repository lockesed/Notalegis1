using System;
using System.Windows;
using Notalegis.Models;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Notalegis.Views
{
    public partial class EditarTramiteWindow : Window
    {
        public Tramite TramiteEditado { get; private set; }
        private Tramite tramiteOriginal;
        private bool _guardado = false;

        public EditarTramiteWindow(Tramite tramite = null)
        {
            InitializeComponent();
            
            if (tramite != null)
            {
                tramiteOriginal = tramite;
                CargarDatosTramite(tramite);
                Title = $"Editar Trámite - {tramite.TipoTramite}";
            }
            else
            {
                Title = "Nuevo Trámite";
                // Establecer valores por defecto
                dpFechaInicio.SelectedDate = DateTime.Today;
                dpFechaVencimiento.SelectedDate = DateTime.Today.AddDays(30);
                cmbEstado.SelectedIndex = 0; // Pendiente
                cmbPrioridad.SelectedIndex = 1; // Media
            }
        }

        private void CargarDatosTramite(Tramite tramite)
        {
            cmbCliente.Text = tramite.Cliente;
            cmbTipoTramite.Text = tramite.TipoTramite;
            dpFechaInicio.SelectedDate = tramite.FechaInicio;
            dpFechaVencimiento.SelectedDate = tramite.FechaVencimiento;
            chkDocumentacionCompleta.IsChecked = tramite.DocumentacionCompleta;
            txtObservaciones.Text = tramite.Observaciones;
            txtNumeroExpediente.Text = tramite.NumeroExpediente;
            txtMontoEstimado.Text = tramite.MontoEstimado?.ToString("F2");

            // Seleccionar estado
            foreach (System.Windows.Controls.ComboBoxItem item in cmbEstado.Items)
            {
                if (item.Content.ToString() == tramite.Estado)
                {
                    cmbEstado.SelectedItem = item;
                    break;
                }
            }

            // Seleccionar prioridad
            foreach (System.Windows.Controls.ComboBoxItem item in cmbPrioridad.Items)
            {
                if (item.Content.ToString() == tramite.Prioridad)
                {
                    cmbPrioridad.SelectedItem = item;
                    break;
                }
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _guardado = true;
            DialogResult = false;
            Close();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (ValidarFormulario())
            {
                // Crear o actualizar trámite
                if (tramiteOriginal == null)
                {
                    // Nuevo trámite
                    TramiteEditado = new Tramite();
                }
                else
                {
                    // Trámite existente
                    TramiteEditado = tramiteOriginal;
                }

                // Asignar valores
                TramiteEditado.Cliente = cmbCliente.Text.Trim();
                TramiteEditado.TipoTramite = cmbTipoTramite.Text.Trim();
                TramiteEditado.FechaInicio = dpFechaInicio.SelectedDate ?? DateTime.Today;
                TramiteEditado.FechaVencimiento = dpFechaVencimiento.SelectedDate ?? DateTime.Today.AddDays(30);
                TramiteEditado.Estado = ((System.Windows.Controls.ComboBoxItem)cmbEstado.SelectedItem)?.Content.ToString() ?? "Pendiente";
                TramiteEditado.Prioridad = ((System.Windows.Controls.ComboBoxItem)cmbPrioridad.SelectedItem)?.Content.ToString() ?? "Media";
                TramiteEditado.DocumentacionCompleta = chkDocumentacionCompleta.IsChecked ?? false;
                TramiteEditado.Observaciones = txtObservaciones.Text.Trim();
                TramiteEditado.NumeroExpediente = txtNumeroExpediente.Text.Trim();
                
                // Convertir monto
                if (decimal.TryParse(txtMontoEstimado.Text, out decimal monto))
                {
                    TramiteEditado.MontoEstimado = monto;
                }

                _guardado = true;
                DialogResult = true;
                Close();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_guardado && !Notalegis.App.BloqueoAutomatico)
            {
                var resultado = MessageBox.Show(
                    "¿Seguro que desea cerrar? Los cambios no guardados se perderán.",
                    "Confirmar cierre",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                    e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private bool ValidarFormulario()
        {
            // Validar cliente
            if (string.IsNullOrWhiteSpace(cmbCliente.Text))
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Error de Validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCliente.Focus();
                return false;
            }

            // Validar tipo de trámite
            if (string.IsNullOrWhiteSpace(cmbTipoTramite.Text))
            {
                MessageBox.Show("Debe seleccionar un tipo de trámite.", "Error de Validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbTipoTramite.Focus();
                return false;
            }

            // Validar fecha de inicio
            if (!dpFechaInicio.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de inicio.", "Error de Validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpFechaInicio.Focus();
                return false;
            }

            // Validar fecha de vencimiento
            if (!dpFechaVencimiento.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar una fecha de vencimiento.", "Error de Validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpFechaVencimiento.Focus();
                return false;
            }

            // Validar que la fecha de vencimiento sea posterior a la de inicio
            if (dpFechaVencimiento.SelectedDate <= dpFechaInicio.SelectedDate)
            {
                MessageBox.Show("La fecha de vencimiento debe ser posterior a la fecha de inicio.", "Error de Validación", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                dpFechaVencimiento.Focus();
                return false;
            }

            // Validar monto si se ingresó
            if (!string.IsNullOrWhiteSpace(txtMontoEstimado.Text))
            {
                if (!decimal.TryParse(txtMontoEstimado.Text, out decimal monto) || monto < 0)
                {
                    MessageBox.Show("El monto estimado debe ser un número válido mayor o igual a cero.", "Error de Validación", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtMontoEstimado.Focus();
                    return false;
                }
            }

            return true;
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
