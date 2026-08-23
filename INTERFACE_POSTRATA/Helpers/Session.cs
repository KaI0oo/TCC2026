using System;

namespace INTERFACE_POSTRATA.Helpers
{
    public static class Session
    {
        // Id do funcionário autenticado (pode ser null se não autenticado)
        public static int? CurrentFuncionarioId { get; set; }

        // Nome exibido do funcionário
        public static string CurrentFuncionarioName { get; set; }

        // CRM do usuário autenticado (pode ser null para RH/Secretaria)
        public static string CurrentFuncionarioCrm { get; set; }

        // Cargo do usuário autenticado (RH, MEDICO, SECRETARIA)
        public static string CurrentFuncionarioCargo { get; set; }

        public static bool IsSecretaria =>
            CurrentFuncionarioCargo?.Equals("SECRETARIA", StringComparison.OrdinalIgnoreCase) == true
            || CurrentFuncionarioCargo?.Equals("Secretaria", StringComparison.OrdinalIgnoreCase) == true;

        public static bool IsMedico =>
            CurrentFuncionarioCargo?.Equals("MEDICO", StringComparison.OrdinalIgnoreCase) == true
            || CurrentFuncionarioCargo?.Equals("Medico", StringComparison.OrdinalIgnoreCase) == true;
    }
}
