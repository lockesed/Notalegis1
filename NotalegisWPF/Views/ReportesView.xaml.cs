using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class ReportesView : Window
    {
        private readonly ReportesService _service = new ReportesService();
        private DataTable? _tablaActual;
        private string _claveActual = "";

        public ReportesView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lstReportes.ItemsSource = ReportesService.Reportes;

            dpDesde.SelectedDate = new DateTime(DateTime.Today.Year, 1, 1);
            dpHasta.SelectedDate = DateTime.Today;
        }

        private void lstReportes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var def = lstReportes.SelectedItem as ReporteDefinicion;
            if (def == null) return;

            _claveActual = def.Clave;
            txtTituloReporte.Text = def.Nombre;
            txtDescripcionReporte.Text = def.Descripcion;
            panelFechas.IsEnabled = def.UsaFechas;
            panelFechas.Opacity = def.UsaFechas ? 1.0 : 0.4;

            EjecutarReporte();
        }

        private void EjecutarReporte()
        {
            if (string.IsNullOrEmpty(_claveActual)) return;

            try
            {
                var desde = dpDesde.SelectedDate ?? new DateTime(DateTime.Today.Year, 1, 1);
                var hasta = dpHasta.SelectedDate ?? DateTime.Today;

                _tablaActual = _service.GenerarReporte(_claveActual, desde, hasta);

                MostrarTabla(_tablaActual);
                MostrarTotales(_tablaActual);
                btnExportarExcel.IsEnabled = _tablaActual != null && _tablaActual.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarTabla(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                dgReporte.Visibility = Visibility.Collapsed;
                panelVacio.Visibility = Visibility.Visible;
                txtConteoFilas.Text = "0";
                return;
            }

            dgReporte.Columns.Clear();
            dgReporte.ItemsSource = null;
            dgReporte.AutoGenerateColumns = true;
            dgReporte.ItemsSource = dt.DefaultView;
            dgReporte.Visibility = Visibility.Visible;
            panelVacio.Visibility = Visibility.Collapsed;

            txtConteoFilas.Text = dt.Rows.Count.ToString("N0");
        }

        private void MostrarTotales(DataTable dt)
        {
            bool esFacturacion = _claveActual is "FacturacionPeriodo" or "CobrosPendientes";
            panelTotales.Visibility = esFacturacion ? Visibility.Visible : Visibility.Collapsed;

            if (!esFacturacion || dt == null || dt.Rows.Count == 0) return;

            // La columna "Total ($)" siempre es la última en los reportes de facturación
            string colTotal = "Total ($)";
            if (!dt.Columns.Contains(colTotal)) return;

            decimal total = dt.AsEnumerable().Sum(r =>
            {
                if (decimal.TryParse(r[colTotal]?.ToString()?.Replace(",", ""), out var v)) return v;
                return 0;
            });
            txtTotal.Text = $"${total:N0}";

            // Totales por estado si hay columna Estado
            if (dt.Columns.Contains("Estado"))
            {
                decimal pagado = dt.AsEnumerable()
                    .Where(r => r["Estado"]?.ToString() == "Pagado")
                    .Sum(r => decimal.TryParse(r[colTotal]?.ToString()?.Replace(",", ""), out var v) ? v : 0);
                decimal pendiente = dt.AsEnumerable()
                    .Where(r => r["Estado"]?.ToString() is "Pendiente" or "Parcial")
                    .Sum(r => decimal.TryParse(r[colTotal]?.ToString()?.Replace(",", ""), out var v) ? v : 0);

                txtTotalPagado.Text = pagado > 0 ? $"Pagado: ${pagado:N0}" : "";
                txtTotalPendiente.Text = pendiente > 0 ? $"Pendiente: ${pendiente:N0}" : "";
            }
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_tablaActual == null || _tablaActual.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"Notalegis_{_claveActual}_{DateTime.Today:yyyyMMdd}.xlsx",
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                using var wb = new XLWorkbook();
                var def = ReportesService.Reportes.FirstOrDefault(r => r.Clave == _claveActual);
                var ws = wb.AddWorksheet(def?.Nombre?.Length > 30 ? def.Nombre[..30] : def?.Nombre ?? "Reporte");

                // Título
                ws.Cell(1, 1).Value = def?.Nombre ?? "Reporte";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;

                ws.Cell(2, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Cell(2, 1).Style.Font.Italic = true;
                ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

                // Encabezados
                int row = 4;
                for (int col = 0; col < _tablaActual.Columns.Count; col++)
                {
                    var cell = ws.Cell(row, col + 1);
                    cell.Value = _tablaActual.Columns[col].ColumnName;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1B2A44");
                    cell.Style.Font.FontColor = XLColor.White;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Datos
                row = 5;
                foreach (DataRow dr in _tablaActual.Rows)
                {
                    for (int col = 0; col < _tablaActual.Columns.Count; col++)
                        ws.Cell(row, col + 1).Value = dr[col]?.ToString() ?? "";
                    row++;
                }

                // Autoajuste y borde
                var rangoUsado = ws.RangeUsed();
                if (rangoUsado != null)
                {
                    rangoUsado.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rangoUsado.SetAutoFilter();
                }
                ws.Columns().AdjustToContents();

                // Totales de facturación
                if (_claveActual is "FacturacionPeriodo" or "CobrosPendientes")
                {
                    ws.Cell(row + 1, 1).Value = "TOTAL";
                    ws.Cell(row + 1, 1).Style.Font.Bold = true;
                    ws.Cell(row + 1, _tablaActual.Columns.Count).Value = txtTotal.Text;
                    ws.Cell(row + 1, _tablaActual.Columns.Count).Style.Font.Bold = true;
                }

                wb.SaveAs(dialog.FileName);

                var res = MessageBox.Show("Reporte exportado exitosamente.\n¿Desea abrir el archivo?",
                    "Exportación completada", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e) => EjecutarReporte();

        private void Fecha_Changed(object sender, SelectionChangedEventArgs e) => EjecutarReporte();

        private void btnCerrar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
