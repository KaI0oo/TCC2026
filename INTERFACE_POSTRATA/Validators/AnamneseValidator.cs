using System;
using System.Globalization;
using INTERFACE_POSTRATA.Validators;

namespace INTERFACE_POSTRATA.Validators
{
    public class AnamneseValidated
    {
        public string CpfPaciente { get; set; } = string.Empty;
        public int? RmMedico { get; set; }
        public bool PossuiDoenca { get; set; }
        public string Doencas { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public bool TomaRemedio { get; set; }
        public string RemedioNome { get; set; } = string.Empty;
        public double? DosagemMg { get; set; }
        public DateTime? InicioTratamento { get; set; }
        public DateTime? FimTratamento { get; set; }
        public string Tabagismo { get; set; } = string.Empty;
        public string Alcool { get; set; } = string.Empty;
        public string Frequencia { get; set; } = string.Empty;
    }

    public static class AnamneseValidator
    {
        public static ValidationResult<AnamneseValidated> Validate(
            string? cpfPaciente,
            string? rmMedicoRaw,
            bool possuiDoenca,
            string? doencas,
            string? observacoes,
            bool tomaRemedio,
            string? remedioNome,
            string? dosagemRaw,
            DateTime? inicio,
            DateTime? fim,
            string? tabagismo,
            string? alcool,
            string? frequencia)
        {
            var res = new ValidationResult<AnamneseValidated> { IsValid = false };

            if (string.IsNullOrWhiteSpace(cpfPaciente))
            {
                res.Message = "CPF do paciente é obrigatório.";
                return res;
            }

            if (possuiDoenca && string.IsNullOrWhiteSpace(doencas))
            {
                res.Message = "Informe as doenças quando 'possui doença' estiver marcado.";
                return res;
            }

            if (tomaRemedio && string.IsNullOrWhiteSpace(remedioNome))
            {
                res.Message = "Informe o nome do remédio quando 'toma remédio' estiver marcado.";
                return res;
            }

            double? dosagem = null;
            if (!string.IsNullOrWhiteSpace(dosagemRaw))
            {
                if (!double.TryParse(dosagemRaw.Replace(',', '.'), System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                {
                    res.Message = "Dosagem inválida.";
                    return res;
                }
                if (d < 0)
                {
                    res.Message = "Dosagem não pode ser negativa.";
                    return res;
                }
                dosagem = d;
            }

            if (inicio.HasValue && fim.HasValue && fim < inicio)
            {
                res.Message = "Data de fim deve ser igual ou posterior à data de início.";
                return res;
            }

            res.IsValid = true;
            res.Value = new AnamneseValidated
            {
                CpfPaciente = cpfPaciente ?? string.Empty,
                RmMedico = int.TryParse(rmMedicoRaw, out int rm) ? rm : (int?)null,
                PossuiDoenca = possuiDoenca,
                Doencas = doencas ?? string.Empty,
                Observacoes = observacoes ?? string.Empty,
                TomaRemedio = tomaRemedio,
                RemedioNome = remedioNome ?? string.Empty,
                DosagemMg = dosagem,
                InicioTratamento = inicio,
                FimTratamento = fim,
                Tabagismo = tabagismo ?? string.Empty,
                Alcool = alcool ?? string.Empty,
                Frequencia = frequencia ?? string.Empty
            };

            return res;
        }
    }
}
