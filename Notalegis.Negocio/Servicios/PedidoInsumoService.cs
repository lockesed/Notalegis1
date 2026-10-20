using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Notalegis.Negocio.Servicios
{
    public class PedidoInsumoService
    {
        private readonly PedidoInsumoDAO pedidoDAO = new PedidoInsumoDAO();
        private readonly StockInsumoDAO stockDAO = new StockInsumoDAO();
        private readonly AuditoriaService _auditoria = new AuditoriaService();

        public List<PedidoInsumo> ObtenerTodos()
        {
            return pedidoDAO.ObtenerTodos();
        }

        public PedidoInsumo ObtenerPorId(int id)
        {
            return pedidoDAO.ObtenerPorId(id);
        }

        public void GuardarPedido(PedidoInsumo pedido, string usuario = "Sistema")
        {
            pedidoDAO.Insertar(pedido);
            _auditoria.Registrar(usuario, "ALTA", "PedidosInsumos", null, $"Pedido de '{pedido.TipoInsumo}' (cant: {pedido.Cantidad}) creado");

            if (EsEstadoRecibido(pedido.Estado))
            {
                SumarStock(pedido.TipoInsumo, pedido.Cantidad);
            }
        }

        public void ModificarPedido(PedidoInsumo pedido, string usuario = "Sistema")
        {
            var pedidoAnterior = pedidoDAO.ObtenerPorId(pedido.Id);
            pedidoDAO.Modificar(pedido);
            _auditoria.Registrar(usuario, "MODIFICACION", "PedidosInsumos", pedido.Id, $"Pedido ID {pedido.Id} de '{pedido.TipoInsumo}' modificado");

            // El stock solo se suma la primera vez que el pedido pasa a "Recibido",
            // para no acumular de nuevo cada vez que se vuelve a guardar el mismo pedido.
            bool yaEstabaRecibido = pedidoAnterior != null && EsEstadoRecibido(pedidoAnterior.Estado);
            if (EsEstadoRecibido(pedido.Estado) && !yaEstabaRecibido)
            {
                SumarStock(pedido.TipoInsumo, pedido.Cantidad);
            }
        }

        private static bool EsEstadoRecibido(string estado)
        {
            return estado == "Recibido";
        }

        private void SumarStock(string tipoInsumo, int cantidad)
        {
            var stock = stockDAO.ObtenerPorTipo(tipoInsumo);
            stockDAO.InsertarOActualizar(new StockInsumo
            {
                TipoInsumo = tipoInsumo,
                CantidadActual = (stock?.CantidadActual ?? 0) + cantidad,
                StockMinimo = stock?.StockMinimo ?? 0,
                UltimaActualizacion = DateTime.Now
            });
        }

        public void EliminarPedido(int id, string usuario = "Sistema")
        {
            var pedido = pedidoDAO.ObtenerPorId(id);
            pedidoDAO.Eliminar(id);
            string desc = pedido != null
                ? $"Pedido de '{pedido.TipoInsumo}' (cant: {pedido.Cantidad}, solicitante: {pedido.Solicitante}) eliminado"
                : $"Pedido ID {id} eliminado";
            _auditoria.Registrar(usuario, "BAJA", "PedidosInsumos", id, desc);
        }

        public List<PedidoInsumo> ObtenerPedidosDelMes(int mes, int anio)
        {
            return pedidoDAO.ObtenerPorMes(mes, anio);
        }

        public List<StockInsumo> ObtenerStockBajo()
        {
            return stockDAO.ObtenerStockBajo();
        }

        public void ActualizarStock(string tipoInsumo, int cantidad)
        {
            stockDAO.ActualizarStock(tipoInsumo, cantidad);
        }

        public StockInsumo ObtenerStockPorTipo(string tipoInsumo)
        {
            return stockDAO.ObtenerPorTipo(tipoInsumo);
        }

        public void GuardarOActualizarStock(StockInsumo stock)
        {
            stockDAO.InsertarOActualizar(stock);
        }
    }
}
