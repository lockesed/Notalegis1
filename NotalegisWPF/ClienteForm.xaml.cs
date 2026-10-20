using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace NotalegisWPF
{
    public partial class ClienteForm : Window
    {
        private readonly ClienteService clienteService = new ClienteService();
        private readonly EstadoClienteService estadoService = new EstadoClienteService();
        private int idClienteSeleccionado = -1;

        public ClienteForm()
        {
            InitializeComponent();
            CargarClientes();
            dgvClientes.SelectionChanged += DgvClientes_SelectionChanged;
            btnGuardar.Click += BtnGuardar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnActualizarEstado.Click += BtnActualizarEstado_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
        }

        private void CargarClientes()
        {
            var clientes = clienteService.ObtenerTodos(); // ✅ llamada correcta
            dgvClientes.ItemsSource = clientes;
        }

        private void LimpiarCampos()
        {
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDNI.Text = "";
            txtEdad.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtDireccion.Text = "";
            cmbEstado.SelectedIndex = -1;
            idClienteSeleccionado = -1;
            dgvHistorialEstados.ItemsSource = null;
        }

        private void DgvClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                txtNombre.Text = cliente.Nombre;
                txtApellido.Text = cliente.Apellido;
                txtDNI.Text = cliente.DNI;
                txtEdad.Text = cliente.Edad.ToString();
                txtTelefono.Text = cliente.Telefono;
                txtEmail.Text = cliente.Email;
                txtDireccion.Text = cliente.Direccion;
                idClienteSeleccionado = cliente.Id;
                CargarHistorialEstados(cliente.Id);
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validación: Nombre obligatorio y solo letras
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El nombre solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Apellido obligatorio y solo letras
            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("El apellido es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El apellido solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: DNI obligatorio y solo números
            if (string.IsNullOrWhiteSpace(txtDNI.Text))
            {
                MessageBox.Show("El DNI es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!txtDNI.Text.All(char.IsDigit))
            {
                MessageBox.Show("El DNI solo debe contener números.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Teléfono obligatorio y solo números con caracteres especiales
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

            // Validación: Email obligatorio con formato válido
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

            // Validación: Dirección obligatoria con letras y números
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

            var cliente = new Cliente
            {
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                DNI = txtDNI.Text,
                Edad = int.TryParse(txtEdad.Text, out int edad) ? edad : 0,
                Telefono = txtTelefono.Text,
                Email = txtEmail.Text,
                Direccion = txtDireccion.Text
            };

            clienteService.GuardarCliente(cliente);
            MessageBox.Show("Cliente guardado correctamente.");
            CargarClientes();
            LimpiarCampos();
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            if (idClienteSeleccionado == -1)
            {
                MessageBox.Show("Seleccioná un cliente para modificar.");
                return;
            }

            // Validación: Nombre obligatorio y solo letras
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El nombre solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Apellido obligatorio y solo letras
            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("El apellido es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El apellido solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: DNI obligatorio y solo números
            if (string.IsNullOrWhiteSpace(txtDNI.Text))
            {
                MessageBox.Show("El DNI es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!txtDNI.Text.All(char.IsDigit))
            {
                MessageBox.Show("El DNI solo debe contener números.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Teléfono obligatorio y solo números con caracteres especiales
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

            // Validación: Email obligatorio con formato válido
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

            // Validación: Dirección obligatoria con letras y números
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

            var cliente = new Cliente
            {
                Id = idClienteSeleccionado,
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                DNI = txtDNI.Text,
                Edad = int.TryParse(txtEdad.Text, out int edad) ? edad : 0,
                Telefono = txtTelefono.Text,
                Email = txtEmail.Text,
                Direccion = txtDireccion.Text
            };

            clienteService.ModificarCliente(cliente);
            MessageBox.Show("Cliente modificado correctamente.");
            CargarClientes();
            LimpiarCampos();
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (idClienteSeleccionado == -1)
            {
                MessageBox.Show("Seleccioná un cliente para eliminar.");
                return;
            }

            clienteService.EliminarCliente(idClienteSeleccionado);
            MessageBox.Show("Cliente eliminado correctamente.");
            CargarClientes();
            LimpiarCampos();
        }

        private void BtnActualizarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (idClienteSeleccionado == -1)
            {
                MessageBox.Show("Seleccioná un cliente para actualizar el estado.");
                return;
            }

            if (cmbEstado.SelectedItem is ComboBoxItem item)
            {
                string estado = item.Content.ToString();
                estadoService.RegistrarEstado(idClienteSeleccionado, estado);
                MessageBox.Show("Estado actualizado correctamente.");
                CargarHistorialEstados(idClienteSeleccionado);
            }
        }

        private void CargarHistorialEstados(int idCliente)
        {
            var historial = estadoService.ObtenerHistorial(idCliente);
            dgvHistorialEstados.ItemsSource = historial.Select(e => new
            {
                Estado = e.Estado,
                Fecha = e.FechaRegistro.ToString("g")
            }).ToList();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }
    }
}

