using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Notalegis.Datos.Modelos;
using NotalegisWPF.Views;

namespace NotalegisWPF.Services
{
    public class PDFGenerator
    {

        public static void GenerarPresupuestoPDFCompleto(Cliente cliente, string numeroExpediente, DateTime fechaCobro, decimal totalGeneral, FacturaModal datosFactura, Cobro cobro)
        {
            try
            {
                // Crear carpeta de presupuestos si no existe
                string carpetaPresupuestos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Notalegis", "Presupuestos");
                if (!Directory.Exists(carpetaPresupuestos))
                {
                    Directory.CreateDirectory(carpetaPresupuestos);
                }

                // Limpiar caracteres inválidos del nombre del cliente
                string clienteNombreLimpio = cliente.NombreCompleto
                    .Replace(" ", "_")
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(":", "_")
                    .Replace("*", "_")
                    .Replace("?", "_")
                    .Replace("\"", "_")
                    .Replace("<", "_")
                    .Replace(">", "_")
                    .Replace("|", "_");

                // Nombre del archivo
                string nombreArchivo = $"Presupuesto_Completo_{clienteNombreLimpio}_{numeroExpediente}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string rutaCompleta = Path.Combine(carpetaPresupuestos, nombreArchivo);

                // Crear documento PDF
                Document documento = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(documento, new FileStream(rutaCompleta, FileMode.Create));

                documento.Open();

                // Fuentes
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font fontTitulo = new Font(baseFont, 18, Font.BOLD);
                Font fontSubtitulo = new Font(baseFont, 14, Font.BOLD);
                Font fontNormal = new Font(baseFont, 12, Font.NORMAL);
                Font fontPequena = new Font(baseFont, 10, Font.NORMAL);

                // Encabezado
                Paragraph titulo = new Paragraph("PRESUPUESTO DE HONORARIOS PROFESIONALES", fontTitulo);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingAfter = 20f;
                documento.Add(titulo);

                // Información del escribano
                Paragraph escribano = new Paragraph($"Escribano: {datosFactura.EscribanoNombreValue}", fontSubtitulo);
                escribano.SpacingAfter = 5f;
                documento.Add(escribano);

                if (!string.IsNullOrEmpty(datosFactura.EscribanoDireccionValue))
                {
                    Paragraph direccion = new Paragraph($"Dirección: {datosFactura.EscribanoDireccionValue}", fontNormal);
                    direccion.SpacingAfter = 5f;
                    documento.Add(direccion);
                }

                if (!string.IsNullOrEmpty(datosFactura.EscribanoLocalidadValue))
                {
                    Paragraph localidad = new Paragraph($"Localidad: {datosFactura.EscribanoLocalidadValue}", fontNormal);
                    localidad.SpacingAfter = 5f;
                    documento.Add(localidad);
                }

                if (!string.IsNullOrEmpty(datosFactura.EscribanoMatriculaValue))
                {
                    Paragraph matricula = new Paragraph($"Matrícula: {datosFactura.EscribanoMatriculaValue}", fontNormal);
                    matricula.SpacingAfter = 5f;
                    documento.Add(matricula);
                }

                // Fecha
                Paragraph fecha = new Paragraph($"Fecha: {datosFactura.FechaFacturaValue:dd/MM/yyyy}", fontNormal);
                fecha.Alignment = Element.ALIGN_RIGHT;
                fecha.SpacingAfter = 20f;
                documento.Add(fecha);

                // Datos del cliente y operación
                PdfPTable tablaCliente = new PdfPTable(2);
                tablaCliente.WidthPercentage = 100;
                tablaCliente.SetWidths(new float[] { 30f, 70f });

                tablaCliente.AddCell(new PdfPCell(new Phrase("Cliente:", fontSubtitulo)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                tablaCliente.AddCell(new PdfPCell(new Phrase(cliente.NombreCompleto, fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });

                tablaCliente.AddCell(new PdfPCell(new Phrase("Tipo de Operación:", fontSubtitulo)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                tablaCliente.AddCell(new PdfPCell(new Phrase(datosFactura.TipoOperacionValue, fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });

                tablaCliente.AddCell(new PdfPCell(new Phrase("Expediente:", fontSubtitulo)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                tablaCliente.AddCell(new PdfPCell(new Phrase(numeroExpediente, fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });

                tablaCliente.AddCell(new PdfPCell(new Phrase("Importe Operación:", fontSubtitulo)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                tablaCliente.AddCell(new PdfPCell(new Phrase($"$ {datosFactura.ImporteOperacionValue:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });

                tablaCliente.SpacingAfter = 20f;
                documento.Add(tablaCliente);

                // Línea separadora
                documento.Add(new Paragraph("_".PadRight(80, '_'), fontPequena));
                documento.Add(new Paragraph(" ", fontNormal));

                // DETALLE COMPLETO DE CONCEPTOS E IMPUESTOS
                Paragraph tituloDetalle = new Paragraph("DETALLE COMPLETO DEL PRESUPUESTO", fontSubtitulo);
                tituloDetalle.SpacingAfter = 15f;
                documento.Add(tituloDetalle);

                // Tabla principal con todos los conceptos
                PdfPTable tablaCompleta = new PdfPTable(2);
                tablaCompleta.WidthPercentage = 100;
                tablaCompleta.SetWidths(new float[] { 70f, 30f });

                decimal totalAcumulado = 0;

                // ESCRITURAS Y DOCUMENTOS
                if (cobro.EscrituraCompraventa > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Escritura de Compraventa", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.EscrituraCompraventa:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.EscrituraCompraventa;
                }

                if (cobro.ConstitucionSociedad > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Constitución de Sociedad", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.ConstitucionSociedad:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.ConstitucionSociedad;
                }

                if (cobro.PoderGeneral > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Poder General", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.PoderGeneral:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.PoderGeneral;
                }

                if (cobro.CertificadoDominio > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Certificado de Dominio", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.CertificadoDominio:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.CertificadoDominio;
                }

                if (cobro.Testamento > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Testamento", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.Testamento:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.Testamento;
                }

                if (cobro.Donacion > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Donación", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.Donacion:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.Donacion;
                }

                if (cobro.Hipoteca > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Hipoteca", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.Hipoteca:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.Hipoteca;
                }

                if (cobro.CancelacionHipoteca > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Cancelación de Hipoteca", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.CancelacionHipoteca:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.CancelacionHipoteca;
                }

                if (cobro.Usufructo > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Usufructo", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.Usufructo:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.Usufructo;
                }

                if (cobro.Servidumbre > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase("Servidumbre", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.Servidumbre:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.Servidumbre;
                }

                if (cobro.OtraGestionImporte > 0)
                {
                    string nombreGestion = string.IsNullOrWhiteSpace(cobro.OtraGestionNombre) ? "Otra Gestión" : cobro.OtraGestionNombre;
                    tablaCompleta.AddCell(new PdfPCell(new Phrase(nombreGestion, fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.OtraGestionImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.OtraGestionImporte;
                }

                // IMPUESTOS Y TASAS
                if (cobro.ImpGananciasImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Impuesto a las Ganancias ({cobro.ImpGananciasPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.ImpGananciasImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.ImpGananciasImporte;
                }

                if (cobro.ITIImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"ITI - Impuesto a la Transferencia Inmobiliaria ({cobro.ITIPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.ITIImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.ITIImporte;
                }

                if (cobro.ImpSellosImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Impuesto de Sellos ({cobro.ImpSellosPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.ImpSellosImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.ImpSellosImporte;
                }

                if (cobro.TasaGeneralImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Tasa General ({cobro.TasaGeneralPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.TasaGeneralImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.TasaGeneralImporte;
                }

                if (cobro.SelloMatrizImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Sello Matriz ({cobro.SelloMatrizPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.SelloMatrizImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.SelloMatrizImporte;
                }

                // CERTIFICADOS Y DOCUMENTOS
                if (cobro.CertificadosImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Certificados y Documentos ({cobro.CertificadosPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.CertificadosImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.CertificadosImporte;
                }

                if (cobro.DiligenciamientoImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Diligenciamiento ({cobro.DiligenciamientoPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.DiligenciamientoImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.DiligenciamientoImporte;
                }

                // HONORARIOS PROFESIONALES
                if (cobro.HonorariosImporte > 0)
                {
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"Honorarios Profesionales ({cobro.HonorariosPorc}%)", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {cobro.HonorariosImporte:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += cobro.HonorariosImporte;
                }

                // CONCEPTOS DEL MODAL (Foja Elaborada, Aporte Notarial, Honorarios Comprobante)
                if (!string.IsNullOrEmpty(datosFactura.ConceptoFojaElaboradaValue) && datosFactura.PrecioFojaElaboradaValue > 0)
                {
                    decimal subtotal = datosFactura.PrecioFojaElaboradaValue * datosFactura.CantidadFojaElaboradaValue;
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"{datosFactura.ConceptoFojaElaboradaValue} (x{datosFactura.CantidadFojaElaboradaValue})", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {subtotal:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += subtotal;
                }

                if (!string.IsNullOrEmpty(datosFactura.ConceptoAporteNotarialValue) && datosFactura.PrecioAporteNotarialValue > 0)
                {
                    decimal subtotal = datosFactura.PrecioAporteNotarialValue * datosFactura.CantidadAporteNotarialValue;
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"{datosFactura.ConceptoAporteNotarialValue} (x{datosFactura.CantidadAporteNotarialValue})", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {subtotal:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += subtotal;
                }

                if (!string.IsNullOrEmpty(datosFactura.ConceptoHonorariosValue) && datosFactura.PrecioHonorariosValue > 0)
                {
                    decimal subtotal = datosFactura.PrecioHonorariosValue * datosFactura.CantidadHonorariosValue;
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"{datosFactura.ConceptoHonorariosValue} (x{datosFactura.CantidadHonorariosValue})", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f });
                    tablaCompleta.AddCell(new PdfPCell(new Phrase($"$ {subtotal:N2}", fontNormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_RIGHT });
                    totalAcumulado += subtotal;
                }

                tablaCompleta.SpacingAfter = 20f;
                documento.Add(tablaCompleta);

                // Total
                PdfPTable tablaTotal = new PdfPTable(2);
                tablaTotal.WidthPercentage = 60;
                tablaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                tablaTotal.SetWidths(new float[] { 70f, 30f });

                PdfPCell celdaTextoTotal = new PdfPCell(new Phrase("TOTAL GENERAL:", fontSubtitulo));
                celdaTextoTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaTextoTotal.Border = Rectangle.TOP_BORDER;
                celdaTextoTotal.Padding = 10f;
                tablaTotal.AddCell(celdaTextoTotal);

                PdfPCell celdaMontoTotal = new PdfPCell(new Phrase($"$ {totalAcumulado:N2}", fontSubtitulo));
                celdaMontoTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaMontoTotal.Border = Rectangle.TOP_BORDER;
                celdaMontoTotal.Padding = 10f;
                tablaTotal.AddCell(celdaMontoTotal);

                tablaTotal.SpacingAfter = 30f;
                documento.Add(tablaTotal);

                // Observaciones
                if (!string.IsNullOrEmpty(datosFactura.ObservacionesFacturaValue))
                {
                    Paragraph observaciones = new Paragraph("OBSERVACIONES:", fontSubtitulo);
                    observaciones.SpacingAfter = 10f;
                    documento.Add(observaciones);

                    Paragraph textoObservaciones = new Paragraph(datosFactura.ObservacionesFacturaValue, fontNormal);
                    textoObservaciones.SpacingAfter = 20f;
                    documento.Add(textoObservaciones);
                }

                // Términos y condiciones
                Paragraph terminos = new Paragraph("TÉRMINOS Y CONDICIONES:", fontSubtitulo);
                terminos.SpacingAfter = 10f;
                documento.Add(terminos);

                Paragraph textoTerminos = new Paragraph(
                    "• Los honorarios están sujetos a la complejidad del trámite.\n" +
                    "• Los gastos adicionales (sellos, certificados, etc.) se cobran por separado.\n" +
                    "• Este presupuesto tiene validez de 30 días.\n" +
                    "• Para proceder con el trámite se requiere el 50% de anticipo.",
                    fontPequena);
                textoTerminos.SpacingAfter = 30f;
                documento.Add(textoTerminos);

                // Pie de página
                Paragraph pie = new Paragraph($"Presupuesto generado el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}", fontPequena);
                pie.Alignment = Element.ALIGN_CENTER;
                documento.Add(pie);

                documento.Close();

                // Abrir el archivo generado
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = rutaCompleta,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF completo: {ex.Message}\n\nDetalles: {ex.StackTrace}");
            }
        }
    }
}
