using Notalegis.Datos.Modelos;

namespace Notalegis.Models
{
    /// <summary>
    /// Almacena el usuario logueado durante toda la sesión.
    /// Se asigna en LoginView al hacer login exitoso y se usa
    /// para permisos, auditoría y filtrado por rol.
    /// </summary>
    public static class SesionActual
    {
        public static Usuario Usuario { get; private set; }

        public static string NombreUsuario => Usuario?.NombreUsuario ?? "Sistema";

        public static bool EsAdministrador =>
            string.Equals(Usuario?.Rol, "Administrador", System.StringComparison.OrdinalIgnoreCase);

        public static void IniciarSesion(Usuario usuario)
        {
            Usuario = usuario;
        }

        public static void CerrarSesion()
        {
            Usuario = null;
        }
    }
}
