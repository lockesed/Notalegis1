using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Notalegis.Views
{
    public partial class VerStockWindow : Window
    {
        private readonly PedidoInsumoService pedidoService = new PedidoInsumoService();

        public VerStockWindow()
        {
            InitializeComponent();
            CargarStock();
        }

        private void CargarStock()
        {
            var stockDAO = new Notalegis.Datos.Repositorios.StockInsumoDAO();
            var stocks = stockDAO.ObtenerTodos();

            // Filtrar solo Hojas Protocolares
            var stockHojasProtocolares = stocks
                .Where(s => s.TipoInsumo == "Hojas Protocolares")
                .Select(s => new StockConEstado
                {
                    Id = s.Id,
                    TipoInsumo = s.TipoInsumo,
                    CantidadActual = s.CantidadActual,
                    StockMinimo = s.StockMinimo,
                    UltimaActualizacion = s.UltimaActualizacion,
                    StockBajo = s.CantidadActual <= s.StockMinimo
                }).ToList();

            dgStock.ItemsSource = stockHojasProtocolares;
            borderSinStock.Visibility = stockHojasProtocolares.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private class StockConEstado : StockInsumo
        {
            public bool StockBajo { get; set; }
        }
    }
}
