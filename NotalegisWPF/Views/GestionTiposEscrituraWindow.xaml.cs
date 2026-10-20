using System;
using System.Windows;
using System.Windows.Controls;
using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class GestionTiposEscrituraWindow : Window
    {
        private readonly EscrituraService escrituraService = new EscrituraService();
        private TipoEscritura tipoSeleccionado; // resaltado en la grilla (para Editar/Eliminar)
        private TipoEscritura tipoEnEdicion;    // el que realmente se está editando en el formulario

        public GestionTiposEscrituraWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarTipos();
        }

        private void CargarTipos()
        {
            try
            {
                var tipos = escrituraService.ObtenerTipos();
                dgTipos.ItemsSource = tipos;
                borderSinTipos.Visibility = tipos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("Debe ingresar el nombre del tipo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var tipo = tipoEnEdicion ?? new TipoEscritura();
                tipo.Nombre = txtNombre.Text.Trim();
                tipo.Descripcion = txtDescripcion.Text.Trim();
                tipo.Activo = chkActivo.IsChecked == true;

                escrituraService.GuardarTipo(tipo);

                MessageBox.Show("Tipo guardado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                LimpiarFormulario();
                CargarTipos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar tipo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (tipoSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un tipo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            tipoEnEdicion = tipoSeleccionado;
            txtNombre.Text = tipoEnEdicion.Nombre;
            txtDescripcion.Text = tipoEnEdicion.Descripcion;
            chkActivo.IsChecked = tipoEnEdicion.Activo;
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tipoSeleccionado == null)
                {
                    MessageBox.Show("Debe seleccionar un tipo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"¿Está seguro de eliminar el tipo '{tipoSeleccionado.Nombre}'?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    escrituraService.EliminarTipo(tipoSeleccionado.Id);
                    MessageBox.Show("Tipo eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LimpiarFormulario();
                    CargarTipos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar tipo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            tipoSeleccionado = null;
            tipoEnEdicion = null;
            txtNombre.Clear();
            txtDescripcion.Clear();
            chkActivo.IsChecked = true;
            dgTipos.SelectedIndex = -1;
        }

        private void dgTipos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tipoSeleccionado = dgTipos.SelectedItem as TipoEscritura;
        }
    }
}
