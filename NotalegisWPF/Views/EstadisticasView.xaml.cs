using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClosedXML.Excel;
using Microsoft.Win32;
using Notalegis.Negocio.Servicios;
using ScottPlot;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Notalegis.Views
{
    public partial class EstadisticasView : Window
    {
        private readonly EstadisticasService estadisticasService = new EstadisticasService();
        private EstadisticaResultado _resultadoActual;
        private bool _cargando = false;

        public EstadisticasView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Cargar métricas en el ComboBox
            foreach (var kvp in EstadisticasService.MetricasDisponibles)
                cmbMetrica.Items.Add(new MetricaItem { Clave = kvp.Key, Nombre = kvp.Value });
            cmbMetrica.DisplayMemberPath = "Nombre";
            cmbMetrica.SelectedIndex = 0;

            // Inicializar fechas personalizadas
            dpDesde.SelectedDate = new DateTime(DateTime.Today.Year, 1, 1);
            dpHasta.SelectedDate = DateTime.Today;

            ActualizarGrafico();
        }

        private void ActualizarGrafico()
        {
            if (_cargando || cmbMetrica.SelectedItem == null) return;

            try
            {
                var filtro = ObtenerFiltroActual();
                _resultadoActual = estadisticasService.ObtenerDatos(filtro);

                txtTituloGrafico.Text = _resultadoActual.Titulo;
                txtSubtitulo.Text = $"  {filtro.Desde:dd/MM/yyyy} — {filtro.Hasta:dd/MM/yyyy}";

                // KPIs
                kpiPanel.ItemsSource = _resultadoActual.Kpis;

                // Tabla de datos
                dgDatos.ItemsSource = _resultadoActual.Datos;

                bool hasDatos = _resultadoActual.Datos?.Any() == true;
                bool soloTabla = rbSoloTabla.IsChecked == true;
                txtSinDatos.Visibility = hasDatos ? Visibility.Collapsed : Visibility.Visible;
                graficoPrincipal.Visibility = hasDatos && !soloTabla ? Visibility.Visible : Visibility.Collapsed;
                AjustarFilaGrafico(hasDatos, soloTabla);

                if (hasDatos && !soloTabla)
                {
                    // Sin esto, el control gráfico todavía puede tener el tamaño colapsado de un cambio
                    // de Visibility/fila anterior, y ScottPlot terminaba dibujando sobre un lienzo de 0px.
                    UpdateLayout();
                    RenderizarGrafico(filtro.TipoGrafico);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estadísticas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AjustarFilaGrafico(bool hasDatos, bool soloTabla)
        {
            if (rowGrafico == null || rowTabla == null || cardTabla == null) return;

            if (hasDatos && soloTabla)
            {
                // Sin gráfico: la tabla pasa a ocupar todo el espacio disponible en vez de quedar
                // apretada en su cajita chica de siempre con un hueco vacío abajo.
                rowGrafico.Height = new GridLength(0);
                rowTabla.Height = new GridLength(1, GridUnitType.Star);
                cardTabla.MaxHeight = double.PositiveInfinity;
            }
            else
            {
                // Con gráfico visible (o sin datos, donde hace falta lugar para el mensaje "Sin datos"):
                // el gráfico se lleva el espacio y la tabla vuelve a su alto acotado de siempre.
                rowGrafico.Height = new GridLength(1, GridUnitType.Star);
                rowTabla.Height = GridLength.Auto;
                cardTabla.MaxHeight = 220;
            }
        }

        private void RenderizarGrafico(string tipoGrafico)
        {
            // Se arranca con un Plot totalmente nuevo en cada render (no plot.Clear() sobre el mismo
            // objeto). Clear() sólo borra los datos dibujados, pero configuraciones como el marco/grilla
            // de la Torta, márgenes o límites de ejes quedaban pegadas de un gráfico al siguiente y
            // terminaban arruinando el próximo (barras/torta reducidas a una línea vertical).
            graficoPrincipal.Reset();
            var plot = graficoPrincipal.Plot;

            var datos = _resultadoActual.Datos;
            if (datos == null || !datos.Any()) return;

            var coloresPaleta = new[]
            {
                "#1976D2", "#388E3C", "#F57F17", "#7B1FA2", "#D32F2F",
                "#0288D1", "#558B2F", "#F9A825", "#6A1B9A", "#C62828",
                "#00838F", "#2E7D32", "#E65100", "#4527A0", "#AD1457"
            };

            switch (tipoGrafico)
            {
                case "Torta":
                    RenderizarTorta(plot, datos, coloresPaleta);
                    break;
                default: // Barras
                    RenderizarBarras(plot, datos, coloresPaleta);
                    break;
            }

            // Sin esto, el gráfico queda con los límites de ejes de la última vez que se calcularon
            // (o los de fábrica), por lo que barras/líneas con valores reales terminaban invisibles o gigantes.
            plot.Axes.AutoScale();

            plot.FigureBackground.Color = ScottPlot.Color.FromHex("#FAFAFA");
            graficoPrincipal.Refresh();
        }

        private void RenderizarBarras(Plot plot, List<PuntoEstadistica> datos, string[] colores)
        {
            var posiciones = Enumerable.Range(0, datos.Count).Select(i => (double)i).ToArray();
            var valores = datos.Select(d => d.Valor).ToArray();

            var barras = plot.Add.Bars(posiciones, valores);

            for (int i = 0; i < barras.Bars.Count && i < datos.Count; i++)
            {
                barras.Bars[i].FillColor = ScottPlot.Color.FromHex(colores[i % colores.Length]);
                barras.Bars[i].LineColor = ScottPlot.Color.FromHex(colores[i % colores.Length]);
            }

            // Con pocas categorías entra el nombre corto sin problema. Con muchas, un nombre rotado
            // se termina cortando o superponiendo igual, así que se numeran las barras (1, 2, 3…) —
            // el número coincide con el orden de la fila en la tabla de abajo, que ya tiene el nombre
            // y el monto exacto de cada una.
            bool pocasCategorias = datos.Count <= 6;
            var etiquetas = pocasCategorias
                ? datos.Select(d => d.Etiqueta.Length > 14 ? d.Etiqueta.Substring(0, 12) + "…" : d.Etiqueta).ToArray()
                : Enumerable.Range(1, datos.Count).Select(n => n.ToString()).ToArray();

            plot.Axes.Bottom.SetTicks(posiciones, etiquetas);
            plot.Axes.Bottom.TickLabelStyle.Rotation = 0;
            plot.Axes.Bottom.TickLabelStyle.FontSize = pocasCategorias ? 10 : 11;

            // Un poco de aire a los costados para que la primera y la última barra no queden pegadas al borde.
            plot.Axes.Margins(0.1, 0.2);
        }

        private void RenderizarTorta(Plot plot, List<PuntoEstadistica> datos, string[] colores)
        {
            // Las etiquetas puestas directamente sobre cada porción se pisaban entre sí cuando había
            // varias categorías; en su lugar se usa una leyenda al costado (una entrada por porción).
            var slices = datos.Select((d, i) => new ScottPlot.PieSlice
            {
                Value = d.Valor,
                LegendText = $"{d.Etiqueta} ({d.ValorFormateado})",
                FillColor = ScottPlot.Color.FromHex(colores[i % colores.Length])
            }).Where(s => s.Value > 0).ToList();

            if (slices.Any())
            {
                var pie = plot.Add.Pie(slices);
                pie.ExplodeFraction = 0.04;
            }
            plot.HideGrid();
            plot.Layout.Frameless();
            plot.ShowLegend(Alignment.UpperRight);
            plot.Legend.FontSize = 11;
        }

        private FiltroEstadisticas ObtenerFiltroActual()
        {
            var filtro = new FiltroEstadisticas
            {
                Metrica = (cmbMetrica.SelectedItem as MetricaItem)?.Clave ?? "TramitesPorEstado",
                TipoGrafico = rbTorta.IsChecked == true ? "Torta" : "Barras"
            };

            if (rbHoy.IsChecked == true)
            {
                filtro.Desde = DateTime.Today;
                filtro.Hasta = DateTime.Today;
            }
            else if (rbEstaSemana.IsChecked == true)
            {
                filtro.Desde = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
                filtro.Hasta = DateTime.Today;
            }
            else if (rbEsteMes.IsChecked == true)
            {
                filtro.Desde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                filtro.Hasta = DateTime.Today;
            }
            else if (rbEsteAnio.IsChecked == true)
            {
                filtro.Desde = new DateTime(DateTime.Today.Year, 1, 1);
                filtro.Hasta = DateTime.Today;
            }
            else if (rbPersonalizado.IsChecked == true)
            {
                filtro.Desde = dpDesde.SelectedDate ?? new DateTime(DateTime.Today.Year, 1, 1);
                filtro.Hasta = dpHasta.SelectedDate ?? DateTime.Today;
            }

            return filtro;
        }

        // Eventos de filtros
        private void Periodo_Changed(object sender, RoutedEventArgs e)
        {
            if (panelFechasPersonalizadas == null) return;
            panelFechasPersonalizadas.Visibility = rbPersonalizado.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            if (rbPersonalizado.IsChecked != true)
                ActualizarGrafico();
        }

        private void FechaPersonalizada_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (dpDesde.SelectedDate.HasValue && dpHasta.SelectedDate.HasValue)
                ActualizarGrafico();
        }

        private void cmbMetrica_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarGrafico();
        }

        private void dgDatos_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Mismo número que usan las barras cuando hay muchas categorías, para poder relacionar
            // rápido cuál barra corresponde a cuál fila.
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void TipoGrafico_Changed(object sender, RoutedEventArgs e)
        {
            if (graficoPrincipal == null) return;
            bool soloTabla = rbSoloTabla.IsChecked == true;
            bool hasDatos = _resultadoActual?.Datos?.Any() == true;
            graficoPrincipal.Visibility = soloTabla || !hasDatos ? Visibility.Collapsed : Visibility.Visible;
            AjustarFilaGrafico(hasDatos, soloTabla);

            if (!soloTabla && hasDatos)
            {
                UpdateLayout();
                string tipo = rbTorta.IsChecked == true ? "Torta" : "Barras";
                RenderizarGrafico(tipo);
            }
        }

        // Exportaciones
        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (_resultadoActual?.Datos == null || !_resultadoActual.Datos.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var filtro = ObtenerFiltroActual();
            var nombreArchivo = $"Notalegis_{filtro.Metrica}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            var dialog = new SaveFileDialog
            {
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = nombreArchivo,
                DefaultExt = "xlsx"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                using var workbook = new XLWorkbook();
                var hoja = workbook.Worksheets.Add("Estadísticas");

                // Título
                hoja.Cell(1, 1).Value = _resultadoActual.Titulo;
                hoja.Cell(2, 1).Value = $"Período: {filtro.Desde:dd/MM/yyyy} — {filtro.Hasta:dd/MM/yyyy}";
                hoja.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
                hoja.Range(1, 1, 1, 2).Style.Font.Bold = true;
                hoja.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                hoja.Range(1, 1, 1, 2).Style.Font.FontColor = XLColor.White;

                // KPIs si se pidieron
                if (chkIncluirGrafico.IsChecked == true && _resultadoActual.Kpis?.Any() == true)
                {
                    int kpiRow = 5;
                    hoja.Cell(kpiRow, 1).Value = "RESUMEN";
                    hoja.Cell(kpiRow, 1).Style.Font.Bold = true;
                    kpiRow++;
                    foreach (var kpi in _resultadoActual.Kpis)
                    {
                        hoja.Cell(kpiRow, 1).Value = kpi.Etiqueta;
                        hoja.Cell(kpiRow, 2).Value = kpi.Valor;
                        kpiRow++;
                    }
                }

                // Tabla de datos si se pidió
                if (chkIncluirTabla.IsChecked == true)
                {
                    int startRow = 10;
                    hoja.Cell(startRow, 1).Value = "Categoría";
                    hoja.Cell(startRow, 2).Value = "Valor";
                    var enc = hoja.Range(startRow, 1, startRow, 2);
                    enc.Style.Fill.BackgroundColor = XLColor.FromHtml("#1976D2");
                    enc.Style.Font.FontColor = XLColor.White;
                    enc.Style.Font.Bold = true;

                    for (int i = 0; i < _resultadoActual.Datos.Count; i++)
                    {
                        int fila = startRow + 1 + i;
                        hoja.Cell(fila, 1).Value = _resultadoActual.Datos[i].Etiqueta;
                        hoja.Cell(fila, 2).Value = _resultadoActual.Datos[i].ValorFormateado;
                        if (i % 2 == 1)
                            hoja.Row(fila).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F9");
                    }
                }

                // Imagen del gráfico si se pidió y el WpfPlot está visible
                if (chkIncluirGrafico.IsChecked == true && graficoPrincipal.Visibility == Visibility.Visible)
                {
                    try
                    {
                        var imgBytes = graficoPrincipal.Plot.GetImageBytes(600, 350, ImageFormat.Png);
                        using var ms = new MemoryStream(imgBytes);
                        var imgXL = hoja.AddPicture(ms);
                        imgXL.MoveTo(hoja.Cell(14, 1)).Scale(0.8);
                    }
                    catch { }
                }

                hoja.Columns().AdjustToContents();
                hoja.PageSetup.Footer.Right.AddText($"Notalegis — {DateTime.Now:dd/MM/yyyy HH:mm}");
                workbook.SaveAs(dialog.FileName);

                MessageBox.Show("Excel exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                if (MessageBox.Show("¿Abrir el archivo?", "Abrir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            if (_resultadoActual?.Datos == null || !_resultadoActual.Datos.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var filtro = ObtenerFiltroActual();
            var dialog = new SaveFileDialog
            {
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                FileName = $"Notalegis_{filtro.Metrica}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                DefaultExt = "pdf"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                GenerarPDF(dialog.FileName, filtro);
                MessageBox.Show("PDF exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                if (MessageBox.Show("¿Abrir el archivo?", "Abrir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarPDF(string rutaArchivo, FiltroEstadisticas filtro)
        {
            var documento = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(documento, new FileStream(rutaArchivo, FileMode.Create));
            documento.Open();

            var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(0, 0, 0));
            var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 11, new BaseColor(0, 0, 0));
            var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 0, 0));

            documento.Add(new Paragraph(_resultadoActual.Titulo, fuenteTitulo) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 6f });
            documento.Add(new Paragraph($"Período: {filtro.Desde:dd/MM/yyyy} — {filtro.Hasta:dd/MM/yyyy}", fuenteNormal) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 4f });
            documento.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", fuenteNormal) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20f });

            if (chkIncluirGrafico.IsChecked == true && _resultadoActual.Kpis?.Any() == true)
            {
                var tablaKpi = new PdfPTable(_resultadoActual.Kpis.Count) { WidthPercentage = 100, SpacingAfter = 20f };
                foreach (var kpi in _resultadoActual.Kpis)
                {
                    tablaKpi.AddCell(new PdfPCell(new Phrase($"{kpi.Valor}\n{kpi.Etiqueta}", fuenteNegrita))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 10,
                        Border = Rectangle.NO_BORDER
                    });
                }
                documento.Add(tablaKpi);
            }

            if (chkIncluirGrafico.IsChecked == true && graficoPrincipal.Visibility == Visibility.Visible)
            {
                try
                {
                    var imgBytes = graficoPrincipal.Plot.GetImageBytes(600, 350, ImageFormat.Png);
                    var imagen = iTextSharp.text.Image.GetInstance(imgBytes);
                    imagen.ScaleToFit(480, 280);
                    imagen.Alignment = Element.ALIGN_CENTER;
                    imagen.SpacingAfter = 20f;
                    documento.Add(imagen);
                }
                catch { }
            }

            if (chkIncluirTabla.IsChecked == true)
            {
                var tablaDatos = new PdfPTable(2) { WidthPercentage = 100, SpacingBefore = 10f };
                tablaDatos.SetWidths(new float[] { 3f, 1f });

                tablaDatos.AddCell(new PdfPCell(new Phrase("Categoría", fuenteNegrita)) { BackgroundColor = new BaseColor(33, 150, 243), Padding = 8, HorizontalAlignment = Element.ALIGN_CENTER });
                tablaDatos.AddCell(new PdfPCell(new Phrase("Valor", fuenteNegrita)) { BackgroundColor = new BaseColor(33, 150, 243), Padding = 8, HorizontalAlignment = Element.ALIGN_CENTER });

                foreach (var d in _resultadoActual.Datos)
                {
                    tablaDatos.AddCell(new PdfPCell(new Phrase(d.Etiqueta, fuenteNormal)) { Padding = 6 });
                    tablaDatos.AddCell(new PdfPCell(new Phrase(d.ValorFormateado, fuenteNormal)) { Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                }
                documento.Add(tablaDatos);
            }

            documento.Add(new Paragraph(" "));
            documento.Add(new Paragraph("Notalegis - Sistema de Gestión para Escribanías", fuenteNormal) { Alignment = Element.ALIGN_CENTER, SpacingBefore = 20f });

            documento.Close();
            writer.Close();
        }

        private void btnExportarCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_resultadoActual?.Datos == null || !_resultadoActual.Datos.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var filtro = ObtenerFiltroActual();
            var dialog = new SaveFileDialog
            {
                Filter = "Archivos CSV (*.csv)|*.csv",
                FileName = $"Notalegis_{filtro.Metrica}_{DateTime.Now:yyyyMMdd}.csv",
                DefaultExt = "csv"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"{_resultadoActual.Titulo}");
                sb.AppendLine($"Período: {filtro.Desde:dd/MM/yyyy} - {filtro.Hasta:dd/MM/yyyy}");
                sb.AppendLine();
                sb.AppendLine("Categoría,Valor");
                foreach (var d in _resultadoActual.Datos)
                    sb.AppendLine($"\"{d.Etiqueta}\",\"{d.ValorFormateado}\"");

                File.WriteAllText(dialog.FileName, sb.ToString(), System.Text.Encoding.UTF8);
                MessageBox.Show("CSV exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar CSV: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e) => Close();

        private class MetricaItem
        {
            public string Clave { get; set; }
            public string Nombre { get; set; }
        }
    }
}
