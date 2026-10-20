using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Notalegis.Datos.Repositorios;
using Notalegis.Datos.Modelos;

namespace Notalegis.Views
{
    public partial class PagadosWindow : Window
    {
        private List<PresupuestoPago> presupuestosCompletos;
        private List<PresupuestoPago> presupuestosFiltrados;
        private List<Cliente> clientes;
        private CobroDAO cobroDAO;
        private ClienteDAO clienteDAO;
        private PagoDAO pagoDAO;

        public PagadosWindow()
        {
            InitializeComponent();
            cobroDAO = new CobroDAO();
            clienteDAO = new ClienteDAO();
            pagoDAO = new PagoDAO();
            InicializarVentana();
            CargarDatos();
        }

        private void InicializarVentana()
        {
            // Configurar fechas por defecto
            FechaHastaFilter.SelectedDate = DateTime.Now;
            FechaDesdeFilter.SelectedDate = DateTime.Now.AddMonths(-3);
            
            // Configurar filtro de estado por defecto
            EstadoFilterComboBox.SelectedIndex = 0; // "Todos"
        }

        private void CargarDatos()
        {
            CargarClientes();
            CargarPresupuestos();
            ActualizarDataGrid();
            ActualizarEstadisticas();
        }

        private void CargarClientes()
        {
            try
            {
                // Se incluyen clientes activos e inactivos: un cliente dado de baja puede tener
                // cobros/pagos históricos y tiene que poder seguir filtrándose por su nombre.
                var clientesDB = clienteDAO.ObtenerTodos();
                clientesDB.AddRange(clienteDAO.ObtenerInactivos());

                clientes = new List<Cliente>
                {
                    new Cliente { Id = 0, Nombre = "Todos", Apellido = "los clientes", DNI = "", Telefono = "", Email = "" }
                };
                clientes.AddRange(clientesDB.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre));

                ClienteFilterComboBox.ItemsSource = clientes;
                ClienteFilterComboBox.DisplayMemberPath = "NombreCompleto";
                ClienteFilterComboBox.SelectedValuePath = "Id";
                ClienteFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Fallback con cliente por defecto
                clientes = new List<Cliente>
                {
                    new Cliente { Id = 0, Nombre = "Todos", Apellido = "los clientes", DNI = "", Telefono = "", Email = "" }
                };
                ClienteFilterComboBox.ItemsSource = clientes;
                ClienteFilterComboBox.SelectedIndex = 0;
            }
        }

