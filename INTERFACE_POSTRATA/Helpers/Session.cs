namespace INTERFACE_POSTRATA.Helpers
{
    public static class Session
    {
        // Id do médico autenticado (pode ser null se não autenticado)
        public static int? CurrentMedicoId { get; set; }

        // Nome exibido do médico (usado em tabelas que armazenam nome)
        public static string CurrentMedicoName { get; set; }

        // CRM do usuário autenticado
        public static string CurrentMedicoCrm { get; set; }

        // Cargo do usuário autenticado (RH, Medico, Secretaria)
        public static string CurrentMedicoCargo { get; set; }
    }
}
