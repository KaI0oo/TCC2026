using System;
using INTERFACE_POSTRATA.Models;

namespace INTERFACE_POSTRATA.Services
{
    public static class PdfImportService
    {
        // Extrai PSA do PDF e retorna um modelo Exame com campos PSA preenchidos (strings cruas)
        public static Exame? ImportFromPdf(string caminhoPdf)
        {
            try
            {
                var res = PdfExtractor.ExtractPsaValues(caminhoPdf);
                if (res == null) return null;

                var exame = new Exame
                {
                    PsaTotal = string.IsNullOrWhiteSpace(res.PsaTotal) ? null : res.PsaTotal.Replace('.', ','),
                    PsaLivre = string.IsNullOrWhiteSpace(res.PsaLivre) ? null : res.PsaLivre.Replace('.', ','),
                    PsaDensidade = string.IsNullOrWhiteSpace(res.PsaDensidade) ? null : res.PsaDensidade.Replace('.', ',')
                };

                return exame;
            }
            catch
            {
                return null;
            }
        }
    }
}