        private void CargarPresupuestos()
        {
            try
            {
                // Cargar presupuestos reales desde la tabla Cobros
                var cobros = cobroDAO.ObtenerTodos();
                presupuestosCompletos = ConvertirCobrosAPresupuestos(cobros);
                presupuestosFiltrados = new List<PresupuestoPago>(presupuestosCompletos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar presupuestos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                presupuestosCompletos = new List<PresupuestoPago>();
                presupuestosFiltrados = new List<PresupuestoPago>();
            }
        }

        private List<PresupuestoPago> ConvertirCobrosAPresupuestos(List<Cobro> cobros)
        {
            var presupuestos = new List<PresupuestoPago>();

            // Cargar todos los clientes una sola vez para evitar múltiples consultas
            Dictionary<int, Cliente> clientesDict = new Dictionary<int, Cliente>();
            try
            {
                var todosLosClientes = clienteDAO.ObtenerTodos();
                clientesDict = todosLosClientes.ToDictionary(c => c.Id, c => c);
            }
            catch (Exception)
            {
                // Si falla la carga de clientes, continuar con diccionario vacío
            }

            foreach (var cobro in cobros)
            {
                // Obtener el nombre del cliente desde el diccionario
                string nombreCliente = "Cliente no encontrado";
                if (clientesDict.ContainsKey(cobro.ClienteId))
                {
                    var cliente = clientesDict[cobro.ClienteId];
                    nombreCliente = $"{cliente.Nombre} {cliente.Apellido}".Trim();
                }
                else
                {
                    nombreCliente = $"Cliente ID: {cobro.ClienteId}";
                }

                // Obtener monto abonado desde la base de datos
                decimal montoAbonado = 0m;
                try
                {
                    System.Diagnostics.Debug.WriteLine($"ConvertirCobros - Consultando pagos para CobroId: {cobro.Id}");
                    montoAbonado = pagoDAO.ObtenerTotalPagadoPorCobro(cobro.Id);
                    System.Diagnostics.Debug.WriteLine($"ConvertirCobros - CobroId: {cobro.Id}, MontoAbonado obtenido: {montoAbonado}");
                }
                catch (Exception ex)
                {
                    // Si falla, usar 0
                    System.Diagnostics.Debug.WriteLine($"ConvertirCobros - Error al obtener pagos para CobroId {cobro.Id}: {ex.Message}");
                    montoAbonado = 0m;
                }

                decimal saldoPendiente = cobro.TotalGeneral - montoAbonado;

                // Usar el estado directamente de la base de datos
                string estadoPago = cobro.Estado ?? "Pendiente";

                presupuestos.Add(new PresupuestoPago
                {
                    Id = cobro.Id,
                    ClienteId = cobro.ClienteId,
                    ClienteNombre = nombreCliente,
                    Fecha = cobro.FechaCobro,
                    NumeroExpediente = cobro.NumeroExpediente ?? $"EXP-{cobro.Id:D4}",
                    TipoTramite = cobro.TipoTramite ?? "No especificado",
                    Total = cobro.TotalGeneral,
                    MontoAbonado = montoAbonado,
                    SaldoPendiente = saldoPendiente,
                    EstadoPago = estadoPago,
                    MetodoPago = cobro.MetodoPago ?? "No especificado",
                    UltimaActualizacion = cobro.FechaActualizacion ?? cobro.FechaCobro,
                    Observaciones = cobro.Observaciones ?? "",
                    SubtotalImpuestos = cobro.SubtotalImpuestos,
                    SubtotalCertificados = cobro.SubtotalCertificados
                });
            }

            return presupuestos.OrderByDescending(p => p.Fecha).ToList();
        }

        private void ActualizarDataGrid()
        {
            PagadosDataGrid.ItemsSource = presupuestosFiltrados;
            
            if (presupuestosFiltrados.Count == 0)
            {
                PagadosDataGrid.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = Visibility.Visible;
            }
            else
            {
                PagadosDataGrid.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ActualizarEstadisticas()
        {
            if (presupuestosFiltrados.Count == 0)
            {
                LimpiarEstadisticas();
                return;
            }

            // Estadísticas principales
            var totalPresupuestos = presupuestosFiltrados.Count;
            var pagadosCompletos = presupuestosFiltrados.Count(p => p.EstadoPago == "Pagado");
            var pagosparciales = presupuestosFiltrados.Count(p => p.EstadoPago == "Parcial");
            var pendientes = presupuestosFiltrados.Count(p => p.EstadoPago == "Pendiente");

            TotalPresupuestosText.Text = totalPresupuestos.ToString();
            PagadosCompletosText.Text = pagadosCompletos.ToString();
            PagosParcialesText.Text = pagosparciales.ToString();
            PendientesText.Text = pendientes.ToString();

            // Totales monetarios
            var totalPresupuestado = presupuestosFiltrados.Sum(p => p.Total);
            var totalAbonado = presupuestosFiltrados.Sum(p => p.MontoAbonado);
            var saldoPendiente = presupuestosFiltrados.Sum(p => p.SaldoPendiente);
            
            // Calcular ganancias reales: honorarios + IVA de los presupuestos pagados
            var gananciasEscribania = presupuestosFiltrados
                .Where(p => p.MontoAbonado > 0) // Solo los que tienen pagos
                .Sum(p => {
                    // Obtener el cobro completo para acceder a los honorarios
                    try 
                    {
                        var cobro = cobroDAO.ObtenerPorId(p.Id);
                        return cobro != null ? (cobro.HonorariosImporte + cobro.IVAImporte) : 0m;
                    }
                    catch 
                    {
                        return 0m;
                    }
                });

            TotalPresupuestadoText.Text = $"$ {totalPresupuestado:N2}";
            TotalAbonadoText.Text = $"$ {totalAbonado:N2}";
            SaldoPendienteText.Text = $"$ {saldoPendiente:N2}";
            GananciasTotalesText.Text = $"$ {gananciasEscribania:N2}";

            // Porcentaje cobrado
            var porcentajeCobrado = totalPresupuestado > 0 ? (totalAbonado / totalPresupuestado) * 100 : 0;
            PorcentajeCobradoText.Text = $"{porcentajeCobrado:F1}%";
        }

        private void LimpiarEstadisticas()
        {
            TotalPresupuestosText.Text = "0";
            PagadosCompletosText.Text = "0";
            PagosParcialesText.Text = "0";
            PendientesText.Text = "0";
            TotalPresupuestadoText.Text = "$ 0,00";
            TotalAbonadoText.Text = "$ 0,00";
            SaldoPendienteText.Text = "$ 0,00";
            GananciasTotalesText.Text = "$ 0,00";
            PorcentajeCobradoText.Text = "0,0%";
        }

        private void FiltrarButton_Click(object sender, RoutedEventArgs e)
        {
            var clienteSeleccionado = ClienteFilterComboBox.SelectedValue as int?;
            var estadoSeleccionado = (EstadoFilterComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var fechaDesde = FechaDesdeFilter.SelectedDate;
            var fechaHasta = FechaHastaFilter.SelectedDate;

            // Debug: mostrar valores de filtro
            System.Diagnostics.Debug.WriteLine($"Filtro - Cliente: {clienteSeleccionado}, Estado: '{estadoSeleccionado}', Desde: {fechaDesde}, Hasta: {fechaHasta}");
            System.Diagnostics.Debug.WriteLine($"Total presupuestos antes de filtrar: {presupuestosCompletos.Count}");

            // Debug: mostrar todos los estados antes del filtro
            foreach (var p in presupuestosCompletos)
            {
                System.Diagnostics.Debug.WriteLine($"ANTES FILTRO - {p.ClienteNombre}: Estado='{p.EstadoPago}', Total={p.Total}, Abonado={p.MontoAbonado}, Saldo={p.SaldoPendiente}");
            }
            
            // Debug: mostrar qué items del ComboBox están disponibles
            System.Diagnostics.Debug.WriteLine("Items en EstadoFilterComboBox:");
            foreach (ComboBoxItem item in EstadoFilterComboBox.Items)
            {
                System.Diagnostics.Debug.WriteLine($"  Item: '{item.Content}', IsSelected: {item.IsSelected}");
            }
            System.Diagnostics.Debug.WriteLine($"SelectedItem: {EstadoFilterComboBox.SelectedItem}");

            presupuestosFiltrados = presupuestosCompletos.Where(p =>
            {
                // Filtro por cliente
                if (clienteSeleccionado.HasValue && clienteSeleccionado.Value != 0 && p.ClienteId != clienteSeleccionado.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"Filtrado por cliente: {p.ClienteNombre} (ID: {p.ClienteId})");
                    return false;
                }

                // Filtro por estado - Debug detallado
                if (!string.IsNullOrEmpty(estadoSeleccionado) && estadoSeleccionado != "Todos")
                {
                    bool estadoCoincide = p.EstadoPago == estadoSeleccionado;
                    System.Diagnostics.Debug.WriteLine($"FILTRO ESTADO - {p.ClienteNombre}: '{p.EstadoPago}' == '{estadoSeleccionado}' = {estadoCoincide}");
                    
                    if (!estadoCoincide)
                    {
                        System.Diagnostics.Debug.WriteLine($"EXCLUIDO por estado: {p.ClienteNombre}");
                        return false;
                    }
                }

                // Filtro por fecha
                if (fechaDesde.HasValue && p.Fecha < fechaDesde.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"EXCLUIDO por fecha desde: {p.ClienteNombre}");
                    return false;
                }
                if (fechaHasta.HasValue && p.Fecha > fechaHasta.Value)
                {
                    System.Diagnostics.Debug.WriteLine($"EXCLUIDO por fecha hasta: {p.ClienteNombre}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"INCLUIDO: {p.ClienteNombre}");
                return true;
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"Total presupuestos después de filtrar: {presupuestosFiltrados.Count}");

            ActualizarDataGrid();
            ActualizarEstadisticas();
        }

        private void RegistrarPagoButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PresupuestoPago presupuesto)
            {
                var registrarPagoWindow = new RegistrarPagoWindow(presupuesto);
                if (registrarPagoWindow.ShowDialog() == true)
                {
                    // Actualizar el cobro en la base de datos
                    ActualizarCobroEnBaseDatos(registrarPagoWindow.PresupuestoActualizado, registrarPagoWindow.MontoNuevoPago,
                        registrarPagoWindow.FechaNuevoPago, registrarPagoWindow.ObservacionesNuevoPago);
                    
                    // Recargar completamente los datos desde la base de datos
                    CargarPresupuestos();
                    
                    // Aplicar filtros actuales pero manteniendo el estado actual del filtro
                    presupuestosFiltrados = new List<PresupuestoPago>(presupuestosCompletos);
                    ActualizarDataGrid();
                    ActualizarEstadisticas();
                    
                    MessageBox.Show("Pago registrado y datos actualizados correctamente.", 
                                  "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ActualizarCobroEnBaseDatos(PresupuestoPago presupuestoActualizado, decimal montoNuevoPago, DateTime fechaNuevoPago, string observacionesNuevoPago)
        {
            try
            {
                // Solo crear el pago si hay un monto válido
                if (montoNuevoPago > 0)
                {
                    // Crear un nuevo registro de pago con el monto individual
                    var nuevoPago = new Pago
                    {
                        CobroId = presupuestoActualizado.Id,
                        MontoPagado = montoNuevoPago, // Usar el monto del pago individual, no el total
                        FechaPago = fechaNuevoPago,
                        MetodoPago = presupuestoActualizado.MetodoPago ?? "No especificado",
                        NumeroComprobante = "", // Se puede agregar en el futuro
                        Observaciones = string.IsNullOrWhiteSpace(observacionesNuevoPago)
                            ? $"Pago registrado: {montoNuevoPago:C}"
                            : observacionesNuevoPago,
                        UsuarioRegistro = Environment.UserName,
                        FechaRegistro = DateTime.Now
                    };

                    // Guardar el pago en la tabla Pagos
                    pagoDAO.Insertar(nuevoPago);
                    
                    // Verificar que se guardó correctamente
                    decimal totalPagadoVerificacion = pagoDAO.ObtenerTotalPagadoPorCobro(presupuestoActualizado.Id);
                    
                    // Obtener el cobro y actualizar su estado
                    var cobro = cobroDAO.ObtenerPorId(presupuestoActualizado.Id);
                    if (cobro != null)
                    {
                        decimal saldoPendiente = cobro.TotalGeneral - totalPagadoVerificacion;
                        
                        // Actualizar estado del cobro basado en el total pagado real
                        if (totalPagadoVerificacion <= 0)
                            cobro.Estado = "Pendiente";
                        else if (saldoPendiente > 0.01m) // Usar tolerancia para decimales
                            cobro.Estado = "Parcial";
                        else
                            cobro.Estado = "Pagado";

                        cobro.MetodoPago = presupuestoActualizado.MetodoPago;
                        cobro.FechaActualizacion = DateTime.Now;
                        cobro.UsuarioActualizacion = Environment.UserName;

                        // Guardar cambios en el cobro
                        bool actualizado = cobroDAO.Actualizar(cobro);
                        
                        if (!actualizado)
                        {
                            MessageBox.Show("El pago se registró pero no se pudo actualizar el estado del cobro.", 
                                          "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            MessageBox.Show($"Pago registrado exitosamente.\nMonto: {montoNuevoPago:C}\nTotal pagado: {totalPagadoVerificacion:C}\nNuevo estado: {cobro.Estado}", 
                                          "Pago Registrado", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar el pago: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VerDetallesButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PresupuestoPago presupuesto)
            {
                var detallesWindow = new DetallesPresupuestoWindow(presupuesto);
                detallesWindow.ShowDialog();
            }
        }

        private void HistorialPagosButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PresupuestoPago presupuesto)
            {
                var historialWindow = new HistorialPagosWindow(presupuesto);
                historialWindow.ShowDialog();
            }
        }

        private void PagadosDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PagadosDataGrid.SelectedItem is PresupuestoPago presupuesto)
            {
                VerDetallesButton_Click(new Button { Tag = presupuesto }, null);
            }
        }

        private void ExportarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.csv)|*.csv|Todos los archivos (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"Pagados_Escribania_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportarACSV(saveFileDialog.FileName);
                    MessageBox.Show($"Datos exportados exitosamente a:\n{saveFileDialog.FileName}", 
                                  "Exportación Exitosa", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el archivo:\n{ex.Message}", 
                              "Error de Exportación", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Error);
            }
        }

        private void ExportarACSV(string rutaArchivo)
        {
            var csv = new StringBuilder();
            
            // Usar todos los presupuestos completos en lugar de solo los filtrados
            var todosLosPresupuestos = presupuestosCompletos;
            
            // Headers de las columnas (primera fila)
            csv.AppendLine("Fecha,Cliente,Expediente,Tipo Trámite,Total Presupuesto,Monto Abonado,Saldo Pendiente,Estado Pago,Método Pago,Última Actualización,Observaciones");
            
            // Datos - exportar todos los registros sin filtrar
            foreach (var presupuesto in todosLosPresupuestos)
            {
                csv.AppendLine($"{presupuesto.Fecha:dd/MM/yyyy}," +
                              $"{EscaparCSV(presupuesto.ClienteNombre)}," +
                              $"{EscaparCSV(presupuesto.NumeroExpediente)}," +
                              $"{EscaparCSV(presupuesto.TipoTramite)}," +
                              $"{presupuesto.Total:F2}," +
                              $"{presupuesto.MontoAbonado:F2}," +
                              $"{presupuesto.SaldoPendiente:F2}," +
                              $"{EscaparCSV(presupuesto.EstadoPago)}," +
                              $"{EscaparCSV(presupuesto.MetodoPago ?? "")}," +
                              $"{presupuesto.UltimaActualizacion:dd/MM/yyyy}," +
                              $"{EscaparCSV(presupuesto.Observaciones ?? "")}");
            }
            
            File.WriteAllText(rutaArchivo, csv.ToString(), Encoding.UTF8);
        }

        private string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "";
            
            // Escapar comillas duplicándolas y remover caracteres problemáticos
            valor = valor.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", " ");
            return valor;
        }

        private void ActualizarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
            MessageBox.Show("Datos actualizados correctamente", 
                          "Información", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Information);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Configuraciones adicionales al cargar
        }

        private void PagadosDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
