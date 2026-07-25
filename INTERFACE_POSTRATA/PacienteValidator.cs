using System.Linq;
using System.Text.RegularExpressions;
using INTERFACE_POSTRATA.Validators;

namespace INTERFACE_POSTRATA
{
    // Validador centralizado para Paciente - não lança exceções, retorna ValidationResult
    public static class PacienteValidator
    {
        public static ValidationResult<int> Validate(string? cpf, string? nome, string? idadeStr, string? telefone)
        {
            var result = new ValidationResult<int> { IsValid = false };

            if (string.IsNullOrWhiteSpace(cpf))
            {
                result.Message = "CPF é obrigatório.";
                return result;
            }

            string cpfDigits = new string(cpf.Where(char.IsDigit).ToArray());
            if (cpfDigits.Length != 11)
            {
                result.Message = "CPF deve conter 11 dígitos.";
                return result;
            }

            if (!IsCpfValid(cpfDigits))
            {
                result.Message = "CPF inválido.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(nome))
            {
                result.Message = "Nome é obrigatório.";
                return result;
            }

            if (Regex.IsMatch(nome, "\\d"))
            {
                result.Message = "Nome não pode conter números.";
                return result;
            }

            if (nome.Length < 2 || nome.Length > 100)
            {
                result.Message = "Nome deve ter entre 2 e 100 caracteres.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(idadeStr) || !int.TryParse(idadeStr, out int idade) || idade <= 0)
            {
                result.Message = "Idade inválida. Informe um número inteiro positivo.";
                return result;
            }

            if (!string.IsNullOrWhiteSpace(telefone))
            {
                string phoneDigits = new string(telefone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 8 || phoneDigits.Length > 15)
                {
                    result.Message = "Telefone deve conter entre 8 e 15 dígitos.";
                    return result;
                }
                if (!Regex.IsMatch(telefone, @"^[0-9()+\s\-]*$"))
                {
                    result.Message = "Telefone contém caracteres inválidos.";
                    return result;
                }
            }

            result.IsValid = true;
            result.Value = idade;
            return result;
        }

        // Implementação do cálculo dos dígitos verificadores do CPF
        private static bool IsCpfValid(string cpf)
        {
            if (cpf.Length != 11) return false;
            if (cpf.Distinct().Count() == 1) return false;
            int[] numbers = cpf.Select(c => c - '0').ToArray();
            int sum = 0;
            for (int i = 0; i < 9; i++) sum += numbers[i] * (10 - i);
            int remainder = sum % 11;
            int firstCheck = remainder < 2 ? 0 : 11 - remainder;
            if (numbers[9] != firstCheck) return false;
            sum = 0;
            for (int i = 0; i < 10; i++) sum += numbers[i] * (11 - i);
            remainder = sum % 11;
            int secondCheck = remainder < 2 ? 0 : 11 - remainder;
            if (numbers[10] != secondCheck) return false;
            return true;
        }
    }
}
