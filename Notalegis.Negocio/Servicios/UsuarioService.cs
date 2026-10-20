using System;
using System.Collections.Generic;
using System.Linq;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Negocio.Servicios
{
    public class UsuarioService
    {
        private readonly UsuarioDAO _usuarioDAO = new UsuarioDAO();
        private readonly AuditoriaService _auditoria = new AuditoriaService();

        public enum ResultadoLogin { Ok, CredencialesInvalidas, CuentaBloqueada }

        public (ResultadoLogin resultado, Usuario usuario, DateTime? bloqueadoHasta) ValidarLoginConBloqueo(string nombreUsuario, string contraseña)
        {
            Usuario usuarioDB = _usuarioDAO.ObtenerPorNombreUsuario(nombreUsuario);

            if (usuarioDB == null)
                return (ResultadoLogin.CredencialesInvalidas, null, null);

            // Verificar bloqueo activo
            if (usuarioDB.BloqueadoHasta.HasValue && usuarioDB.BloqueadoHasta.Value > DateTime.Now)
                return (ResultadoLogin.CuentaBloqueada, null, usuarioDB.BloqueadoHasta);

            bool credencialesOk = false;
            try
            {
                credencialesOk = BCrypt.Net.BCrypt.Verify(contraseña, usuarioDB.Contraseña);
            }
            catch
            {
                credencialesOk = usuarioDB.Contraseña == contraseña;
            }

            if (credencialesOk)
            {
                _usuarioDAO.ResetearIntentos(usuarioDB.Id);
                return (ResultadoLogin.Ok, usuarioDB, null);
            }

            _usuarioDAO.RegistrarIntentoFallido(usuarioDB.Id);
            // Re-leer para saber el estado actualizado
            var actualizado = _usuarioDAO.ObtenerPorNombreUsuario(nombreUsuario);
            if (actualizado?.BloqueadoHasta.HasValue == true && actualizado.BloqueadoHasta.Value > DateTime.Now)
                return (ResultadoLogin.CuentaBloqueada, null, actualizado.BloqueadoHasta);

            return (ResultadoLogin.CredencialesInvalidas, null, null);
        }

        public Usuario ValidarLogin(string nombreUsuario, string contraseña)
        {
            var (resultado, usuario, _) = ValidarLoginConBloqueo(nombreUsuario, contraseña);
            return resultado == ResultadoLogin.Ok ? usuario : null;
        }

        public List<Usuario> ObtenerTodos()
        {
            return _usuarioDAO.ObtenerTodos();
        }

        public void Agregar(Usuario usuario, string usuarioActual = "Sistema")
        {
            // Validar existencia
            if (_usuarioDAO.ObtenerPorNombreUsuario(usuario.NombreUsuario) != null)
            {
                throw new Exception("Ya existe un usuario con este nombre.");
            }

            // Hashear contraseña antes de guardar
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);
            _usuarioDAO.Agregar(usuario);
            _auditoria.Registrar(usuarioActual, "ALTA", "Usuarios", null, $"Usuario '{usuario.NombreUsuario}' creado con rol '{usuario.Rol}'");
        }

        public void Actualizar(Usuario usuarioActualizado, string usuarioActual = "Sistema")
        {
            var usuarioDB = _usuarioDAO.ObtenerPorNombreUsuario(usuarioActualizado.NombreUsuario);

            // Validar que si cambiamos el nombre de usuario, no colisione con otro
            if (usuarioDB != null && usuarioDB.Id != usuarioActualizado.Id)
            {
                throw new Exception("Ya existe otro usuario con este nombre.");
            }

            // Si la contraseña viene vacía o nula, significa que no queremos cambiarla
            if (string.IsNullOrWhiteSpace(usuarioActualizado.Contraseña))
            {
                // Mantenemos la anterior (hay que buscarla temporalmente si no la teníamos en memoria)
                var temp = _usuarioDAO.ObtenerTodos().Find(u => u.Id == usuarioActualizado.Id);
                if (temp != null)
                {
                    usuarioActualizado.Contraseña = temp.Contraseña;
                }
            }
            else
            {
                // Hashear la nueva contraseña
                usuarioActualizado.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuarioActualizado.Contraseña);
            }

            _usuarioDAO.Actualizar(usuarioActualizado);
            _auditoria.Registrar(usuarioActual, "MODIFICACION", "Usuarios", usuarioActualizado.Id, $"Usuario '{usuarioActualizado.NombreUsuario}' modificado");
        }

        public void Eliminar(int id, int idUsuarioActual, string usuarioActual = "Sistema")
        {
            if (id == idUsuarioActual)
            {
                throw new Exception("No podés eliminar tu propio usuario mientras estás logueado con él.");
            }

            var usuario = _usuarioDAO.ObtenerTodos().Find(u => u.Id == id);
            if (usuario == null)
            {
                throw new Exception("El usuario no existe.");
            }

            if (string.Equals(usuario.Rol, "Administrador", StringComparison.OrdinalIgnoreCase))
            {
                int cantidadAdmins = _usuarioDAO.ObtenerTodos().Count(u => string.Equals(u.Rol, "Administrador", StringComparison.OrdinalIgnoreCase));
                if (cantidadAdmins <= 1)
                {
                    throw new Exception("No se puede eliminar el último administrador del sistema.");
                }
            }

            _usuarioDAO.Eliminar(id);
            _auditoria.Registrar(usuarioActual, "BAJA", "Usuarios", id, $"Usuario '{usuario.NombreUsuario}' eliminado");
        }
    }
}
