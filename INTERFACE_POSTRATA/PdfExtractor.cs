using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

// NOTE: This class prefers to use UglyToad.PdfPig for reliable PDF text extraction.
// Please install the NuGet package UglyToad.PdfPig if you want best results:
// PM> Install-Package UglyToad.PdfPig
// If the package is not available, a lightweight fallback extractor will be used.

namespace INTERFACE_POSTRATA
{
    public static class PdfExtractor
    {
        public class PsaExtractionResult
        {
            public string PsaTotal { get; set; }
            public string PsaLivre { get; set; }
            public string PsaDensidade { get; set; }
        }

        /// <summary>
        /// Extrai os valores de PSA (total, livre e densidade) de um PDF.
        /// Usa UglyToad.PdfPig quando disponível; caso contrário usa um fallback simples.
        /// Retorna null ou strings vazias para valores não encontrados.
        /// </summary>
        public static PsaExtractionResult ExtractPsaValues(string caminhoPdf)
        {
            if (string.IsNullOrEmpty(caminhoPdf) || !File.Exists(caminhoPdf)) return null;

            string fullText = null;

            // 1) Tentar usar UglyToad.PdfPig (recomendado)
            try
            {
                // Tentamos carregar e usar a biblioteca PdfPig se estiver instalada
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UglyToad.PdfPig");
                if (assembly == null)
                {
                    // Tentar carregar a partir do NuGet/arquivo
                    assembly = System.Reflection.Assembly.Load("UglyToad.PdfPig");
                }

                if (assembly != null)
                {
                    var pdfDocType = assembly.GetType("UglyToad.PdfPig.PdfDocument");
                    if (pdfDocType != null)
                    {
                        // Chamar PdfDocument.Open(path)
                        var openMethod = pdfDocType.GetMethod("Open", new[] { typeof(string) });
                        if (openMethod != null)
                        {
                            using (var doc = openMethod.Invoke(null, new object[] { caminhoPdf }) as IDisposable)
                            {
                                var docObj = doc;
                                var docType = docObj.GetType();
                                var getPages = docType.GetMethod("GetPages");
                                if (getPages != null)
                                {
                                    var pages = getPages.Invoke(docObj, null) as System.Collections.IEnumerable;
                                    var sb = new System.Text.StringBuilder();
                                    if (pages != null)
                                    {
                                        foreach (var page in pages)
                                        {
                                            var pageType = page.GetType();
                                            var textProp = pageType.GetProperty("Text");
                                            if (textProp != null)
                                            {
                                                var pageText = textProp.GetValue(page) as string;
                                                if (!string.IsNullOrEmpty(pageText)) sb.AppendLine(pageText);
                                            }
                                        }
                                    }
                                    fullText = sb.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Se falhar, continuamos para o fallback abaixo
                fullText = null;
            }

            // 2) Fallback simples: tentar extrair texto heurístico a partir dos bytes
            if (string.IsNullOrEmpty(fullText))
            {
                try
                {
                    var bytes = File.ReadAllBytes(caminhoPdf);
                    var sb = new System.Text.StringBuilder();
                    foreach (var b in bytes)
                    {
                        if (b >= 32 && b <= 126) sb.Append((char)b);
                        else sb.Append(' ');
                    }
                    fullText = sb.ToString();
                }
                catch
                {
                    fullText = null;
                }
            }

            if (string.IsNullOrEmpty(fullText)) return new PsaExtractionResult();

            // Função auxiliar para procurar múltiplos padrões e normalizar o número
            string FindValue(string[] patterns)
            {
                foreach (var p in patterns)
                {
                    var rx = new Regex(p, RegexOptions.IgnoreCase);
                    var m = rx.Match(fullText);
                    if (m.Success && m.Groups.Count > 1)
                    {
                        var val = m.Groups[1].Value.Trim();
                        val = val.Replace(',', '.');
                        return val;
                    }
                }
                return null;
            }

            // Padrões flexíveis que cobrem variações comuns
            var totalPatterns = new[] {
                @"PSA\s*(?:Total)?\s*[:\-\(]?\s*([0-9]+(?:[\.,][0-9]+)?)",
                @"Ant[ií]geno Prost[aá]tico Espec[ií]fico\s*(?:Total)?\s*[:\-]?\s*([0-9]+(?:[\.,][0-9]+)?)",
                @"PSA-?Total\s*[:\-]?\s*([0-9]+(?:[\.,][0-9]+)?)"
            };

            var livrePatterns = new[] {
                @"PSA\s*(?:Livre|Free)\s*[:\-\(]?\s*([0-9]+(?:[\.,][0-9]+)?)",
                @"Ant[ií]geno Prost[aá]tico Espec[ií]fico\s*(?:Livre)\s*[:\-]?\s*([0-9]+(?:[\.,][0-9]+)?)"
            };

            var densidadePatterns = new[] {
                @"Densidade\s*(?:PSA)?\s*[:\-]?\s*([0-9]+(?:[\.,][0-9]+)?)",
                @"PSA\s*Densidade\s*[:\-]?\s*([0-9]+(?:[\.,][0-9]+)?)",
                @"Densidade PSA\s*[:\-]?\s*([0-9]+(?:[\.,][0-9]+)?)"
            };

            var result = new PsaExtractionResult();
            result.PsaTotal = FindValue(totalPatterns) ?? string.Empty;
            result.PsaLivre = FindValue(livrePatterns) ?? string.Empty;
            result.PsaDensidade = FindValue(densidadePatterns) ?? string.Empty;

            return result;
        }
    }
}
