using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Windows.Documents;
using System.Windows.Media;
using System.Printing;
using iTextParagraph = iTextSharp.text.Paragraph;
using iTextDocument = iTextSharp.text.Document;
using iTextFont = iTextSharp.text.Font;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Notalegis.Views
{
    public partial class PedidosInsumosView : Window
    {
        private readonly PedidoInsumoService pedidoService = new PedidoInsumoService();
        private readonly ConfiguracionService confService = new ConfiguracionService();

        private readonly List<string> estados = new List<string>
        {
            "Todos",
            "Pendiente",
            "Enviado",
            "Recibido",
            "Cancelado"
        };

        public PedidosInsumosView()
        {
            InitializeComponent();
            CargarCombos();
            CargarPedidos();
            CargarAlertas();
            CargarStockActual();
        }

        private void CargarStockActual()
        {
            var stock = pedidoService.ObtenerStockPorTipo("Hojas Protocolares");
            if (stock != null)
            {
                txtCantidadActual.Text = stock.CantidadActual.ToString();
                txtStockMinimo.Text = stock.StockMinimo.ToString();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpFechaPedido.SelectedDate = DateTime.Today;
            cmbEstado.SelectedIndex = 0; // Pendiente
            cmbFiltroEstado.SelectedIndex = 0; // Todos
        }

        private void CargarCombos()
        {
            cmbEstado.ItemsSource = estados.Where(e => e != "Todos").ToList();
            cmbFiltroEstado.ItemsSource = estados;
        }

        private void CargarPedidos()
        {
            var lista = pedidoService.ObtenerTodos();
            dgPedidos.ItemsSource = lista;
            borderSinPedidos.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CargarAlertas()
        {
            var stockBajo = pedidoService.ObtenerStockBajo();
            lstAlertas.ItemsSource = stockBajo;

            if (stockBajo.Count > 0)
            {
                // Mostrar notificación
                string mensaje = $"¡ALERTA! Hay {stockBajo.Count} insumo(s) con stock bajo:\n\n";
                foreach (var item in stockBajo)
                {
                    mensaje += $"• {item.TipoInsumo}: {item.CantidadActual} unidades (Mínimo: {item.StockMinimo})\n";
                }
                
                // No mostrar MessageBox automáticamente, solo cargar en la lista
            }
        }

        private void LimpiarFormulario()
        {
            dpFechaPedido.SelectedDate = DateTime.Today;
            txtTipoInsumo.Text = "Hojas Protocolares";
            txtCantidad.Text = "";
            txtSolicitante.Text = "";
            cmbEstado.SelectedIndex = 0;
            txtEmailDestino.Text = "";
            txtObservaciones.Text = "";
            chkFirmado.IsChecked = false;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (dpFechaPedido.SelectedDate == null)
            {
                MessageBox.Show("La fecha del pedido es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // El tipo de insumo siempre es "Hojas Protocolares", no necesita validación

            if (string.IsNullOrWhiteSpace(txtCantidad.Text) || !int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Debe ingresar una cantidad válida mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSolicitante.Text))
            {
                MessageBox.Show("El solicitante es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbEstado.Text))
            {
                MessageBox.Show("El estado es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Este formulario siempre crea un pedido nuevo. La edición de un pedido
            // existente se hace exclusivamente desde el botón "Editar" (EditarPedidoWindow).
            var pedido = new PedidoInsumo
            {
                FechaPedido = dpFechaPedido.SelectedDate.Value,
                TipoInsumo = "Hojas Protocolares",
                Cantidad = cantidad,
                Solicitante = txtSolicitante.Text,
                Estado = cmbEstado.Text,
                EmailDestino = txtEmailDestino.Text,
                Observaciones = txtObservaciones.Text,
                Firmado = chkFirmado.IsChecked == true
            };

            pedidoService.GuardarPedido(pedido, SesionActual.NombreUsuario);
            MessageBox.Show("Pedido guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            CargarPedidos();
            LimpiarFormulario();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.DataContext is PedidoInsumo pedido)
            {
                // Abrir ventana de edición en lugar de cargar en el formulario
                var ventanaEditar = new EditarPedidoWindow(pedido);
                if (ventanaEditar.ShowDialog() == true)
                {
                    CargarPedidos();
                    CargarAlertas();
                }
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.DataContext is PedidoInsumo pedido)
            {
                var resultado = MessageBox.Show($"¿Está seguro que desea eliminar el pedido ID {pedido.Id}?",
                    "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    pedidoService.EliminarPedido(pedido.Id, SesionActual.NombreUsuario);
                    MessageBox.Show("Pedido eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CargarPedidos();
                    LimpiarFormulario();
                }
            }
        }

        private void btnEmitir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.DataContext is PedidoInsumo pedido)
            {
                EmitirPedido(pedido);
            }
        }

        private void EmitirPedido(PedidoInsumo pedido)
        {
            FlowDocument doc = CrearDocumentoImpresion(pedido);
            doc.PageWidth = 794; // ancho aproximado de una hoja A4 a 96 DPI
            doc.PagePadding = new Thickness(50);

            var visor = new FlowDocumentReader
            {
                Document = doc,
                ViewingMode = FlowDocumentReaderViewingMode.Page
            };

            var ventanaPreview = new Window
            {
                Title = $"Vista previa — Pedido de Hojas Protocolares N° {pedido.Id}",
                Content = visor,
                Width = 850,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            ventanaPreview.ShowDialog();
        }

        private FlowDocument CrearDocumentoImpresion(PedidoInsumo pedido)
        {
            string escribaniaNombre = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "ESCRIBANÍA EJEMPLO");
            string escribaniaDireccion = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaDireccion, "Calle Falsa 123");

            FlowDocument doc = new FlowDocument();
            
            // Título
            System.Windows.Documents.Paragraph titulo = new System.Windows.Documents.Paragraph(new Run("PEDIDO DE HOJAS PROTOCOLARES"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            doc.Blocks.Add(titulo);

            System.Windows.Documents.Paragraph subtitulo = new System.Windows.Documents.Paragraph(new Run(escribaniaNombre.ToUpper()))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 5)
            };
            doc.Blocks.Add(subtitulo);

            System.Windows.Documents.Paragraph direccion = new System.Windows.Documents.Paragraph(new Run(escribaniaDireccion))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            };
            doc.Blocks.Add(direccion);

            // Línea separadora
            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new Run(new string('─', 80)))
            {
                FontSize = 10,
                Margin = new Thickness(0, 10, 0, 10)
            });

            // Información del pedido
            System.Windows.Documents.Paragraph infoPedido = new System.Windows.Documents.Paragraph();
            infoPedido.Inlines.Add(new Run($"PEDIDO N°: ") { FontWeight = FontWeights.Bold });
            infoPedido.Inlines.Add(new Run($"{pedido.Id}\n"));
            infoPedido.Inlines.Add(new Run($"FECHA: ") { FontWeight = FontWeights.Bold });
            infoPedido.Inlines.Add(new Run($"{pedido.FechaPedido:dd/MM/yyyy}\n\n"));
            infoPedido.FontSize = 12;
            doc.Blocks.Add(infoPedido);

            // Detalle del pedido
            System.Windows.Documents.Paragraph detalleTitulo = new System.Windows.Documents.Paragraph(new Run("DETALLE DEL PEDIDO"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };
            doc.Blocks.Add(detalleTitulo);

            System.Windows.Documents.Paragraph detalle = new System.Windows.Documents.Paragraph();
            detalle.Inlines.Add(new Run($"Tipo de Insumo: ") { FontWeight = FontWeights.Bold });
            detalle.Inlines.Add(new Run($"{pedido.TipoInsumo}\n"));
            detalle.Inlines.Add(new Run($"Cantidad Solicitada: ") { FontWeight = FontWeights.Bold });
            detalle.Inlines.Add(new Run($"{pedido.Cantidad} unidades\n\n"));
            detalle.FontSize = 12;
            doc.Blocks.Add(detalle);

            // Información del solicitante
            System.Windows.Documents.Paragraph solicitanteTitulo = new System.Windows.Documents.Paragraph(new Run("INFORMACIÓN DEL SOLICITANTE"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };
            doc.Blocks.Add(solicitanteTitulo);

            System.Windows.Documents.Paragraph solicitante = new System.Windows.Documents.Paragraph();
            solicitante.Inlines.Add(new Run($"Solicitante: ") { FontWeight = FontWeights.Bold });
            solicitante.Inlines.Add(new Run($"{pedido.Solicitante}\n"));
            solicitante.Inlines.Add(new Run($"Estado: ") { FontWeight = FontWeights.Bold });
            solicitante.Inlines.Add(new Run($"{pedido.Estado}\n"));
            solicitante.FontSize = 12;
            doc.Blocks.Add(solicitante);

            // Observaciones
            if (!string.IsNullOrWhiteSpace(pedido.Observaciones))
            {
                System.Windows.Documents.Paragraph obsTitulo = new System.Windows.Documents.Paragraph(new Run("OBSERVACIONES"))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 20, 0, 10)
                };
                doc.Blocks.Add(obsTitulo);

                System.Windows.Documents.Paragraph obs = new System.Windows.Documents.Paragraph(new Run(pedido.Observaciones))
                {
                    FontSize = 12,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                doc.Blocks.Add(obs);
            }

            // Firma
            System.Windows.Documents.Paragraph firmaTitulo = new System.Windows.Documents.Paragraph(new Run("FIRMA Y ACLARACIÓN"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 40, 0, 20)
            };
            doc.Blocks.Add(firmaTitulo);

            System.Windows.Documents.Paragraph firma = new System.Windows.Documents.Paragraph();
            firma.Inlines.Add(new Run("Firma: _______________________________\n\n"));
            firma.Inlines.Add(new Run($"Aclaración: {pedido.Solicitante}\n\n"));
            firma.FontSize = 12;
            doc.Blocks.Add(firma);

            // Pie de página
            System.Windows.Documents.Paragraph pie = new System.Windows.Documents.Paragraph(new Run($"\nGenerado el: {DateTime.Now:dd/MM/yyyy HH:mm}"))
            {
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 30, 0, 0)
            };
            doc.Blocks.Add(pie);

            return doc;
        }

        private void btnEmitirListadoMensual_Click(object sender, RoutedEventArgs e)
        {
            var ventanaSeleccion = new SeleccionMesWindow();
            if (ventanaSeleccion.ShowDialog() == true)
            {
                int mes = ventanaSeleccion.MesSeleccionado;
                int anio = ventanaSeleccion.AnioSeleccionado;

                var pedidosMes = pedidoService.ObtenerPedidosDelMes(mes, anio);

                if (pedidosMes.Count == 0)
                {
                    MessageBox.Show($"No hay pedidos registrados para {ventanaSeleccion.NombreMes} de {anio}.",
                        "Sin Resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                GenerarListadoMensualPDF(pedidosMes, mes, anio, ventanaSeleccion.NombreMes);
            }
        }

        private void GenerarListadoMensualPDF(List<PedidoInsumo> pedidos, int mes, int anio, string nombreMes)
        {
            try
            {
                string escribaniaNombre = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "ESCRIBANÍA EJEMPLO");
                string escribaniaDireccion = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaDireccion, "Calle Falsa 123");

                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                    $"Listado_Pedidos_Hojas_Protocolares_{mes}_{anio}.pdf");

                iTextDocument document = new iTextDocument(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
                
                document.Open();

                // Fuentes
                BaseFont bfBold = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                BaseFont bfNormal = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                
                iTextFont fontTitulo = new iTextFont(bfBold, 18, iTextFont.BOLD, BaseColor.Black);
                iTextFont fontSubtitulo = new iTextFont(bfBold, 14, iTextFont.BOLD, BaseColor.Black);
                iTextFont fontNormal = new iTextFont(bfNormal, 11, iTextFont.NORMAL, BaseColor.Black);
                iTextFont fontBold = new iTextFont(bfBold, 11, iTextFont.BOLD, BaseColor.Black);
                iTextFont fontSmall = new iTextFont(bfNormal, 9, iTextFont.ITALIC, BaseColor.Gray);

                // Título
                iTextParagraph titulo = new iTextParagraph("LISTADO MENSUAL DE PEDIDOS\nHOJAS PROTOCOLARES", fontTitulo);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 10;
                document.Add(titulo);

                // Subtítulo con mes y año
                iTextParagraph subtitulo = new iTextParagraph($"{nombreMes.ToUpper()} {anio}", fontSubtitulo);
                subtitulo.Alignment = Element.ALIGN_CENTER;
                subtitulo.SpacingAfter = 5;
                document.Add(subtitulo);

                // Información de la escribanía
                iTextParagraph escribania = new iTextParagraph($"{escribaniaNombre.ToUpper()}\n{escribaniaDireccion}", fontNormal);
                escribania.Alignment = Element.ALIGN_CENTER;
                escribania.SpacingAfter = 20;
                document.Add(escribania);

                // Línea separadora
                iTextSharp.text.pdf.draw.LineSeparator line = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, -2);
                document.Add(new Chunk(line));
                document.Add(new iTextParagraph(" "));

                // Total de pedidos
                iTextParagraph totalPedidos = new iTextParagraph($"Total de Pedidos: {pedidos.Count}", fontBold);
                totalPedidos.SpacingAfter = 15;
                document.Add(totalPedidos);

                // Tabla de pedidos
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 10f, 15f, 12f, 25f, 15f, 15f });
                table.SpacingBefore = 10;

                // Encabezados de la tabla
                string[] headers = { "N°", "Fecha", "Cantidad", "Solicitante", "Estado", "Firmado" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, fontBold));
                    cell.BackgroundColor = new BaseColor(200, 200, 200);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                // Datos de los pedidos
                int totalCantidad = 0;
                foreach (var pedido in pedidos)
                {
                    table.AddCell(new PdfPCell(new Phrase(pedido.Id.ToString(), fontNormal)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pedido.FechaPedido.ToString("dd/MM/yyyy"), fontNormal)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pedido.Cantidad.ToString(), fontNormal)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    table.AddCell(new PdfPCell(new Phrase(pedido.Solicitante, fontNormal)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pedido.Estado, fontNormal)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(pedido.Firmado ? "SÍ" : "NO", fontNormal)) { Padding = 5, HorizontalAlignment = Element.ALIGN_CENTER });
                    
                    totalCantidad += pedido.Cantidad;
                }

                document.Add(table);

                // Total de hojas solicitadas
                iTextParagraph totalHojas = new iTextParagraph($"\nTotal de Hojas Protocolares Solicitadas: {totalCantidad} unidades", fontBold);
                totalHojas.SpacingBefore = 15;
                document.Add(totalHojas);

                // Observaciones si las hay
                var pedidosConObs = pedidos.Where(p => !string.IsNullOrWhiteSpace(p.Observaciones)).ToList();
                if (pedidosConObs.Count > 0)
                {
                    document.Add(new iTextParagraph(" "));
                    document.Add(new Chunk(line));
                    iTextParagraph obsTitle = new iTextParagraph("\nOBSERVACIONES", fontBold);
                    document.Add(obsTitle);

                    foreach (var pedido in pedidosConObs)
                    {
                        iTextParagraph obs = new iTextParagraph($"Pedido N° {pedido.Id}: {pedido.Observaciones}", fontNormal);
                        obs.SpacingBefore = 5;
                        document.Add(obs);
                    }
                }

                // Pie de página
                document.Add(new iTextParagraph(" "));
                document.Add(new Chunk(line));
                iTextParagraph pie = new iTextParagraph($"\nGenerado el: {DateTime.Now:dd/MM/yyyy HH:mm}", fontSmall);
                pie.Alignment = Element.ALIGN_CENTER;
                document.Add(pie);

                document.Close();
                writer.Close();

                MessageBox.Show($"Listado mensual generado exitosamente.\n\nArchivo: {ruta}", 
                    "PDF Generado", MessageBoxButton.OK, MessageBoxImage.Information);

                // Abrir el PDF
                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEnviarEmail_Click(object sender, RoutedEventArgs e)
        {
            if (dgPedidos.SelectedItem is PedidoInsumo pedido)
            {
                if (string.IsNullOrWhiteSpace(pedido.EmailDestino))
                {
                    MessageBox.Show("El pedido seleccionado no tiene un email de destino configurado.",
                        "Email Requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string asunto = $"Pedido de Insumos N° {pedido.Id} - {pedido.TipoInsumo}";
                string cuerpo = GenerarCuerpoEmail(pedido);

                string mailto = $"mailto:{pedido.EmailDestino}?subject={Uri.EscapeDataString(asunto)}&body={Uri.EscapeDataString(cuerpo)}";

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = mailto,
                        UseShellExecute = true
                    });

                    // Actualizar fecha de envío
                    pedido.FechaEnvio = DateTime.Now;
                    if (pedido.Estado == "Pendiente")
                        pedido.Estado = "Enviado";
                    
                    pedidoService.ModificarPedido(pedido, SesionActual.NombreUsuario);
                    CargarPedidos();

                    MessageBox.Show("Cliente de email abierto correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir el cliente de email: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un pedido para enviar por email.", "Selección Requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GenerarCuerpoEmail(PedidoInsumo pedido)
        {
            string escribaniaNombre = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "Escribanía Ejemplo");
            string escribaniaDireccion = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaDireccion, "Calle Falsa 123");

            return $@"Estimados,

Por medio de la presente, solicitamos el siguiente pedido de insumos:

PEDIDO N°: {pedido.Id}
FECHA: {pedido.FechaPedido:dd/MM/yyyy}

DETALLE:
- Tipo de Insumo: {pedido.TipoInsumo}
- Cantidad: {pedido.Cantidad} unidades
- Solicitante: {pedido.Solicitante}

{(string.IsNullOrWhiteSpace(pedido.Observaciones) ? "" : $"OBSERVACIONES:\n{pedido.Observaciones}\n")}

Quedamos a la espera de su confirmación.

Saludos cordiales,
{escribaniaNombre}
{escribaniaDireccion}";
        }

        private void btnActualizarStock_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtCantidadActual.Text, out int cantidadActual) || cantidadActual < 0)
            {
                MessageBox.Show("Debe ingresar una cantidad actual válida.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStockMinimo.Text, out int stockMinimo) || stockMinimo < 0)
            {
                MessageBox.Show("Debe ingresar un stock mínimo válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var stock = new StockInsumo
            {
                TipoInsumo = "Hojas Protocolares",
                CantidadActual = cantidadActual,
                StockMinimo = stockMinimo,
                UltimaActualizacion = DateTime.Now
            };

            pedidoService.GuardarOActualizarStock(stock);
            MessageBox.Show("Stock de Hojas Protocolares actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // Recargar los campos con el valor real recién guardado (no vaciarlos)
            CargarStockActual();

            // Recargar alertas
            CargarAlertas();
        }

        private void btnVerStock_Click(object sender, RoutedEventArgs e)
        {
            var ventanaStock = new VerStockWindow();
            ventanaStock.ShowDialog();
            CargarAlertas(); // Recargar por si hubo cambios
        }

        private void btnVerificarAlertas_Click(object sender, RoutedEventArgs e)
        {
            CargarAlertas();
            
            var stockBajo = pedidoService.ObtenerStockBajo();
            if (stockBajo.Count > 0)
            {
                string mensaje = $"¡ALERTA! Hay {stockBajo.Count} insumo(s) con stock bajo:\n\n";
                foreach (var item in stockBajo)
                {
                    mensaje += $"• {item.TipoInsumo}: {item.CantidadActual} unidades (Mínimo: {item.StockMinimo})\n";
                }
                MessageBox.Show(mensaje, "Alertas de Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("No hay alertas de stock bajo en este momento.", "Sin Alertas", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarPedidos();
        }

        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarPedidos();
        }

        private void FiltrarPedidos()
        {
            string textoBuscar = txtBuscar.Text.Trim().ToLower();
            string estadoSeleccionado = cmbFiltroEstado.SelectedItem as string;

            var pedidos = pedidoService.ObtenerTodos();

            var filtrados = pedidos.Where(p =>
            {
                bool coincideTexto = string.IsNullOrEmpty(textoBuscar) ||
                    p.Solicitante.ToLower().Contains(textoBuscar) ||
                    p.TipoInsumo.ToLower().Contains(textoBuscar);

                bool coincideEstado = estadoSeleccionado == "Todos" || p.Estado == estadoSeleccionado;

                return coincideTexto && coincideEstado;
            }).ToList();

            dgPedidos.ItemsSource = filtrados;
            borderSinPedidos.Visibility = filtrados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnActualizarLista_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            cmbFiltroEstado.SelectedIndex = 0;
            CargarPedidos();
            CargarAlertas();
        }

        private void txtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
