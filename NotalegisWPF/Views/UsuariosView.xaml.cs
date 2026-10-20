using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Notalegis.Datos.Modelos;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class UsuariosView : Window
    {
        private readonly UsuarioService _usuarioService;
        private List<Usuario> _usuariosOriginales;
        private Usuario _usuarioEnEdicion;

        public UsuariosView()
        {
            InitializeComponent();
            _usuarioService = new UsuarioService();
            _usuariosOriginales = new List<Usuario>();
        }

        private void UsuariosView_Loaded(object sender, RoutedEventArgs e)
        {
            CargarUsuarios();
        }

        private void CargarUsuarios()
        {
            try
            {
                _usuariosOriginales = _usuarioService.ObtenerTodos();
                dgvUsuarios.ItemsSource = _usuariosOriginales;
                borderSinUsuarios.Visibility = _usuariosOriginales.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los usuarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtNombreUsuario.Text = string.Empty;
            txtPassword.Password = string.Empty;
            cmbRol.SelectedIndex = -1;

            _usuarioEnEdicion = null;
            dgvUsuarios.SelectedItem = null;
            txtTituloFormulario.Text = "Nuevo usuario";
            btnGuardar.Content = "Nuevo / Guardar";
            txtNombreUsuario.Focus();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario()) return;

            try
            {
                if (_usuarioEnEdicion == null)
                {
                    // Estamos CREANDO un usuario nuevo, la password es OBLIGATORIA
                    if (string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        MessageBox.Show("Debe ingresar una contraseña para el usuario nuevo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var nuevoUsuario = new Usuario
                    {
                        NombreUsuario = txtNombreUsuario.Text.Trim(),
                        Contraseña = txtPassword.Password,
                        Rol = (cmbRol.SelectedItem as ComboBoxItem)?.Content.ToString()
                    };

                    _usuarioService.Agregar(nuevoUsuario, SesionActual.NombreUsuario);
                    MessageBox.Show("Usuario creado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Estamos EDITANDO
                    _usuarioEnEdicion.NombreUsuario = txtNombreUsuario.Text.Trim();
                    // Si el PasswordBox tiene texto, lo actualizamos. Si está vacío, el Service mantiene el hash anterior.
                    if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                    {
                        _usuarioEnEdicion.Contraseña = txtPassword.Password;
                    }
                    else
                    {
                         // Mandamos vacio y el servicio sabrá qué hacer
                         _usuarioEnEdicion.Contraseña = string.Empty;
                    }
                    _usuarioEnEdicion.Rol = (cmbRol.SelectedItem as ComboBoxItem)?.Content.ToString();

                    _usuarioService.Actualizar(_usuarioEnEdicion, SesionActual.NombreUsuario);
                    MessageBox.Show("Usuario actualizado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                LimpiarFormulario();
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MessageBox.Show("Por favor, ingrese un Nombre de Usuario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (cmbRol.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione un Rol para el usuario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_usuariosOriginales == null) return;

            string filtro = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(filtro))
            {
                dgvUsuarios.ItemsSource = _usuariosOriginales;
            }
            else
            {
                var filtrados = _usuariosOriginales.Where(u =>
                    u.NombreUsuario.ToLower().Contains(filtro) ||
                    u.Rol.ToLower().Contains(filtro)
                ).ToList();

                dgvUsuarios.ItemsSource = filtrados;
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.DataContext is Usuario usuarioFila)
            {
                _usuarioEnEdicion = new Usuario 
                {
                    Id = usuarioFila.Id, 
                    NombreUsuario = usuarioFila.NombreUsuario, 
                    Rol = usuarioFila.Rol 
                    // No copiamos el hash de la contraseña a txtPassword.Password por seguridad y UI
                };

                txtNombreUsuario.Text = _usuarioEnEdicion.NombreUsuario;
                txtPassword.Password = string.Empty; // Siempre en blanco al editar, sólo se llena si quiere cambiarla

                // Mapear el combobox (si el rol guardado ya no existe en la lista, queda sin selección
                // en vez de mostrar por error la selección anterior que haya quedado en el combo)
                cmbRol.SelectedItem = null;
                foreach (ComboBoxItem item in cmbRol.Items)
                {
                    if (item.Content.ToString() == _usuarioEnEdicion.Rol)
                    {
                        cmbRol.SelectedItem = item;
                        break;
                    }
                }

                txtTituloFormulario.Text = $"Editando usuario: {_usuarioEnEdicion.NombreUsuario}";
                btnGuardar.Content = "Guardar Cambios";
                scrollContenido.ScrollToTop();
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
             var btn = sender as Button;
             if (btn?.DataContext is Usuario usuarioFila)
             {
                 var resultado = MessageBox.Show($"¿Está seguro que desea eliminar a {usuarioFila.NombreUsuario}?\nEsta acción es irreversible.", 
                                                 "Confirmar Eliminación", 
                                                 MessageBoxButton.YesNo, 
                                                 MessageBoxImage.Warning);

                 if (resultado == MessageBoxResult.Yes)
                 {
                     try
                     {
                         _usuarioService.Eliminar(usuarioFila.Id, SesionActual.Usuario?.Id ?? 0, SesionActual.NombreUsuario);
                         MessageBox.Show("Usuario eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                         LimpiarFormulario();
                         CargarUsuarios();
                     }
                     catch (Exception ex)
                     {
                         MessageBox.Show($"Error al eliminar el usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                     }
                 }
             }
        }
    }
}
