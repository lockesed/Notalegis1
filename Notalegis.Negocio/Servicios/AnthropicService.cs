using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Notalegis.Datos.Modelos;
using UglyToad.PdfPig;

namespace Notalegis.Negocio.Servicios
{
    public class AnthropicService
    {
        private const string API_URL = "https://api.anthropic.com/v1/messages";
        private const string ANTHROPIC_VERSION = "2023-06-01";
        private const string MODELO_DEFAULT = "claude-haiku-4-5-20251001";
        private const int MAX_TOKENS_DEFAULT = 8192;

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        private readonly ConfiguracionService _conf = new ConfiguracionService();

        public async Task<string> GenerarEscritura(
            string tipoEscritura,
            Cliente? cliente,
            string instruccionesAdicionales = "",
            List<string>? rutasArchivosReferencia = null,
            string nacionalidad = "",
            string estadoCivil = "",
            string nombreConyuge = "",
            DateTime? fechaNacimiento = null,
            string cuilCuit = "")
        {
            string apiKey = _conf.ObtenerValorString(ClavesConfiguracion.IAClaudeApiKey, "");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("No se configuró la API Key de Claude.\nAndá a Configuración → Inteligencia Artificial e ingresá la clave.");

            string modelo = _conf.ObtenerValorString(ClavesConfiguracion.IAModelo, MODELO_DEFAULT);
            if (string.IsNullOrWhiteSpace(modelo)) modelo = MODELO_DEFAULT;

            int maxTokens = MAX_TOKENS_DEFAULT;
            string maxTokensStr = _conf.ObtenerValorString(ClavesConfiguracion.IAMaxTokens, "");
            if (int.TryParse(maxTokensStr, out int maxTokensParsed) && maxTokensParsed >= 1024)
                maxTokens = maxTokensParsed;

            // Incorporar PDF de normativa vigente configurado globalmente
            var todasLasRutas = new List<string>(rutasArchivosReferencia ?? new List<string>());
            string rutaNormativa = _conf.ObtenerValorString(ClavesConfiguracion.IARutaNormativaPDF, "");
            if (!string.IsNullOrWhiteSpace(rutaNormativa)
                && File.Exists(rutaNormativa)
                && !todasLasRutas.Contains(rutaNormativa))
            {
                todasLasRutas.Insert(0, rutaNormativa);
            }

            string systemPrompt = ConstruirSystemPrompt();
            string userPrompt = ConstruirPrompt(
                tipoEscritura, cliente, instruccionesAdicionales,
                _conf.ObtenerValorString(ClavesConfiguracion.IAPromptBase, ""),
                todasLasRutas,
                nacionalidad, estadoCivil, nombreConyuge, fechaNacimiento, cuilCuit);

            using var request = new HttpRequestMessage(HttpMethod.Post, API_URL);
            request.Headers.Add("x-api-key", apiKey);
            request.Headers.Add("anthropic-version", ANTHROPIC_VERSION);

            var body = new
            {
                model = modelo,
                max_tokens = maxTokens,
                system = systemPrompt,
                messages = new[] { new { role = "user", content = userPrompt } }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            string raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    using var errDoc = JsonDocument.Parse(raw);
                    string msg = errDoc.RootElement
                        .GetProperty("error").GetProperty("message").GetString() ?? raw;
                    throw new Exception($"Error de la API de Claude: {msg}");
                }
                catch (Exception ex) when (!ex.Message.StartsWith("Error de la API"))
                {
                    throw new Exception($"Error HTTP {(int)response.StatusCode}. Verificá que la API Key sea correcta.");
                }
            }

            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "";
        }

