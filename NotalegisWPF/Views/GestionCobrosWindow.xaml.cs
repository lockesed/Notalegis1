using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;
using NotalegisWPF.Services;
using NotalegisWPF.Views;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Notalegis.Views
{
    public partial class GestionCobrosWindow : Window
    {
        private List<Notalegis.Datos.Modelos.Cliente> clientes;
        private Notalegis.Datos.Modelos.Cliente clienteSeleccionado;
        private ClienteDAO clienteDAO;
        private CobroDAO cobroDAO;
        private ConfiguracionService confService;
        private readonly AuditoriaService _auditoria = new AuditoriaService();

        public GestionCobrosWindow()
        {
            InitializeComponent();
            clienteDAO = new ClienteDAO();
            cobroDAO = new CobroDAO();
            confService = new ConfiguracionService();
            CargarClientes();
            CargarValoresPorDefecto();
        }


        private void CargarClientes()
        {
            try
            {
                clientes = clienteDAO.ObtenerTodos();
                ClienteComboBox.ItemsSource = clientes;
                ClienteComboBox.DisplayMemberPath = "NombreCompleto";
                ClienteComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Cargar datos de respaldo si falla la base de datos
                CargarClientesRespaldo();
            }
        }

        private void CargarClientesRespaldo()
        {
            clientes = new List<Notalegis.Datos.Modelos.Cliente>
            {
                new Notalegis.Datos.Modelos.Cliente { Id = 1, Nombre = "Cliente", Apellido = "Demo", DNI = "00000000", Telefono = "000-000-0000", Email = "demo@notalegis.com" }
            };

            ClienteComboBox.ItemsSource = clientes;
            ClienteComboBox.DisplayMemberPath = "NombreCompleto";
            ClienteComboBox.SelectedValuePath = "Id";
        }

        private void CargarValoresPorDefecto()
        {
            // Cargar valores por defecto desde configuración
            var valoresPorDefecto = ObtenerValoresPorDefecto();
            
            if (valoresPorDefecto.ContainsKey("IVA"))
                IVAPorc.Text = valoresPorDefecto["IVA"].ToString("0.00", CultureInfo.InvariantCulture);
            
            if (valoresPorDefecto.ContainsKey("TasaGeneral"))
                TasaGeneralPorc.Text = valoresPorDefecto["TasaGeneral"].ToString("0.00", CultureInfo.InvariantCulture);
                
            if (valoresPorDefecto.ContainsKey("ImpGanancias"))
                ImpGananciasPorc.Text = valoresPorDefecto["ImpGanancias"].ToString("0.00", CultureInfo.InvariantCulture);
                
            if (valoresPorDefecto.ContainsKey("ITI"))
                ITIPorc.Text = valoresPorDefecto["ITI"].ToString("0.00", CultureInfo.InvariantCulture);
                
            if (valoresPorDefecto.ContainsKey("ImpSellos"))
                ImpSellosPorc.Text = valoresPorDefecto["ImpSellos"].ToString("0.00", CultureInfo.InvariantCulture);
                
            if (valoresPorDefecto.ContainsKey("Honorarios"))
                HonorariosPorc.Text = valoresPorDefecto["Honorarios"].ToString("0.00", CultureInfo.InvariantCulture);
        }

        private Dictionary<string, decimal> ObtenerValoresPorDefecto()
        {
            return new Dictionary<string, decimal>
            {
                { "IVA", confService.ObtenerValorDecimal(ClavesConfiguracion.ImpuestoIVAPorc, 21.0m) },
                { "TasaGeneral", confService.ObtenerValorDecimal("TasaGeneral_Porc", 1.5m) }, // Valor por defecto anterior
                { "ImpGanancias", confService.ObtenerValorDecimal(ClavesConfiguracion.ImpuestoGananciasPorc, 0.0m) },
                { "ITI", confService.ObtenerValorDecimal(ClavesConfiguracion.ImpuestoITIPorc, 0.0m) },
                { "ImpSellos", confService.ObtenerValorDecimal(ClavesConfiguracion.ImpuestoSellosPorc, 0.0m) },
                { "Honorarios", confService.ObtenerValorDecimal(ClavesConfiguracion.HonorariosGeneralesPorc, 0.0m) },
                { "SelloMatriz", 0.0m },
                { "Certificados", 0.0m },
                { "Diligenciamiento", 0.0m }
            };
        }

        private void ClienteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClienteComboBox.SelectedItem is Notalegis.Datos.Modelos.Cliente cliente)
            {
                clienteSeleccionado = cliente;
                MostrarInformacionCliente(cliente);
            }
            else
            {
                clienteSeleccionado = null;
                OcultarInformacionCliente();
            }
        }

        private void MostrarInformacionCliente(Notalegis.Datos.Modelos.Cliente cliente)
        {
            ClienteNombreText.Text = cliente.NombreCompleto;
            ClienteDocumentoText.Text = $"DNI: {cliente.DNI}";
            ClienteTelefonoText.Text = $"Tel: {cliente.Telefono}";
            ClienteEmailText.Text = cliente.Email;

            ClienteInfoPanel.Visibility = Visibility.Visible;
            SinClienteText.Visibility = Visibility.Collapsed;
        }

        private void OcultarInformacionCliente()
        {
            ClienteInfoPanel.Visibility = Visibility.Collapsed;
            SinClienteText.Visibility = Visibility.Visible;
        }

        private void NuevoClienteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir ventana de gestión de clientes
                var clientesView = new ClientesView();
                clientesView.ShowDialog();
                
                // Recargar la lista de clientes después de cerrar la ventana
                CargarClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la gestión de clientes: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalcularImporte(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                CalcularImporteParaCampo(textBox);
            }
            CalcularTotales();
        }

        private void CalcularImporteParaCampo(TextBox textBox)
        {
            string nombreCampo = textBox.Name;
            
            if (nombreCampo.EndsWith("Porc") || nombreCampo.EndsWith("Base"))
            {
                string baseName = nombreCampo.Replace("Porc", "").Replace("Base", "");
                
                var porcTextBox = FindName($"{baseName}Porc") as TextBox;
                var baseTextBox = FindName($"{baseName}Base") as TextBox;
                var importeTextBox = FindName($"{baseName}Importe") as TextBox;

                if (porcTextBox != null && baseTextBox != null && importeTextBox != null)
                {
                    // Intentamos parchear las comas y puntos porque Decimal.TryParse puede ser quisquilloso
                    bool hasPorc = decimal.TryParse(porcTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal porcentaje);
                    bool hasBase = decimal.TryParse(baseTextBox.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal baseValue);
                    
                    if (hasPorc && hasBase)
                    {
                        decimal importe = (baseValue * porcentaje) / 100;
                        importeTextBox.Text = importe.ToString("F2");
                    }
                    else
                    {
                        importeTextBox.Text = "";
                    }
                }
            }
        }

        private void CalcularTotales(object sender = null, TextChangedEventArgs e = null)
        {
            CalcularSubtotal1();
            CalcularSubtotal2();
            CalcularTotalGeneral();
        }

        private void CertificadoChanged(object sender, TextChangedEventArgs e)
        {
            // Calcular el monto total de las escrituras (certificados)
            decimal montoTotalEscrituras = CalcularMontoTotalEscrituras();
            
            // Actualizar automáticamente los campos "base" de los impuestos
            ActualizarBasesImpuestos(montoTotalEscrituras);
            
            // Recalcular totales
            CalcularTotales();
        }

        private decimal CalcularMontoTotalEscrituras()
        {
            decimal total = 0;
            
            // Sumar todos los montos de certificados y gestiones
            total += TryParseDecimal(EscrituraCompraventa.Text);
            total += TryParseDecimal(ConstitucionSociedad.Text);
            total += TryParseDecimal(PoderGeneral.Text);
            total += TryParseDecimal(CertificadoDominio.Text);
            total += TryParseDecimal(Testamento.Text);
            total += TryParseDecimal(Donacion.Text);
            total += TryParseDecimal(Hipoteca.Text);
            total += TryParseDecimal(CancelacionHipoteca.Text);
            total += TryParseDecimal(Usufructo.Text);
            total += TryParseDecimal(Servidumbre.Text);
            total += TryParseDecimal(CustomTramiteImporte.Text);
            
            return total;
        }

        private decimal TryParseDecimal(string text)
        {
            return decimal.TryParse(text, out decimal result) ? result : 0;
        }

        private void ActualizarBasesImpuestos(decimal montoBase)
        {
            // Solo actualizar si hay un monto base válido
            if (montoBase > 0)
            {
                string montoFormateado = montoBase.ToString("F2");
                
                // Actualizar todos los campos "base" de los impuestos
                // Solo si están vacíos o si el usuario no los ha modificado manualmente
                if (string.IsNullOrWhiteSpace(ImpGananciasBase.Text) || ShouldUpdateBase(ImpGananciasBase.Text, montoBase))
                    ImpGananciasBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(ITIBase.Text) || ShouldUpdateBase(ITIBase.Text, montoBase))
                    ITIBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(ImpSellosBase.Text) || ShouldUpdateBase(ImpSellosBase.Text, montoBase))
                    ImpSellosBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(TasaGeneralBase.Text) || ShouldUpdateBase(TasaGeneralBase.Text, montoBase))
                    TasaGeneralBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(SelloMatrizBase.Text) || ShouldUpdateBase(SelloMatrizBase.Text, montoBase))
                    SelloMatrizBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(CertificadosBase.Text) || ShouldUpdateBase(CertificadosBase.Text, montoBase))
                    CertificadosBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(DiligenciamientoBase.Text) || ShouldUpdateBase(DiligenciamientoBase.Text, montoBase))
                    DiligenciamientoBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(HonorariosBase.Text) || ShouldUpdateBase(HonorariosBase.Text, montoBase))
                    HonorariosBase.Text = montoFormateado;
                
                if (string.IsNullOrWhiteSpace(IVABase.Text) || ShouldUpdateBase(IVABase.Text, montoBase))
                    IVABase.Text = montoFormateado;
            }
        }

        // Decide si un campo "Base" de impuesto se puede pisar con el nuevo monto total
        // de escrituras, o si hay que respetarlo porque el usuario lo escribió a mano.
        //
        // No hay ninguna bandera guardada que diga "esto lo tocó el usuario", así que se
        // infiere matemáticamente: se reconstruye cuál habría sido el monto total de
        // escrituras ANTES del último cambio (restando el total nuevo y volviendo a sumar
        // el valor que hoy tiene el campo). Si ese monto reconstruido coincide con lo que
        // el campo ya tiene, es porque el campo fue completado automáticamente la vez
        // anterior (y por lo tanto es seguro actualizarlo de nuevo). Si no coincide, es
        // porque el usuario lo pisó a mano, y no se lo debe tocar.
        private bool ShouldUpdateBase(string currentBaseText, decimal newMontoBase)
        {
            if (decimal.TryParse(currentBaseText, out decimal currentBase))
            {
                decimal totalAnteriorReconstruido = CalcularMontoTotalEscrituras() - newMontoBase + currentBase;
                return Math.Abs(currentBase - totalAnteriorReconstruido) < 0.01m;
            }
            return true;
        }

        private void CalcularSubtotal1()
        {
            decimal subtotal = 0;
            
            var campos = new[] { "ImpGananciasImporte", "ITIImporte", "ImpSellosImporte", 
                               "TasaGeneralImporte", "SelloMatrizImporte", "CertificadosImporte",
                               "DiligenciamientoImporte", "HonorariosImporte", "IVAImporte" };

            foreach (string campo in campos)
            {
                var textBox = FindName(campo) as TextBox;
                if (textBox != null && decimal.TryParse(textBox.Text, out decimal valor))
                {
                    subtotal += valor;
                }
            }

            Subtotal1.Text = subtotal.ToString("F2");
            SubtotalImpuestosDisplay.Text = $"$ {subtotal:N2}";
        }

        private void CalcularSubtotal2()
        {
            decimal subtotal = 0;
            
            var campos = new[] { "EscrituraCompraventa", "ConstitucionSociedad", "PoderGeneral", 
                               "CertificadoDominio", "Testamento", "Donacion",
                               "Hipoteca", "CancelacionHipoteca", "Usufructo", "Servidumbre" };

            foreach (string campo in campos)
            {
                var textBox = FindName(campo) as TextBox;
                if (textBox != null && decimal.TryParse(textBox.Text, out decimal valor))
                {
                    subtotal += valor;
                }
            }

            // Add custom tramite amount if valid
            if (CustomTramiteImporte != null && decimal.TryParse(CustomTramiteImporte.Text, out decimal customAmount))
            {
                subtotal += customAmount;
            }

            Subtotal2.Text = subtotal.ToString("F2");
            SubtotalCertificadosDisplay.Text = $"$ {subtotal:N2}";
        }

        private void CalcularTotalGeneral()
        {
            decimal subtotal1 = 0;
            decimal subtotal2 = 0;

            if (decimal.TryParse(Subtotal1.Text, out subtotal1) &&
                decimal.TryParse(Subtotal2.Text, out subtotal2))
            {
                decimal total = subtotal1 + subtotal2;
                TotalGeneralDisplay.Text = $"$ {total:N2}";
            }
        }


        private void LimpiarButton_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            MessageBox.Show("Todos los campos han sido limpiados", 
                          "Información", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LimpiarFormulario()
        {
            foreach (TextBox textBox in FindVisualChildren<TextBox>(this))
            {
                if (!textBox.IsReadOnly)
                {
                    textBox.Clear();
                }
            }
            
            ClienteComboBox.SelectedItem = null;
            clienteSeleccionado = null;
            OcultarInformacionCliente();
            
            // Recargar valores por defecto
            CargarValoresPorDefecto();
        }


        private void GenerarFacturaButton_Click(object sender, RoutedEventArgs e)
        {
            if (clienteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente antes de generar la factura", 
                              "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // El número de expediente es opcional

            // Validar fecha
            if (!FechaCobroDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Debe seleccionar la fecha del cobro", 
                              "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar que haya al menos un importe
            if (string.IsNullOrWhiteSpace(TotalGeneralDisplay.Text) || TotalGeneralDisplay.Text == "$ 0,00")
            {
                MessageBox.Show("Debe calcular los importes antes de generar la factura", 
                              "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Obtener el total como decimal usando el método existente
            decimal totalGeneral = ObtenerTotalGeneral();

            // Abrir modal completo para datos del presupuesto
            try
            {
                var facturaModal = new NotalegisWPF.Views.FacturaModal(clienteSeleccionado, totalGeneral, ExpedienteTextBox.Text);
                facturaModal.Owner = this;
                
                
                if (facturaModal.ShowDialog() == true)
                {
                    try
                    {
                        // Primero generar el presupuesto PDF
                        GenerarPresupuestoPDFCompleto(facturaModal);
                        
                        // Solo después de generar exitosamente el PDF, guardar el cobro en la base de datos
                        var cobro = CrearCobroDesdeFormulario(facturaModal);
                        
                        if (cobroDAO.Insertar(cobro))
                        {
                            _auditoria.Registrar(SesionActual.NombreUsuario, "ALTA", "Cobros", null, $"Cobro registrado para cliente '{clienteSeleccionado.NombreCompleto}', expediente '{ExpedienteTextBox.Text}', total {cobro.TotalGeneral:N2}");
                            MessageBox.Show($"Presupuesto generado y cobro registrado exitosamente.\n\nCliente: {clienteSeleccionado.NombreCompleto}\nExpediente: {ExpedienteTextBox.Text}\nTotal: {TotalGeneralDisplay.Text}",
                                          "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // Preguntar si desea limpiar el formulario
                            var cleanResult = MessageBox.Show("¿Desea limpiar el formulario para un nuevo cobro?", 
                                                             "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            
                            if (cleanResult == MessageBoxResult.Yes)
                            {
                                LimpiarFormulario();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error al registrar el cobro en la base de datos", 
                                          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al generar el presupuesto: {ex.Message}", 
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la ventana de facturación: {ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarPresupuestoPDFCompleto(NotalegisWPF.Views.FacturaModal datosFactura)
        {
            try
            {
                // Obtener el total como decimal
                decimal totalGeneral = 0;
                string totalText = TotalGeneralDisplay.Text.Replace("$ ", "").Replace(",", "");
                decimal.TryParse(totalText, out totalGeneral);

                // Crear objeto cobro con todos los datos para el PDF
                var cobroParaPDF = CrearCobroDesdeFormulario(datosFactura);
                
                // Generar el PDF usando la clase PDFGenerator con datos completos del modal
                PDFGenerator.GenerarPresupuestoPDFCompleto(
                    clienteSeleccionado, 
                    ExpedienteTextBox.Text, 
                    FechaCobroDatePicker.SelectedDate ?? DateTime.Now, 
                    totalGeneral,
                    datosFactura,
                    cobroParaPDF
                );

                MessageBox.Show($"Presupuesto PDF generado exitosamente con datos completos.\n\n" +
                              $"Cliente: {clienteSeleccionado.NombreCompleto}\n" +
                              $"Escribano: {datosFactura.EscribanoNombreValue}\n" +
                              $"Expediente: {ExpedienteTextBox.Text}\n" +
                              $"Total: {TotalGeneralDisplay.Text}\n\n" +
                              $"El archivo se ha guardado en Documentos/Notalegis/Presupuestos/", 
                              "Presupuesto PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar presupuesto PDF: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HistorialCobrosButton_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar ventana de historial de presupuestos
            var historialWindow = new HistorialCobrosWindow(clienteSeleccionado);
            historialWindow.ShowDialog();
        }

        private void HistorialAbonadosButton_Click(object sender, RoutedEventArgs e)
        {
            // Crear y mostrar ventana de historial de abonados (pagados)
            var pagadosWindow = new PagadosWindow();
            pagadosWindow.ShowDialog();
        }

        private void CustomTramiteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                // Validar que el texto no esté vacío y habilitar el campo de importe
                bool isValid = !string.IsNullOrWhiteSpace(textBox.Text) && textBox.Text.Length >= 3;
                CustomTramiteImporte.IsEnabled = isValid;
                
                if (!isValid)
                {
                    CustomTramiteImporte.Text = "";
                }
                
                // Aplicar estilo visual según validación
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    // Estado normal
                    textBox.BorderBrush = null;
                }
                else if (textBox.Text.Length < 3)
                {
                    // Estado de error - muy corto
                    textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    // Estado válido
                    textBox.BorderBrush = System.Windows.Media.Brushes.Green;
                }
            }
        }

        // Método auxiliar para encontrar controles hijos
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Configuraciones adicionales al cargar la ventana
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Validar si hay cambios sin guardar antes de cerrar
            if (TieneModificacionesSinGuardar())
            {
                var result = MessageBox.Show("¿Está seguro que desea cerrar sin guardar los cambios?", 
                                           "Confirmar cierre", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private bool TieneModificacionesSinGuardar()
        {
            // Verificar si hay datos ingresados
            var textBoxes = FindVisualChildren<TextBox>(this).Where(tb => !tb.IsReadOnly);
            return textBoxes.Any(tb => !string.IsNullOrWhiteSpace(tb.Text));
        }

        private string ObtenerTipoTramiteSeleccionado()
        {
            // Primero verificar si hay un trámite personalizado
            if (!string.IsNullOrWhiteSpace(CustomTramiteTextBox.Text))
            {
                return CustomTramiteTextBox.Text;
            }

            // Verificar los trámites predefinidos con sus campos de importe
            if (ObtenerDecimalDeTextBox(EscrituraCompraventa) > 0)
                return "Escritura de Compraventa";
            
            if (ObtenerDecimalDeTextBox(ConstitucionSociedad) > 0)
                return "Constitución de Sociedad";
            
            if (ObtenerDecimalDeTextBox(PoderGeneral) > 0)
                return "Poder General";
            
            if (ObtenerDecimalDeTextBox(CertificadoDominio) > 0)
                return "Certificado de Dominio";
            
            if (ObtenerDecimalDeTextBox(Testamento) > 0)
                return "Testamento";
            
            if (ObtenerDecimalDeTextBox(Donacion) > 0)
                return "Donación";
            
            if (ObtenerDecimalDeTextBox(Hipoteca) > 0)
                return "Hipoteca";
            
            if (ObtenerDecimalDeTextBox(CancelacionHipoteca) > 0)
                return "Cancelación de Hipoteca";
            
            if (ObtenerDecimalDeTextBox(Usufructo) > 0)
                return "Usufructo";
            
            if (ObtenerDecimalDeTextBox(Servidumbre) > 0)
                return "Servidumbre";

            // Si hay un trámite personalizado con importe
            if (ObtenerDecimalDeTextBox(CustomTramiteImporte) > 0 && !string.IsNullOrWhiteSpace(CustomTramiteTextBox.Text))
                return CustomTramiteTextBox.Text;

            // Por defecto
            return "Gestión Notarial";
        }

        private Cobro CrearCobroDesdeFormulario(FacturaModal facturaModal = null)
        {
            var cobro = new Cobro
            {
                ClienteId = clienteSeleccionado.Id,
                NumeroExpediente = ExpedienteTextBox.Text,
                FechaCobro = facturaModal?.FechaFacturaValue ?? FechaCobroDatePicker.SelectedDate ?? DateTime.Now,
                TipoTramite = ObtenerTipoTramiteSeleccionado(),

                // Impuestos y Honorarios
                ImpGananciasPorc = ObtenerDecimalDeTextBox(ImpGananciasPorc),
                ImpGananciasBase = ObtenerDecimalDeTextBox(ImpGananciasBase),
                ImpGananciasImporte = ObtenerDecimalDeTextBox(ImpGananciasImporte),

                ITIPorc = ObtenerDecimalDeTextBox(ITIPorc),
                ITIBase = ObtenerDecimalDeTextBox(ITIBase),
                ITIImporte = ObtenerDecimalDeTextBox(ITIImporte),

                ImpSellosPorc = ObtenerDecimalDeTextBox(ImpSellosPorc),
                ImpSellosBase = ObtenerDecimalDeTextBox(ImpSellosBase),
                ImpSellosImporte = ObtenerDecimalDeTextBox(ImpSellosImporte),

                TasaGeneralPorc = ObtenerDecimalDeTextBox(TasaGeneralPorc),
                TasaGeneralBase = ObtenerDecimalDeTextBox(TasaGeneralBase),
                TasaGeneralImporte = ObtenerDecimalDeTextBox(TasaGeneralImporte),

                SelloMatrizPorc = ObtenerDecimalDeTextBox(SelloMatrizPorc),
                SelloMatrizBase = ObtenerDecimalDeTextBox(SelloMatrizBase),
                SelloMatrizImporte = ObtenerDecimalDeTextBox(SelloMatrizImporte),

                CertificadosPorc = ObtenerDecimalDeTextBox(CertificadosPorc),
                CertificadosBase = ObtenerDecimalDeTextBox(CertificadosBase),
                CertificadosImporte = ObtenerDecimalDeTextBox(CertificadosImporte),

                DiligenciamientoPorc = ObtenerDecimalDeTextBox(DiligenciamientoPorc),
                DiligenciamientoBase = ObtenerDecimalDeTextBox(DiligenciamientoBase),
                DiligenciamientoImporte = ObtenerDecimalDeTextBox(DiligenciamientoImporte),

                HonorariosPorc = ObtenerDecimalDeTextBox(HonorariosPorc),
                HonorariosBase = ObtenerDecimalDeTextBox(HonorariosBase),
                HonorariosImporte = ObtenerDecimalDeTextBox(HonorariosImporte),

                // Escrituras y Documentos
                EscrituraCompraventa = ObtenerDecimalDeTextBox(EscrituraCompraventa),
                ConstitucionSociedad = ObtenerDecimalDeTextBox(ConstitucionSociedad),
                PoderGeneral = ObtenerDecimalDeTextBox(PoderGeneral),
                CertificadoDominio = ObtenerDecimalDeTextBox(CertificadoDominio),
                Testamento = ObtenerDecimalDeTextBox(Testamento),
                Donacion = ObtenerDecimalDeTextBox(Donacion),
                Hipoteca = ObtenerDecimalDeTextBox(Hipoteca),
                CancelacionHipoteca = ObtenerDecimalDeTextBox(CancelacionHipoteca),
                Usufructo = ObtenerDecimalDeTextBox(Usufructo),
                Servidumbre = ObtenerDecimalDeTextBox(Servidumbre),

                // Otra gestión
                OtraGestionNombre = string.IsNullOrWhiteSpace(CustomTramiteTextBox.Text) ? null : CustomTramiteTextBox.Text.Trim(),
                OtraGestionImporte = ObtenerDecimalDeTextBox(CustomTramiteImporte),

                // Calcular totales
                SubtotalImpuestos = decimal.TryParse(Subtotal1.Text, out decimal sub1) ? sub1 : 0,
                SubtotalCertificados = decimal.TryParse(Subtotal2.Text, out decimal sub2) ? sub2 : 0,
                TotalGeneral = ObtenerTotalGeneral(),

                // Información adicional
                Estado = "Pendiente",
                UsuarioCreacion = Environment.UserName,
                FechaCreacion = DateTime.Now
            };

            return cobro;
        }

        private decimal ObtenerDecimalDeTextBox(TextBox textBox)
        {
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text))
                return 0;

            if (decimal.TryParse(textBox.Text, out decimal valor))
                return valor;

            return 0;
        }

        private decimal ObtenerTotalGeneral()
        {
            if (string.IsNullOrWhiteSpace(TotalGeneralDisplay.Text))
                return 0;

            // Remover el símbolo $ y espacios, manejar separadores de miles
            string texto = TotalGeneralDisplay.Text.Replace("$", "").Replace(" ", "").Trim();
            
            // Usar la cultura actual para el parsing correcto de números con separadores de miles
            if (decimal.TryParse(texto, System.Globalization.NumberStyles.Currency, 
                System.Globalization.CultureInfo.CurrentCulture, out decimal total))
                return total;

            return 0;
        }

        private void EstadisticasButton_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new EstadisticasView { Owner = this };
            ventana.ShowDialog();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
