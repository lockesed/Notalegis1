using Notalegis.Datos.Modelos;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Notalegis.Views
{
    public partial class EditarPedidoWindow : Window
    {
        private readonly PedidoInsumoService pedidoService = new PedidoInsumoService();
        private readonly PedidoInsumo pedido;
        private bool _guardado = false;

        private readonly List<string> estados = new List<string>
        {
            "Pendiente",
            "Enviado",
            "Recibido",
            "Cancelado"
        };

        public EditarPedidoWindow(PedidoInsumo pedidoAEditar)
        {
            InitializeComponent();
            pedido = pedidoAEditar;
            CargarDatos();
        }

        private void CargarDatos()
        {
            cmbEstado.ItemsSource = estados;

            dpFechaPedido.SelectedDate = pedido.FechaPedido;
            txtTipoInsumo.Text = pedido.TipoInsumo;
            txtCantidad.Text = pedido.Cantidad.ToString();
            txtSolicitante.Text = pedido.Solicitante;
            cmbEstado.Text = pedido.Estado;
            txtEmailDestino.Text = pedido.EmailDestino;
            txtObservaciones.Text = pedido.Observaciones;
            chkFirmado.IsChecked = pedido.Firmado;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (dpFechaPedido.SelectedDate == null)
            {
                MessageBox.Show("La fecha del pedido es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCantidad.Text) || !int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Debe ingresar una cantidad válida mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSolicitante.Text))
            {
                MessageBox.Show("El solicitante es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbEstado.Text))
            {
                MessageBox.Show("El estado es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Actualizar el pedido
            pedido.FechaPedido = dpFechaPedido.SelectedDate.Value;
            pedido.TipoInsumo = "Hojas Protocolares";
            pedido.Cantidad = cantidad;
            pedido.Solicitante = txtSolicitante.Text;
            pedido.Estado = cmbEstado.Text;
            pedido.EmailDestino = txtEmailDestino.Text;
            pedido.Observaciones = txtObservaciones.Text;
            pedido.Firmado = chkFirmado.IsChecked == true;

            try
            {
                pedidoService.ModificarPedido(pedido, SesionActual.NombreUsuario);
                MessageBox.Show("Pedido actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                _guardado = true;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el pedido: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _guardado = true;
            this.DialogResult = false;
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_guardado && !Notalegis.App.BloqueoAutomatico)
            {
                var resultado = MessageBox.Show(
                    "¿Seguro que desea cerrar? Los cambios no guardados se perderán.",
                    "Confirmar cierre",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                    e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void txtCantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Solo permitir números
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }
}