        private static string ConstruirSystemPrompt()
        {
            return @"Sos un asistente especializado en redacción de documentos notariales argentinos, perteneciente al sistema de gestión del Registro Notarial Número UNO de la Ciudad Ejemplo, Provincia de Neuquén, a cargo de la Escribana María Ejemplo.

TU ÚNICA FUNCIÓN es generar documentos notariales completos basándote EXCLUSIVAMENTE en la plantilla que se te proporciona. Está PROHIBIDO inventar estructuras, cláusulas, títulos o secciones que no figuren en la plantilla.

═══════════════════════════════════════════
REGLAS ABSOLUTAS
═══════════════════════════════════════════

1. SEGUÍ LA PLANTILLA AL PIE DE LA LETRA
   - Reproducí el orden exacto de todas las secciones
   - Conservá los títulos en mayúsculas tal como aparecen
   - Conservá la numeración de cláusulas (PRIMERO, SEGUNDO, TERCERO, etc.)
   - Conservá las fórmulas jurídicas fijas sin modificarlas
   - No agregues ni elimines secciones

2. REEMPLAZÁ LOS DATOS DEL CLIENTE
   Con los datos que se te proveen, reemplazá los campos variables:
   - Nombre completo del/los compareciente/s
   - DNI / CUIT / CUIL
   - Domicilio
   - Estado civil (si se provee)
   - Si algún dato falta escribí exactamente: [DATO - COMPLETAR]
   - Si el domicilio está vacío escribí: [DOMICILIO - COMPLETAR]

3. CAMPOS CON BARRAS (/////////) EN LA PLANTILLA
   Las barras /////// indican campos que la escribana completa a mano después.
   Dejálos exactamente igual: /////////.
   No los reemplaces con datos inventados.

4. FECHA Y LUGAR
   - Ciudad: Ciudad Ejemplo, Provincia de Neuquén
   - Si no se provee fecha escribí: /////////// días del mes de /////// del año dos mil veinticinco

5. FÓRMULAS FIJAS QUE NUNCA DEBÉS MODIFICAR
   Las siguientes frases son fórmulas legales que deben aparecer exactamente como en la plantilla:
   - ""doy fe""
   - ""LEIDA Y RATIFICADA, así la aceptan, otorgan y firman...""
   - ""ante mí, María Ejemplo, Escribana Pública Nacional""
   - ""conforme lo prescripto por el artículo 306 inciso...""
   - ""con carácter de declaración jurada""
   - Cualquier otra fórmula notarial que aparezca en el documento

6. FORMATO DEL TEXTO
   - Respetá las palabras en MAYÚSCULAS tal como aparecen en la plantilla
   - Respetá las palabras en negrita tal como aparecen
   - Respetá la separación entre párrafos
   - No uses viñetas ni listas donde la plantilla usa párrafos corridos

7. TIPO DE DOCUMENTO
   El tipo de documento lo determina exclusivamente la plantilla.
   Si la plantilla es una escritura de venta, generás una escritura de venta.
   Si es un poder, generás un poder. Nunca cambiés el tipo.

8. LO QUE NUNCA PODÉS HACER
   - Inventar datos del cliente que no se te proporcionaron
   - Cambiar el tipo o la naturaleza jurídica del acto
   - Agregar cláusulas que no estén en la plantilla
   - Omitir secciones de la plantilla
   - Cambiar el estilo de redacción notarial
   - Interpretar o ""mejorar"" las cláusulas jurídicas

═══════════════════════════════════════════
MÁRGENES Y CONFIGURACIÓN DE PÁGINA
═══════════════════════════════════════════

El documento debe respetar la siguiente configuración obligatoria según normativa notarial de la Provincia de Neuquén:

- Márgenes: Superior 3 cm | Inferior 3 cm | Izquierdo 3 cm | Derecho 2 cm
- Fuente: Times New Roman, cuerpo 12
- Interlineado: 1,5 líneas
- Tamaño de página: A4
- Todos los párrafos justificados

Estos márgenes son de cumplimiento legal obligatorio y no pueden modificarse.

═══════════════════════════════════════════
FORMATO DE SALIDA
═══════════════════════════════════════════

Devolvé únicamente el texto del documento notarial completo, listo para ser volcado a Word.
Sin explicaciones previas.
Sin comentarios al final.
Sin aclaraciones del tipo ""aquí está el documento"".
Solo el documento, desde el encabezado hasta la firma inclusive.";
        }

        private string ConstruirPrompt(string tipo, Cliente? cliente, string instrucciones, string promptBase, List<string> rutasArchivos,
            string nacionalidad = "", string estadoCivil = "", string nombreConyuge = "", DateTime? fechaNacimiento = null, string cuilCuit = "")
        {
            string nombreEscribania = _conf.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "");
            string ciudad = _conf.ObtenerValorString(ClavesConfiguracion.EscribaniaCiudad, "[CIUDAD]");
            string provincia = _conf.ObtenerValorString(ClavesConfiguracion.EscribaniaProvincia, "[PROVINCIA]");
            string numeroRegistro = _conf.ObtenerValorString(ClavesConfiguracion.EscribaniaNumeroRegistro, "");
            string escribano = _conf.ObtenerValorString(ClavesConfiguracion.EscribaniaEscribano, "");

            string infoRegistro = numeroRegistro.Length > 0
                ? $"Registro Notarial N° {numeroRegistro}"
                : "[NÚMERO DE REGISTRO NOTARIAL]";

            string infoEscribano = escribano.Length > 0
                ? $"Titular: {escribano}"
                : "[NOMBRE DEL ESCRIBANO/A TITULAR]";

            string seccionEscribania = string.IsNullOrWhiteSpace(promptBase)
                ? ""
                : $"\nDIRECTRICES ESPECÍFICAS DE LA ESCRIBANÍA:\n{promptBase}\n";

            string seccionReferencias = ExtraerContenidoArchivos(rutasArchivos);

            string seccionInstrucciones = string.IsNullOrWhiteSpace(instrucciones)
                ? ""
                : $"\nINSTRUCCIONES ADICIONALES PARA ESTE ACTO:\n{instrucciones}\n";

            var datosOtorgante = new StringBuilder();
            if (cliente != null)
            {
                datosOtorgante.AppendLine($"• Nombre completo: {cliente.Nombre} {cliente.Apellido}");
                datosOtorgante.AppendLine($"• DNI: {cliente.DNI}");
                if (!string.IsNullOrWhiteSpace(cuilCuit))
                    datosOtorgante.AppendLine($"• CUIL/CUIT: {cuilCuit}");
                datosOtorgante.AppendLine($"• Domicilio: {(string.IsNullOrWhiteSpace(cliente.Direccion) ? "[DOMICILIO - COMPLETAR]" : cliente.Direccion)}");
            }
            else
            {
                // Todavía no se eligió el cliente (se elige recién al guardar) — se deja como texto a completar.
                datosOtorgante.AppendLine("• Nombre completo: [NOMBRE Y APELLIDO DEL OTORGANTE - COMPLETAR]");
                datosOtorgante.AppendLine("• DNI: [DNI - COMPLETAR]");
                if (!string.IsNullOrWhiteSpace(cuilCuit))
                    datosOtorgante.AppendLine($"• CUIL/CUIT: {cuilCuit}");
                datosOtorgante.AppendLine("• Domicilio: [DOMICILIO - COMPLETAR]");
            }
            datosOtorgante.AppendLine($"• Nacionalidad: {(string.IsNullOrWhiteSpace(nacionalidad) ? "[NACIONALIDAD - COMPLETAR]" : nacionalidad)}");
            datosOtorgante.AppendLine($"• Estado civil: {(string.IsNullOrWhiteSpace(estadoCivil) ? "[ESTADO CIVIL - COMPLETAR]" : estadoCivil)}");
            if (!string.IsNullOrWhiteSpace(nombreConyuge))
                datosOtorgante.AppendLine($"• Nombre del cónyuge: {nombreConyuge}");
            datosOtorgante.Append($"• Fecha de nacimiento: {(fechaNacimiento.HasValue ? fechaNacimiento.Value.ToString("dd/MM/yyyy") : "[FECHA DE NACIMIENTO - COMPLETAR]")}");
            string bloqueDatosOtorgante = datosOtorgante.ToString();

            string tipoMostrado = string.IsNullOrWhiteSpace(tipo) ? "[TIPO DE ESCRITURA - COMPLETAR]" : tipo.ToUpper();

            return $@"GENERÁ UN DOCUMENTO NOTARIAL DE TIPO: {tipoMostrado}

La plantilla y/o normativa de referencia para este acto figura más abajo, en ""DOCUMENTOS DE REFERENCIA Y NORMATIVA ADJUNTA"". Usala como base EXCLUSIVA de la estructura, el orden de las secciones, la numeración de cláusulas y las fórmulas fijas, conforme las reglas del system prompt. No inventes secciones que no estén en la plantilla.
{seccionEscribania}{seccionReferencias}{seccionInstrucciones}
DATOS DEL OTORGANTE PRINCIPAL PARA REEMPLAZAR EN LA PLANTILLA:
{bloqueDatosOtorgante}

DATOS DE LA ESCRIBANÍA ACTUANTE (para los campos del encabezado/cierre que ya estén en la plantilla):
• Nombre: {(string.IsNullOrWhiteSpace(nombreEscribania) ? "[NOMBRE DE LA ESCRIBANÍA]" : nombreEscribania)}
• {infoRegistro}
• Ciudad de otorgamiento: {ciudad}, {provincia}
• {infoEscribano}

Reemplazá los campos variables de la plantilla con estos datos. Los campos sin dato disponible dejálos como [DATO - COMPLETAR] según corresponda. Las barras /////// de la plantilla se dejan exactamente igual, sin completar.";
        }

