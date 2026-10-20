using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Notalegis.Views
{
    public partial class EditarClienteWindow : Window
    {
        private Cliente cliente;
        private readonly ClienteService clienteService = new ClienteService();
        private bool _guardado = false;
        public EditarClienteWindow(Cliente clienteSeleccionado)
        {
            InitializeComponent();
            cliente = clienteSeleccionado;
            CargarDatosDesdeInstancia();
        }

        private void GuardarCambiosCliente()
        {
            // Obtener cliente actualizado desde los campos
            cliente.Nombre = txtNombre.Text.Trim();
            cliente.Apellido = txtApellido.Text.Trim();
            cliente.DNI = txtDocumento.Text.Trim();
            cliente.Telefono = txtTelefono.Text.Trim();
            cliente.Email = txtEmail.Text.Trim();
            cliente.Direccion = txtDireccion.Text.Trim();
            cliente.Estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString();


            // Observaciones opcional
            cliente.Observaciones = txtObservaciones?.Text?.Trim() ?? "";

            try
            {
                clienteService.ModificarCliente(cliente, SesionActual.NombreUsuario);
                _guardado = true;
                MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el cliente: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosDesdeInstancia()
        {
            txtNombre.Text = cliente.Nombre;
            txtApellido.Text = cliente.Apellido;
            txtDocumento.Text = cliente.DNI;
            txtTelefono.Text = cliente.Telefono;
            txtEmail.Text = cliente.Email;
            txtDireccion.Text = cliente.Direccion;
            cmbEstado.Text = cliente.Estado;
            txtObservaciones.Text = cliente.Observaciones ?? "";
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Nombre: obligatorio y solo letras con tildes y espacios
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                !Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El nombre es obligatorio y solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Apellido: obligatorio y solo letras con tildes y espacios
            if (string.IsNullOrWhiteSpace(txtApellido.Text) ||
                !Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El apellido es obligatorio y solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // DNI: obligatorio y solo números
            if (string.IsNullOrWhiteSpace(txtDocumento.Text) || !txtDocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El DNI es obligatorio y solo debe contener números.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (txtDocumento.Text.Trim().Length < 7 || txtDocumento.Text.Trim().Length > 11)
            {
                MessageBox.Show("El DNI/CUIT debe tener entre 7 y 11 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (clienteService.ExisteDNI(txtDocumento.Text.Trim(), cliente.Id))
            {
                MessageBox.Show("Ya existe otro cliente registrado con ese DNI/CUIT.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Teléfono: obligatorio y solo números con caracteres especiales permitidos (+, -, espacios, paréntesis)
            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MessageBox.Show("El teléfono es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtTelefono.Text, @"^[0-9+\-\s()]+$"))
            {
                MessageBox.Show("El teléfono solo debe contener números y los caracteres: + - ( ) espacios", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int cantidadDigitosTelefono = txtTelefono.Text.Count(char.IsDigit);
            if (cantidadDigitosTelefono < 6 || cantidadDigitosTelefono > 15)
            {
                MessageBox.Show("El teléfono debe tener entre 6 y 15 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Email: obligatorio con formato válido
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("El email es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtEmail.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("El email debe tener un formato válido (ejemplo@ejemplo.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Dirección: obligatoria con letras y números
            if (string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MessageBox.Show("La dirección es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtDireccion.Text, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s,.#-]+$"))
            {
                MessageBox.Show("La dirección solo debe contener letras, números y caracteres básicos (. , # -).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Estado: obligatorio
            if (cmbEstado.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un estado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Observaciones: NO es obligatorio (no se valida)

            // Si todo está ok, continuar con el guardado
            GuardarCambiosCliente();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _guardado = true;
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
    }
}
