
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using System;
using System.Collections.Generic;

namespace Notalegis.Negocio.Servicios
{
    public class EstadoClienteService
    {
        private readonly EstadoClienteDAO estadoClienteDAO = new EstadoClienteDAO();

        public void RegistrarEstado(int idCliente, string nuevoEstado)
        {
            EstadoCliente estado = new EstadoCliente
            {
                IdCliente = idCliente,
                Estado = nuevoEstado,
                FechaRegistro = DateTime.Now
            };
            estadoClienteDAO.RegistrarEstado(estado);
        }

        public List<EstadoCliente> ObtenerHistorial(int idCliente)
        {
            return estadoClienteDAO.ObtenerHistorialPorCliente(idCliente);
        }
    }
}