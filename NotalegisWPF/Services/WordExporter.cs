using System;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Services
{
    public static class WordExporter
    {
        private const uint AnchoA4      = 11906;
        private const uint AltoA4       = 16838;
        private const int  MargenSuperior   = 1701;
        private const int  MargenInferior   = 1701;
        private const uint MargenIzquierdo  = 1701;
        private const uint MargenDerecho    = 1134;   // 2 cm
        private const string TamanioFuente  = "24";   // 12pt en half-points

        // Reconoce **negrita** y _cursiva_ en una sola pasada
        private static readonly Regex RegexInline = new Regex(
            @"\*\*(.+?)\*\*|_(.+?)_",
            RegexOptions.Compiled | RegexOptions.Singleline);

        private static readonly string[] EncabezadosSeccion = {
            "PRIMERO", "SEGUNDO", "TERCERO", "CUARTO", "QUINTO",
            "SEXTO", "SÉPTIMO", "OCTAVO", "NOVENO", "DÉCIMO",
            "UNDÉCIMO", "DUODÉCIMO",
            "ASENTIMIENTO CONYUGAL",
            "LA ESCRIBANA AUTORIZANTE",
            "EL ESCRIBANO AUTORIZANTE",
            "LEIDA Y RATIFICADA",
            "LEÍDA Y RATIFICADA",
            "HACE CONSTAR"
        };

        public static void Exportar(string rutaDestino, string contenido)
        {
            using var docx = WordprocessingDocument.Create(rutaDestino, WordprocessingDocumentType.Document);

            var mainPart = docx.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            AgregarEstilosPredeterminados(mainPart);

            // La primera línea no vacía es el encabezado del documento (negrita+subrayado)
            bool primeraLineaEmitida = false;
            var lineas = contenido.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var linea in lineas)
            {
                bool esEncabezado = !primeraLineaEmitida && !string.IsNullOrWhiteSpace(linea);
                if (esEncabezado) primeraLineaEmitida = true;
                body.AppendChild(CrearParrafo(linea, esEncabezado));
            }

            // SectionProperties al final del body (estándar OpenXml)
            body.AppendChild(new SectionProperties(
                new PageSize { Width = AnchoA4, Height = AltoA4 },
                new PageMargin
                {
                    Top    = MargenSuperior,
                    Bottom = MargenInferior,
                    Left   = MargenIzquierdo,
                    Right  = MargenDerecho
                }
            ));

            mainPart.Document.Save();

            // AplicarConfiguracionPaginaNotarial siempre al final, con todo el contenido ya escrito
            AnthropicService.AplicarConfiguracionPaginaNotarial(docx);
        }

        private static void AgregarEstilosPredeterminados(MainDocumentPart mainPart)
        {
            var stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles(
                new DocDefaults(
                    new RunPropertiesDefault(
                        new RunPropertiesBaseStyle(
                            new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" },
                            new FontSize { Val = TamanioFuente },
                            new FontSizeComplexScript { Val = TamanioFuente }
                        )
                    ),
                    new ParagraphPropertiesDefault(
                        new ParagraphPropertiesBaseStyle(
                            new Justification { Val = JustificationValues.Both },
                            new SpacingBetweenLines
                            {
                                Line     = "360",
                                LineRule = LineSpacingRuleValues.Auto,
                                Before   = "0",
                                After    = "0"
                            }
                        )
                    )
                )
            );
            stylesPart.Styles.Save();
        }

        private static Paragraph CrearParrafo(string lineaOriginal, bool esEncabezadoDocumento = false)
        {
            var para = new Paragraph();
            para.AppendChild(PropiedadesParrafo());

            string texto = lineaOriginal.Trim();
            if (string.IsNullOrEmpty(texto))
                return para;

            // Primera línea del documento: encabezado notarial (ej. "FOLIO 91.- ESCRITURA...")
            if (esEncabezadoDocumento)
            {
                para.AppendChild(CrearRun(texto, negrita: true, subrayado: true));
                return para;
            }

            // Línea completamente envuelta en **: sección entera negrita+subrayado
            if (texto.StartsWith("**") && texto.EndsWith("**") && texto.Length > 4)
            {
                string contenido = texto.Substring(2, texto.Length - 4).Trim();
                para.AppendChild(CrearRun(contenido, negrita: true, subrayado: true));
                return para;
            }

            // Encabezado de sección por palabra clave (PRIMERO:, SEGUNDO:, etc.)
            string kw = DetectarEncabezado(texto);
            if (kw != null)
            {
                int posColon = texto.IndexOf(':', kw.Length - 1);
                if (posColon > 0 && posColon < texto.Length - 1)
                {
                    para.AppendChild(CrearRun(texto.Substring(0, posColon + 1), negrita: true, subrayado: true));
                    AgregarRunsMarkdown(para, texto.Substring(posColon + 1));
                }
                else
                {
                    para.AppendChild(CrearRun(texto, negrita: true, subrayado: true));
                }
                return para;
            }

            // Párrafo de cuerpo: procesar **negrita** y _cursiva_
            AgregarRunsMarkdown(para, texto);
            return para;
        }

        private static void AgregarRunsMarkdown(Paragraph para, string texto)
        {
            int pos = 0;
            foreach (Match m in RegexInline.Matches(texto))
            {
                if (m.Index > pos)
                    AppendRun(para, texto.Substring(pos, m.Index - pos));

                if (m.Groups[1].Success)                          // **negrita**
                    AppendRun(para, m.Groups[1].Value, negrita: true);
                else                                              // _cursiva_
                    AppendRun(para, m.Groups[2].Value, cursiva: true);

                pos = m.Index + m.Length;
            }
            if (pos < texto.Length)
                AppendRun(para, texto.Substring(pos));
        }

        private static void AppendRun(Paragraph para, string texto, bool negrita = false, bool cursiva = false)
        {
            if (string.IsNullOrEmpty(texto)) return;
            para.AppendChild(CrearRun(texto, negrita, subrayado: false, cursiva: cursiva));
        }

        private static Run CrearRun(string texto, bool negrita, bool subrayado, bool cursiva = false)
        {
            var rPr = new RunProperties();
            rPr.AppendChild(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" });
            rPr.AppendChild(new FontSize { Val = TamanioFuente });
            rPr.AppendChild(new FontSizeComplexScript { Val = TamanioFuente });
            if (negrita)   rPr.AppendChild(new Bold());
            if (subrayado) rPr.AppendChild(new Underline { Val = UnderlineValues.Single });
            if (cursiva)   rPr.AppendChild(new Italic());

            var run = new Run();
            run.AppendChild(rPr);
            run.AppendChild(new Text(texto) { Space = SpaceProcessingModeValues.Preserve });
            return run;
        }

        private static ParagraphProperties PropiedadesParrafo()
        {
            return new ParagraphProperties(
                new Justification { Val = JustificationValues.Both },
                new SpacingBetweenLines
                {
                    Line     = "360",
                    LineRule = LineSpacingRuleValues.Auto,
                    Before   = "0",
                    After    = "0"
                }
            );
        }

        private static string DetectarEncabezado(string texto)
        {
            foreach (var kw in EncabezadosSeccion)
                if (texto.StartsWith(kw, StringComparison.OrdinalIgnoreCase))
                    return kw;
            return null;
        }
    }
}
