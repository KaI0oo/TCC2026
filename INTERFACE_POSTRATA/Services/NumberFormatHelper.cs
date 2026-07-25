using System.Globalization;

namespace INTERFACE_POSTRATA.Services
{
    public static class NumberFormatHelper
    {
        // Normaliza números aceitando "," ou "." e retornando string com ponto decimal para uso em Python/DB
        public static string? NormalizarNumero(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return null;
            try
            {
                numero = numero.Trim().Replace(',', '.');
                if (double.TryParse(numero, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                {
                    // Retorna representação invariável com ponto
                    return numero;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
