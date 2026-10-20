using System.Text.RegularExpressions;

namespace Notalegis.Helpers
{
    /// <summary>
    /// Métodos de validación reutilizables para formularios.
    /// Evita duplicar las mismas expresiones regulares en múltiples vistas.
    /// </summary>
    public static class ValidacionHelper
    {
        private static readonly Regex _regexEmail =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _regexDni =
            new Regex(@"^\d{7,11}$", RegexOptions.Compiled);

        private static readonly Regex _regexTelefono =
            new Regex(@"^[\d\s\-\+\(\)]{6,20}$", RegexOptions.Compiled);

        private static readonly Regex _regexSoloLetras =
            new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", RegexOptions.Compiled);

        private static readonly Regex _regexNumeroPositivo =
            new Regex(@"^\d+(\.\d{1,2})?$", RegexOptions.Compiled);

        public static bool EsEmailValido(string valor)
            => !string.IsNullOrWhiteSpace(valor) && _regexEmail.IsMatch(valor.Trim());

        public static bool EsDniValido(string valor)
            => !string.IsNullOrWhiteSpace(valor) && _regexDni.IsMatch(valor.Trim());

        public static bool EsTelefonoValido(string valor)
            => string.IsNullOrWhiteSpace(valor) || _regexTelefono.IsMatch(valor.Trim());

        public static bool EsSoloLetras(string valor)
            => !string.IsNullOrWhiteSpace(valor) && _regexSoloLetras.IsMatch(valor);

        public static bool EsNumeroPositivo(string valor)
            => !string.IsNullOrWhiteSpace(valor) && _regexNumeroPositivo.IsMatch(valor.Trim());

        public static bool EsNumeroDecimalValido(string valor, out decimal resultado)
        {
            resultado = 0;
            if (string.IsNullOrWhiteSpace(valor)) return false;
            return decimal.TryParse(valor.Replace(",", "."),
                                    System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out resultado) && resultado >= 0;
        }
    }
}
