using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using INTERFACE_POSTRATA.Models;

namespace INTERFACE_POSTRATA.Services
{
    public static class HtmlLaudoService
    {
        public static void GenerateAndOpenHtml(Exame exame)
        {
            if (exame == null) throw new ArgumentNullException(nameof(exame));

            string basePath = AppContext.BaseDirectory ?? Environment.CurrentDirectory;
            string templatesDir = Path.Combine(basePath, "Templates");
            string templatePath = Path.Combine(templatesDir, "LaudoTemplate.html");
            string cssPath = Path.Combine(templatesDir, "style.css");

            string template;
            if (File.Exists(templatePath)) template = File.ReadAllText(templatePath, Encoding.UTF8);
            else
            {
                // fallback minimal template
                template = "<html><body><h1>Laudo</h1>{{RESULTADO}}</body></html>";
            }

            // Calcular Relação Livre/Total (L/T) em percentual, validar entrada e divisão por zero
            string psaTotalRaw = exame.PsaTotal ?? string.Empty;
            string psaLivreRaw = exame.PsaLivre ?? string.Empty;
            string psaDensRaw = exame.PsaDensidade ?? string.Empty;

            bool parsedTotal = TryParseDouble(psaTotalRaw, out double psaTotalVal);
            bool parsedLivre = TryParseDouble(psaLivreRaw, out double psaLivreVal);

            string ltDisplay;
            string ltInterpretation;

            if (!parsedTotal || !parsedLivre || psaTotalVal == 0)
            {
                ltDisplay = "N/D"; // Não disponível
                ltInterpretation = "Interpretação da Relação L/T não disponível devido a valores inválidos ou PSA Total igual a zero.";
            }
            else
            {
                double lt = (psaLivreVal / psaTotalVal) * 100.0;
                double ltRounded = Math.Round(lt, 2);
                // Formatar usando notação local pt-BR (vírgula)
                ltDisplay = ltRounded.ToString("F2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) + " %";

                // Determinar interpretação usando faixas fornecidas (comportamento apenas informativo)
                if (ltRounded >= 25.0)
                    ltInterpretation = "Baixa probabilidade de câncer de próstata.";
                else if (ltRounded >= 20.0)
                    ltInterpretation = "Risco discretamente aumentado.";
                else if (ltRounded >= 15.0)
                    ltInterpretation = "Risco moderado.";
                else if (ltRounded >= 10.0)
                    ltInterpretation = "Risco elevado.";
                else
                    ltInterpretation = "Risco muito elevado.";
            }

            var replacements = new Dictionary<string, string>()
            {
                ["{{PACIENTE}}"] = HtmlEncode(exame.PacienteNome ?? "-"),
                ["{{IDADE}}"] = HtmlEncode(exame.Idade ?? "-"),
                ["{{PSATOTAL}}"] = HtmlEncode(!string.IsNullOrWhiteSpace(psaTotalRaw) ? psaTotalRaw : "-"),
                ["{{PSALIVRE}}"] = HtmlEncode(!string.IsNullOrWhiteSpace(psaLivreRaw) ? psaLivreRaw : "-"),
                ["{{PSADENSIDADE}}"] = HtmlEncode(!string.IsNullOrWhiteSpace(psaDensRaw) ? psaDensRaw : "-"),
                ["{{LTRATIO}}"] = HtmlEncode(ltDisplay),
                ["{{LTINTERPRETACAO}}"] = HtmlEncode(ltInterpretation),
                ["{{RESULTADO}}"] = HtmlEncode(exame.Resultado ?? "-"),
                ["{{MEDICO}}"] = HtmlEncode(exame.Medico ?? "-"),
                ["{{CRM}}"] = HtmlEncode(exame.Crm ?? "-")
            };

            foreach (var kv in replacements)
            {
                template = template.Replace(kv.Key, kv.Value);
            }

            // Prepare temp directory and copy assets (css)
            string tempDir = Path.Combine(Path.GetTempPath(), "INTERFACE_POSTRATA_LAUDOS");
            Directory.CreateDirectory(tempDir);
            string tempHtml = Path.Combine(tempDir, $"laudo_{Guid.NewGuid():N}.html");
            string tempCss = Path.Combine(tempDir, "style.css");

            try
            {
                if (File.Exists(cssPath)) File.Copy(cssPath, tempCss, true);
                else File.WriteAllText(tempCss, "/* style fallback */", Encoding.UTF8);

                // Ensure the HTML references the local style.css (in same folder)
                template = template.Replace("href=\"style.css\"", $"href=\"{Path.GetFileName(tempCss)}\"");

                File.WriteAllText(tempHtml, template, Encoding.UTF8);

                // Open default browser
                var psi = new ProcessStartInfo(tempHtml) { UseShellExecute = true };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // fallback: write file and try to open anyway
                File.WriteAllText(tempHtml, template, Encoding.UTF8);
                try { Process.Start(new ProcessStartInfo(tempHtml) { UseShellExecute = true }); }
                catch { throw new InvalidOperationException("Não foi possível abrir o laudo HTML.", ex); }
            }
        }

        private static string HtmlEncode(string input) => WebUtility.HtmlEncode(input);

        private static bool TryParseDouble(string? input, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            string s = input.Trim().Replace(',', '.');
            return double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value);
        }
    }
}
