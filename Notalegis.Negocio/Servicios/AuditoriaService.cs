using System.Collections.Generic;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Negocio.Servicios
{
    public class AuditoriaService
    {
        private readonly AuditoriaDAO _dao = new AuditoriaDAO();

        public void Registrar(string usuario, string accion, string modulo, int? entidadId = null, string descripcion = null)
        {
            _dao.Registrar(usuario, accion, modulo, entidadId, descripcion);
        }

        public List<RegistroAuditoria> ObtenerTodos(int limite = 500)
        {
            return _dao.ObtenerTodos(limite);
        }

        public List<RegistroAuditoria> ObtenerPorUsuario(string usuario, int limite = 200)
        {
            return _dao.ObtenerPorUsuario(usuario, limite);
        }
    }
}