        private static string ExtraerContenidoArchivos(List<string> rutasArchivos)
        {
            if (rutasArchivos == null || rutasArchivos.Count == 0)
                return "";

            var sb = new StringBuilder("\nDOCUMENTOS DE REFERENCIA Y NORMATIVA ADJUNTA:\n");
            foreach (var ruta in rutasArchivos)
            {
                if (!File.Exists(ruta)) continue;
                try
                {
                    string ext = Path.GetExtension(ruta).ToLowerInvariant();
                    string contenido = ext switch
                    {
                        ".pdf" => ExtraerTextoDePDF(ruta),
                        ".docx" => ExtraerTextoDeDOCX(ruta),
                        _ => File.ReadAllText(ruta, Encoding.UTF8)
                    };
                    if (!string.IsNullOrWhiteSpace(contenido))
                        sb.AppendLine($"\n--- {Path.GetFileName(ruta)} ---\n{contenido}");
                }
                catch { }
            }
            return sb.ToString();
        }

        private static string ExtraerTextoDePDF(string ruta)
        {
            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(ruta);
            foreach (var page in doc.GetPages())
            {
                string texto = string.Join(" ", page.GetWords().Select(w => w.Text));
                if (!string.IsNullOrWhiteSpace(texto))
                    sb.AppendLine(texto);
            }
            return sb.ToString();
        }

