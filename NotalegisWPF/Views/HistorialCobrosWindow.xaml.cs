using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using ClosedXML.Excel;
using Notalegis.Models;

namespace Notalegis.Views
{
    public partial class HistorialCobrosWindow : Window
    {
        private Notalegis.Datos.Modelos.Cliente cliente;
        private List<Cobro> historialCompleto;
        private List<Cobro> historialFiltrado;
        private CobroDAO cobroDAO;

        public HistorialCobrosWindow(Notalegis.Datos.Modelos.Cliente cliente = null)
        {
            InitializeComponent();
            this.cliente = cliente;
            cobroDAO = new CobroDAO();
            InicializarVentana();
            CargarHistorial();
        }

        private void InicializarVentana()
        {
            if (cliente != null)
            {
                ClienteHeaderText.Text = $"Cliente: {cliente.NombreCompleto} - DNI: {cliente.DNI}";
            }
            else
            {
                ClienteHeaderText.Text = "Historial General de Cobros - Todos los Clientes";
            }
            
            // Configurar fechas por defecto (hoy para mostrar cobros del día)
            FechaHastaFilter.SelectedDate = DateTime.Now;
            FechaDesdeFilter.SelectedDate = DateTime.Now;
        }

        private void CargarHistorial()
        {
            try
            {
                if (cliente != null)
                {
                    historialCompleto = cobroDAO.ObtenerHistorialPorCliente(cliente.Id);
                }
                else
                {
                    // Cargar todos los cobros del día actual
                    historialCompleto = cobroDAO.ObtenerCobrosPorRangoFechas(DateTime.Now.Date, DateTime.Now.Date.AddDays(1).AddTicks(-1));
                }
                
                // Agregar nombres de clientes a cada cobro
                CargarNombresClientes(historialCompleto);
                
                historialFiltrado = new List<Cobro>(historialCompleto);
                
                ActualizarDataGrid();
                CalcularTotales();
                
                if (historialCompleto.Count == 0)
                {
                    string mensaje = cliente != null 
                        ? $"No se encontraron registros de cobros para el cliente {cliente.NombreCompleto}.\n\nPuede generar nuevos cobros desde la ventana de Gestión de Cobros."
                        : "No se encontraron registros de cobros para el día de hoy.\n\nPuede generar nuevos cobros desde la ventana de Gestión de Cobros.";
                    
                    MessageBox.Show(mensaje, "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el historial: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                historialCompleto = new List<Cobro>();
                historialFiltrado = new List<Cobro>();
                ActualizarDataGrid();
                CalcularTotales();
            }
        }


        private void ActualizarDataGrid()
        {
            HistorialDataGrid.ItemsSource = historialFiltrado;
            
            if (historialFiltrado.Count == 0)
            {
                HistorialDataGrid.Visibility = Visibility.Collapsed;
                EmptyStatePanel.Visibility = Visibility.Visible;
            }
            else
            {
                HistorialDataGrid.Visibility = Visibility.Visible;
                EmptyStatePanel.Visibility = Visibility.Collapsed;
            }
        }

        private void CalcularTotales()
        {
            if (historialFiltrado.Count == 0)
            {
                TotalCobrosText.Text = "0";
                TotalImpuestosText.Text = "$ 0,00";
                TotalCertificadosText.Text = "$ 0,00";
                TotalGeneralText.Text = "$ 0,00";
                return;
            }

            var totalCobros = historialFiltrado.Count;
            var totalImpuestos = historialFiltrado.Sum(h => h.SubtotalImpuestos);
            var totalCertificados = historialFiltrado.Sum(h => h.SubtotalCertificados);
            var totalGeneral = historialFiltrado.Sum(h => h.TotalGeneral); // Usar el total real de cada cobro

            TotalCobrosText.Text = totalCobros.ToString();
            TotalImpuestosText.Text = $"$ {totalImpuestos:N2}";
            TotalCertificadosText.Text = $"$ {totalCertificados:N2}";
            TotalGeneralText.Text = $"$ {totalGeneral:N2}";
        }

        private void FiltrarButton_Click(object sender, RoutedEventArgs e)
        {
            AplicarFiltros();
        }

        private void FiltroHoyButton_Click(object sender, RoutedEventArgs e)
        {
            FechaDesdeFilter.SelectedDate = DateTime.Now.Date;
            FechaHastaFilter.SelectedDate = DateTime.Now.Date;
            AplicarFiltros();
        }

        private void FiltroMesButton_Click(object sender, RoutedEventArgs e)
        {
            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);
            
            FechaDesdeFilter.SelectedDate = primerDiaMes;
            FechaHastaFilter.SelectedDate = ultimoDiaMes;
            AplicarFiltros();
        }

        private void FiltroAñoButton_Click(object sender, RoutedEventArgs e)
        {
            var primerDiaAño = new DateTime(DateTime.Now.Year, 1, 1);
            var ultimoDiaAño = new DateTime(DateTime.Now.Year, 12, 31);
            
            FechaDesdeFilter.SelectedDate = primerDiaAño;
            FechaHastaFilter.SelectedDate = ultimoDiaAño;
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            try
            {
                var fechaDesde = FechaDesdeFilter.SelectedDate;
                var fechaHasta = FechaHastaFilter.SelectedDate;

                if (fechaDesde.HasValue && fechaHasta.HasValue)
                {
                    if (cliente != null)
                    {
                        // Filtrar por cliente específico y fechas
                        historialFiltrado = cobroDAO.ObtenerHistorialPorCliente(cliente.Id)
                            .Where(h => h.FechaCobro >= fechaDesde.Value && h.FechaCobro <= fechaHasta.Value)
                            .ToList();
                    }
                    else
                    {
                        // Filtrar todos los cobros por fechas
                        historialFiltrado = cobroDAO.ObtenerCobrosPorRangoFechas(fechaDesde.Value, fechaHasta.Value);
                        // Agregar nombres de clientes a los cobros filtrados
                        CargarNombresClientes(historialFiltrado);
                    }
                }
                else
                {
                    // Filtrar en memoria si no se especifican ambas fechas
                    historialFiltrado = historialCompleto.Where(h =>
                    {
                        if (fechaDesde.HasValue && h.FechaCobro < fechaDesde.Value)
                            return false;
                        if (fechaHasta.HasValue && h.FechaCobro > fechaHasta.Value)
                            return false;
                        return true;
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar datos: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ActualizarDataGrid();
            CalcularTotales();
        }

        private void ExportarExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden exportar el historial de cobros.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx|Todos los archivos (*.*)|*.*",
                    DefaultExt = "xlsx",
                    FileName = cliente != null
                        ? $"Historial_Cobros_{cliente.Apellido}_{cliente.Nombre}_{DateTime.Now:yyyyMMdd}.xlsx"
                        : $"Historial_Cobros_General_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportarAExcel(saveFileDialog.FileName);
                    MessageBox.Show($"Historial exportado exitosamente a:\n{saveFileDialog.FileName}",
                                  "Exportación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el archivo:\n{ex.Message}",
                              "Error de Exportación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportarAExcel(string rutaArchivo)
        {
            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Historial Cobros");

            string[] columnas = { "Fecha", "Expediente", "Cliente", "DNI", "Tipo Trámite", "Impuestos", "Certificados", "Total", "Estado", "Método Pago", "Observaciones" };
            for (int i = 0; i < columnas.Length; i++)
                hoja.Cell(1, i + 1).Value = columnas[i];

            var encabezado = hoja.Range(1, 1, 1, columnas.Length);
            encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
            encabezado.Style.Font.FontColor = XLColor.White;
            encabezado.Style.Font.Bold = true;
            encabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int fila = 2;
            foreach (var cobro in historialFiltrado)
            {
                var clienteCobro = cliente ?? new Notalegis.Datos.Modelos.Cliente
                {
                    Id = cobro.ClienteId,
                    Nombre = "Cliente",
                    Apellido = "General",
                    DNI = "N/A"
                };

                if (cliente == null && cobro.ClienteId > 0)
                {
                    try
                    {
                        var clienteDAO = new Notalegis.Datos.Repositorios.ClienteDAO();
                        var clienteTemp = clienteDAO.ObtenerPorId(cobro.ClienteId);
                        if (clienteTemp != null) clienteCobro = clienteTemp;
                    }
                    catch { }
                }

                hoja.Cell(fila, 1).Value = cobro.FechaCobro.ToString("dd/MM/yyyy");
                hoja.Cell(fila, 2).Value = cobro.NumeroExpediente ?? "";
                hoja.Cell(fila, 3).Value = clienteCobro.NombreCompleto;
                hoja.Cell(fila, 4).Value = clienteCobro.DNI;
                hoja.Cell(fila, 5).Value = cobro.TipoTramite ?? "";
                hoja.Cell(fila, 6).Value = cobro.SubtotalImpuestos;
                hoja.Cell(fila, 7).Value = cobro.SubtotalCertificados;
                hoja.Cell(fila, 8).Value = cobro.TotalGeneral;
                hoja.Cell(fila, 9).Value = cobro.Estado ?? "";
                hoja.Cell(fila, 10).Value = cobro.MetodoPago ?? "";
                hoja.Cell(fila, 11).Value = cobro.Observaciones ?? "";

                if ((fila - 2) % 2 == 1)
                    hoja.Row(fila).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F9");

                fila++;
            }

            hoja.Columns().AdjustToContents();
            hoja.PageSetup.Footer.Right.AddText($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
            workbook.SaveAs(rutaArchivo);
        }

        private void CargarNombresClientes(List<Cobro> cobros)
        {
            var clienteDAO = new Notalegis.Datos.Repositorios.ClienteDAO();
            
            foreach (var cobro in cobros)
            {
                if (cliente != null)
                {
                    // Si ya tenemos el cliente específico, usar su nombre
                    cobro.Cliente = cliente.NombreCompleto;
                }
                else
                {
                    // Si es historial general, obtener el nombre del cliente por ID
                    try
                    {
                        var clienteCobro = clienteDAO.ObtenerPorId(cobro.ClienteId);
                        cobro.Cliente = clienteCobro?.NombreCompleto ?? "Cliente no encontrado";
                    }
                    catch
                    {
                        cobro.Cliente = "Error al cargar cliente";
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Configuraciones adicionales al cargar
        }
    }
}
