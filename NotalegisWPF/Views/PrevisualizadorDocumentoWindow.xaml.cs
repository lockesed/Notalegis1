using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Packaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Notalegis.Views
{
    public partial class PrevisualizadorDocumentoWindow : Window
    {
        private readonly string rutaArchivo;
        private PdfDocument? pdfDocumento;
        private uint paginaActual;
        private uint totalPaginas;

        public PrevisualizadorDocumentoWindow(string rutaArchivo)
        {
            InitializeComponent();
            this.rutaArchivo = rutaArchivo;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(rutaArchivo))
            {
                MessageBox.Show("El archivo no existe.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            txtTituloArchivo.Text = Path.GetFileName(rutaArchivo);
            string ext = Path.GetExtension(rutaArchivo).ToLowerInvariant();

            try
            {
                switch (ext)
                {
                    case ".pdf":
                        await CargarPdf();
                        break;
                    case ".docx":
                        CargarDocx();
                        break;
                    default:
                        CargarTxt();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar el archivo: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                borderCargando.Visibility = Visibility.Collapsed;
            }
        }

        // ── PDF ──────────────────────────────────────────────────────────────

        private async Task CargarPdf()
        {
            panelNavPaginas.Visibility = Visibility.Visible;

            var storageFile = await StorageFile.GetFileFromPathAsync(rutaArchivo);
            pdfDocumento = await PdfDocument.LoadFromFileAsync(storageFile);
            totalPaginas = pdfDocumento.PageCount;
            paginaActual = 0;

            scrollPdf.Visibility = Visibility.Visible;
            await RenderizarPagina(paginaActual);
        }

        private async Task RenderizarPagina(uint numero)
        {
            if (pdfDocumento == null) return;

            using var page = pdfDocumento.GetPage(numero);
            using var stream = new InMemoryRandomAccessStream();

            var options = new PdfPageRenderOptions
            {
                DestinationWidth = 1200
            };
            await page.RenderToStreamAsync(stream, options);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream.AsStream();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            imgPdf.Source = bitmap;
            txtPagina.Text = $"Página {numero + 1} de {totalPaginas}";
            btnPaginaAnterior.IsEnabled = numero > 0;
            btnPaginaSiguiente.IsEnabled = numero < totalPaginas - 1;
        }

        private async void btnPaginaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual > 0)
                await RenderizarPagina(--paginaActual);
        }

        private async void btnPaginaSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (paginaActual < totalPaginas - 1)
                await RenderizarPagina(++paginaActual);
        }

        // ── DOCX ─────────────────────────────────────────────────────────────

        private void CargarDocx()
        {
            try
            {
                using var doc = WordprocessingDocument.Open(rutaArchivo, false);
                var body = doc.MainDocumentPart?.Document?.Body;

                if (body == null)
                {
                    CargarTxt();
                    return;
                }

                var flowDoc = new FlowDocument
                {
                    FontSize = 13,
                    PagePadding = new Thickness(48),
                    Background = System.Windows.Media.Brushes.White,
                    LineHeight = 22
                };

                foreach (var para in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    string texto = string.Concat(
                        para.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                            .Select(t => t.Text));

                    var p = new Paragraph(new Run(texto))
                    {
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    flowDoc.Blocks.Add(p);
                }

                flowDocViewer.Document = flowDoc;
                flowDocViewer.Visibility = Visibility.Visible;
            }
            catch
            {
                // Si OpenXml falla (docx corrupto, protegido, etc.), mostrar como texto
                CargarTxt();
            }
        }

        // ── TXT ──────────────────────────────────────────────────────────────

        private void CargarTxt()
        {
            txtContenido.Text = File.ReadAllText(rutaArchivo);
            scrollTxt.Visibility = Visibility.Visible;
        }
    }
}
