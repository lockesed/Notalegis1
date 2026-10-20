using Notalegis.Datos.Repositorios;
using Notalegis.Datos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notalegis.Negocio.Servicios
{
    public class ClienteService
    {
        private readonly ClienteDAO dao = new ClienteDAO();
        private readonly AuditoriaService _auditoria = new AuditoriaService();

        public List<Cliente> ObtenerTodos()
        {
            return dao.ObtenerTodos();
        }

        public bool ExisteDNI(string dni, int idExcluir = 0)
        {
            return dao.ExisteDNI(dni, idExcluir);
        }

        public void GuardarCliente(Cliente cliente, string usuario = "Sistema")
        {
            dao.Insertar(cliente);
            _auditoria.Registrar(usuario, "ALTA", "Clientes", null, $"Cliente '{cliente.Nombre} {cliente.Apellido}' (DNI: {cliente.DNI}) creado");
        }

        public void ModificarCliente(Cliente cliente, string usuario = "Sistema")
        {
            dao.Modificar(cliente);
            _auditoria.Registrar(usuario, "MODIFICACION", "Clientes", cliente.Id, $"Cliente '{cliente.Nombre} {cliente.Apellido}' modificado");
        }

        public void EliminarCliente(int id, string usuario = "Sistema")
        {
            var cliente = dao.ObtenerPorId(id);
            dao.Eliminar(id);
            string desc = cliente != null
                ? $"Cliente '{cliente.Nombre} {cliente.Apellido}' (DNI: {cliente.DNI}) eliminado"
                : $"Cliente ID {id} eliminado";
            _auditoria.Registrar(usuario, "BAJA", "Clientes", id, desc);
        }

        public void RestaurarCliente(int id)
        {
            dao.Restaurar(id);
        }

        public List<Cliente> ObtenerInactivos()
        {
            return dao.ObtenerInactivos();
        }
    }
}