        private static string ExtraerTextoDeDOCX(string ruta)
        {
            using var doc = WordprocessingDocument.Open(ruta, false);
            var mainPart = doc.MainDocumentPart;
            if (mainPart == null) return "";

            var sb = new StringBuilder();
            var numPart = mainPart.NumberingDefinitionsPart;

            foreach (var hp in mainPart.HeaderParts)
            {
                string txt = ProcesarBloques(hp.Header.ChildElements, numPart);
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    sb.AppendLine("[ENCABEZADO]");
                    sb.AppendLine(txt.Trim());
                    sb.AppendLine();
                }
            }

            if (mainPart.Document?.Body != null)
                sb.Append(ProcesarBloques(mainPart.Document.Body.ChildElements, numPart));

            var cajas = mainPart.Document?.Body?.Descendants<TextBoxContent>().ToList();
            if (cajas?.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("[CUADROS DE TEXTO]");
                foreach (var caja in cajas)
                {
                    string txt = ProcesarBloques(caja.ChildElements, numPart, skipTextBoxes: false);
                    if (!string.IsNullOrWhiteSpace(txt))
                        sb.AppendLine(txt.Trim());
                }
            }

            foreach (var fp in mainPart.FooterParts)
            {
                string txt = ProcesarBloques(fp.Footer.ChildElements, numPart);
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    sb.AppendLine("[PIE DE PÁGINA]");
                    sb.AppendLine(txt.Trim());
                }
            }

            return sb.ToString().Trim();
        }

        private static string ProcesarBloques(IEnumerable<OpenXmlElement> elementos, NumberingDefinitionsPart? numPart, bool skipTextBoxes = true)
        {
            var sb = new StringBuilder();
            var contadores = new Dictionary<(int numId, int ilvl), int>();
            foreach (var elem in elementos)
                ProcesarBloque(elem, sb, numPart, contadores, skipTextBoxes);
            return sb.ToString();
        }

        private static void ProcesarBloque(OpenXmlElement elem, StringBuilder sb, NumberingDefinitionsPart? numPart, Dictionary<(int, int), int> contadores, bool skipTextBoxes)
        {
            switch (elem)
            {
                case Paragraph para:
                    string linea = ProcesarParrafo(para, numPart, contadores, skipTextBoxes);
                    if (!string.IsNullOrWhiteSpace(linea))
                        sb.AppendLine(linea);
                    break;
                case Table tabla:
                    sb.Append(ProcesarTabla(tabla));
                    sb.AppendLine();
                    break;
                case SdtBlock sdt:
                    var content = sdt.GetFirstChild<SdtContentBlock>();
                    if (content != null)
                        foreach (var child in content.ChildElements)
                            ProcesarBloque(child, sb, numPart, contadores, skipTextBoxes);
                    break;
            }
        }

        private static string ProcesarParrafo(Paragraph para, NumberingDefinitionsPart? numPart, Dictionary<(int, int), int> contadores, bool skipTextBoxes)
        {
            string prefix = ObtenerPrefijoLista(para, numPart, contadores);
            var sb = new StringBuilder(prefix);

            IEnumerable<Run> runs = para.Descendants<Run>();
            if (skipTextBoxes)
                runs = runs.Where(r => !r.Ancestors<TextBoxContent>().Any());

            foreach (var run in runs)
            {
                string text = string.Join("", run.Elements<Text>().Select(t => t.Text));
                if (string.IsNullOrEmpty(text)) continue;

                var rPr = run.RunProperties;
                bool bold   = rPr?.Bold      != null && rPr.Bold.Val?.Value      != false;
                bool italic = rPr?.Italic    != null && rPr.Italic.Val?.Value    != false;
                bool caps   = (rPr?.Caps     != null && rPr.Caps.Val?.Value      != false)
                           || (rPr?.SmallCaps != null && rPr.SmallCaps.Val?.Value != false);

                if (caps) text = text.ToUpper();
                text = (bold, italic) switch
                {
                    (true, true)  => $"**_{text}_**",
                    (true, false) => $"**{text}**",
                    (false, true) => $"_{text}_",
                    _             => text
                };

                sb.Append(text);
            }

            return sb.ToString().Trim();
        }

        private static string ObtenerPrefijoLista(Paragraph para, NumberingDefinitionsPart? numPart, Dictionary<(int, int), int> contadores)
        {
            var numProps = para.ParagraphProperties?.NumberingProperties;
            if (numProps == null || numPart?.Numbering == null) return "";

            int numId = numProps.NumberingId?.Val?.Value ?? 0;
            if (numId == 0) return "";
            int ilvl = numProps.NumberingLevelReference?.Val?.Value ?? 0;
            string indent = new string(' ', ilvl * 2);

            var numInstance = numPart.Numbering.Elements<NumberingInstance>()
                .FirstOrDefault(n => n.NumberID?.Value == numId);

            Level? level = numInstance?.Elements<LevelOverride>()
                .FirstOrDefault(lo => lo.LevelIndex?.Value == ilvl)?.Level;

            if (level == null)
            {
                int absId = numInstance?.AbstractNumId?.Val?.Value ?? -1;
                level = numPart.Numbering.Elements<AbstractNum>()
                    .FirstOrDefault(a => a.AbstractNumberId?.Value == absId)
                    ?.Elements<Level>()
                    .FirstOrDefault(l => l.LevelIndex?.Value == ilvl);
            }

            var fmt = level?.NumberingFormat?.Val?.Value ?? NumberFormatValues.Bullet;

            if (fmt == NumberFormatValues.Bullet)
                return indent + "• ";

            var key = (numId, ilvl);
            contadores.TryGetValue(key, out int count);
            contadores[key] = ++count;

            foreach (var k in contadores.Keys.Where(k => k.Item1 == numId && k.Item2 > ilvl).ToList())
                contadores[k] = 0;

            if (fmt == NumberFormatValues.LowerLetter) return indent + $"{(char)('a' + (count - 1) % 26)}. ";
            if (fmt == NumberFormatValues.UpperLetter) return indent + $"{(char)('A' + (count - 1) % 26)}. ";
            if (fmt == NumberFormatValues.LowerRoman)  return indent + $"{ToRoman(count).ToLower()}. ";
            if (fmt == NumberFormatValues.UpperRoman)  return indent + $"{ToRoman(count)}. ";
            return indent + $"{count}. ";
        }

        private static string ProcesarTabla(Table tabla)
        {
            var sb = new StringBuilder();
            bool primeraFila = true;

            foreach (var row in tabla.Elements<TableRow>())
            {
                var celdas = row.Elements<TableCell>().Select(celda =>
                {
                    string contenido = string.Join(" ",
                        celda.Descendants<Paragraph>()
                            .Where(p => !p.Ancestors<TextBoxContent>().Any())
                            .Select(p => string.Join("",
                                p.Descendants<Run>()
                                    .Where(r => !r.Ancestors<TextBoxContent>().Any())
                                    .SelectMany(r => r.Elements<Text>())
                                    .Select(t => t.Text)))
                            .Where(t => !string.IsNullOrWhiteSpace(t)));
                    return contenido.Replace("|", "\\|").Trim();
                }).ToList();

                if (celdas.Any(c => !string.IsNullOrWhiteSpace(c)))
                {
                    sb.AppendLine("| " + string.Join(" | ", celdas) + " |");
                    if (primeraFila)
                    {
                        sb.AppendLine("|" + string.Join("|", celdas.Select(c => new string('-', Math.Max(c.Length + 2, 3)))) + "|");
                        primeraFila = false;
                    }
                }
            }

            return sb.ToString();
        }

        private static string ToRoman(int n)
        {
            if (n <= 0) return n.ToString();
            (int val, string sym)[] map =
            {
                (1000,"M"),(900,"CM"),(500,"D"),(400,"CD"),
                (100,"C"),(90,"XC"),(50,"L"),(40,"XL"),
                (10,"X"),(9,"IX"),(5,"V"),(4,"IV"),(1,"I")
            };
            var sb = new StringBuilder();
            foreach (var (val, sym) in map)
                while (n >= val) { sb.Append(sym); n -= val; }
            return sb.ToString();
        }

        /// <summary>
        /// Aplica configuración de página notarial estándar a un documento Word ya abierto.
        /// Llamar justo antes de cerrar el WordprocessingDocument.
        /// </summary>
        public static void AplicarConfiguracionPaginaNotarial(WordprocessingDocument doc)
        {
            var mainPart = doc.MainDocumentPart!;
            var body = mainPart.Document.Body!;

            // Tamaño A4 y márgenes notariales (twips)
            var sectPr = body.Elements<SectionProperties>().LastOrDefault();
            if (sectPr == null)
            {
                sectPr = new SectionProperties();
                body.Append(sectPr);
            }
            sectPr.RemoveAllChildren<PageSize>();
            sectPr.RemoveAllChildren<PageMargin>();
            sectPr.AppendChild(new PageSize { Width = 11906U, Height = 16838U });
            sectPr.AppendChild(new PageMargin { Top = 1701, Bottom = 1701, Left = 1701U, Right = 1134U });

            // StyleDefinitionsPart
            if (mainPart.StyleDefinitionsPart == null)
                mainPart.AddNewPart<StyleDefinitionsPart>();
            var stylesPart = mainPart.StyleDefinitionsPart!;
            if (stylesPart.Styles == null)
                stylesPart.Styles = new Styles();
            var styles = stylesPart.Styles;

            // Fuente por defecto: Times New Roman 12pt (24 half-points)
            var docDefaults = styles.GetFirstChild<DocDefaults>() ?? new DocDefaults();
            if (docDefaults.Parent == null)
                styles.InsertAt(docDefaults, 0);

            var rPrDefault = docDefaults.GetFirstChild<RunPropertiesDefault>() ?? new RunPropertiesDefault();
            if (rPrDefault.Parent == null)
                docDefaults.AppendChild(rPrDefault);

            var rPr = rPrDefault.GetFirstChild<RunProperties>() ?? new RunProperties();
            if (rPr.Parent == null)
                rPrDefault.AppendChild(rPr);

            rPr.RemoveAllChildren<RunFonts>();
            rPr.RemoveAllChildren<FontSize>();
            rPr.RemoveAllChildren<FontSizeComplexScript>();
            rPr.AppendChild(new RunFonts { Ascii = "Times New Roman", HighAnsi = "Times New Roman" });
            rPr.AppendChild(new FontSize { Val = "24" });
            rPr.AppendChild(new FontSizeComplexScript { Val = "24" });

            // Estilo Normal: interlineado 1.5 (360/240) y justificado
            var estiloNormal = styles.Elements<Style>()
                .FirstOrDefault(s => s.StyleId?.Value == "Normal");
            if (estiloNormal == null)
            {
                estiloNormal = new Style { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true };
                estiloNormal.AppendChild(new StyleName { Val = "Normal" });
                styles.AppendChild(estiloNormal);
            }

            var pPr = estiloNormal.GetFirstChild<StyleParagraphProperties>() ?? new StyleParagraphProperties();
            if (pPr.Parent == null)
                estiloNormal.AppendChild(pPr);

            pPr.RemoveAllChildren<SpacingBetweenLines>();
            pPr.RemoveAllChildren<Justification>();
            pPr.AppendChild(new SpacingBetweenLines { Line = "360", LineRule = LineSpacingRuleValues.Auto });
            pPr.AppendChild(new Justification { Val = JustificationValues.Both });

            stylesPart.Styles.Save();
            mainPart.Document.Save();
        }
    }
}
