using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NotalegisWPF.Views
{
    public partial class ClientesInactivosView : Window
    {
        private readonly ClienteService clienteService = new ClienteService();

        public ClientesInactivosView()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            var inactivos = clienteService.ObtenerInactivos();
            dgvClientes.ItemsSource = inactivos;
            borderSinInactivos.Visibility = inactivos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();
            var inactivos = clienteService.ObtenerInactivos();

            var filtrados = inactivos.Where(c => 
                c.Nombre.ToLower().Contains(filtro) || 
                c.Apellido.ToLower().Contains(filtro) || 
                c.DNI.Contains(filtro)).ToList();

            dgvClientes.ItemsSource = filtrados;
        }

        private void btnReactivar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                if (MessageBox.Show($"¿Desea reactivar al cliente {cliente.Nombre} {cliente.Apellido}?", 
                    "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    clienteService.RestaurarCliente(cliente.Id);
                    MessageBox.Show("Cliente reactivado correctamente.");
                    CargarClientes(); // Recargar la lista
                }
            }
        }
    }
}
