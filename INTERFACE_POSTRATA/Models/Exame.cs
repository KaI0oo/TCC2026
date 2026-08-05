using System;

namespace INTERFACE_POSTRATA.Models
{
    public class Exame
    {
        public string? PacienteNome { get; set; }
        public string? Idade { get; set; }
        public string? Medico { get; set; }
        public string? Crm { get; set; }
        public string? PsaTotal { get; set; }
        public string? PsaLivre { get; set; }
        public string? PsaDensidade { get; set; }
        public string? Resultado { get; set; }
        public DateTime? DataExame { get; set; }
    }
}
