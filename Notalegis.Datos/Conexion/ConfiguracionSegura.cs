using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace Notalegis.Datos.Conexion
{
    [SupportedOSPlatform("windows")]
    public static class ConfiguracionSegura
    {
        private static string? _cadena;
        private static readonly string _rutaDat =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "conn.dat");

        public static string Cadena
        {
            get
            {
                if (_cadena != null) return _cadena;
                _cadena = CargarCadena();
                return _cadena;
            }
        }

        private static string CargarCadena()
        {
            if (File.Exists(_rutaDat))
                return Descifrar();

            var plainText = ConfigurationManager.ConnectionStrings["conexionBD"]?.ConnectionString
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión en App.config.");

            Cifrar(plainText);
            return plainText;
        }

        private static void Cifrar(string texto)
        {
            var bytes = Encoding.UTF8.GetBytes(texto);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.LocalMachine);
            File.WriteAllBytes(_rutaDat, encrypted);
        }

        private static string Descifrar()
        {
            var encrypted = File.ReadAllBytes(_rutaDat);
            var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
