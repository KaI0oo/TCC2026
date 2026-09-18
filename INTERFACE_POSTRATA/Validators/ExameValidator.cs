using System;
using System.IO;
using System.Globalization;
using INTERFACE_POSTRATA.Validators;

namespace INTERFACE_POSTRATA.Validators
{
    public class ExameValidated
    {
        public double PsaTotal { get; set; }
        public double PsaLivre { get; set; }
        public double Densidade { get; set; }
        public string CpfPaciente { get; set; } = string.Empty;
        public string CaminhoPdf { get; set; } = string.Empty;
    }

    public static class ExameValidator
    {
        public static ValidationResult<ExameValidated> Validate(string? cpfPaciente, string? psaTotalRaw, string? psaLivreRaw, string? densidadeRaw, string? caminhoPdf)
        {
            var res = new ValidationResult<ExameValidated> { IsValid = false };

            if (string.IsNullOrWhiteSpace(cpfPaciente))
            {
                res.Message = "CPF do paciente é obrigatório para o exame.";
                return res;
            }

            // Validar PSA Total (Obrigatório, > 0)
            if (!TryParseDouble(psaTotalRaw, out double psaTotal) || psaTotal <= 0)
            {
                res.Message = "PSA Total é obrigatório e deve ser maior que zero.";
                return res;
            }

            // Validar PSA Livre (Obrigatório, >= 0)
            if (!TryParseDouble(psaLivreRaw, out double psaLivre) || psaLivre < 0)
            {
                res.Message = "PSA Livre é obrigatório e não pode ser negativo.";
                return res;
            }

            double densidade = 0;
            if (!string.IsNullOrWhiteSpace(densidadeRaw))
            {
                if (!TryParseDouble(densidadeRaw, out densidade) || densidade < 0)
                {
                    res.Message = "Densidade PSA inválida. Deve ser um número não negativo.";
                    return res;
                }
            }

            // Validar existência do PDF se informado
            if (!string.IsNullOrWhiteSpace(caminhoPdf) && !File.Exists(caminhoPdf))
            {
                res.Message = "Arquivo PDF informado não foi encontrado.";
                return res;
            }

            res.IsValid = true;
            res.Value = new ExameValidated
            {
                CpfPaciente = cpfPaciente ?? string.Empty,
                PsaTotal = psaTotal,
                PsaLivre = psaLivre,
                Densidade = string.IsNullOrWhiteSpace(densidadeRaw) ? 0 : (double)Convert.ToDouble(densidadeRaw.Replace(',', '.'), CultureInfo.InvariantCulture),
                CaminhoPdf = caminhoPdf ?? string.Empty
            };

            return res;
        }

        private static bool TryParseDouble(string? input, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            string s = input.Trim().Replace(',', '.');
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
