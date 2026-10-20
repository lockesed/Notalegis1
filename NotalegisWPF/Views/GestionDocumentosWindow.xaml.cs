using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class GestionDocumentosWindow : Window
    {
        private readonly DocumentoService _documentoService;
        private readonly string _tipoEntidad;
        private readonly string _nombreCarpeta; // e.g., "1_JuanPerez"

        public GestionDocumentosWindow(string tipoEntidad, string nombreCarpeta, string tituloEntidad)
        {
            InitializeComponent();
            _documentoService = new DocumentoService();
            _tipoEntidad = tipoEntidad;
            _nombreCarpeta = nombreCarpeta;

            txtTitulo.Text = $"DOCUMENTOS: {tituloEntidad.ToUpper()}";
            CargarArchivos();
        }

        private void CargarArchivos()
        {
            try
            {
                var archivos = _documentoService.ListarArchivos(_tipoEntidad, _nombreCarpeta);
                dgArchivos.ItemsSource = archivos;
                borderSinDocumentos.Visibility = archivos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                txtEstado.Text = $"{archivos.Count} archivos encontrados.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar archivos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSubir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Title = "Seleccionar documentos para subir"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        _documentoService.GuardarArchivo(filename, _tipoEntidad, _nombreCarpeta);
                    }
                    CargarArchivos();
                    MessageBox.Show("Archivos subidos exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al subir archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAbrirCarpeta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _documentoService.AbrirCarpeta(_tipoEntidad, _nombreCarpeta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir carpeta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarArchivos();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAbrirArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var archivo = ((Button)sender).DataContext as FileInfo;
                if (archivo != null)
                {
                    _documentoService.AbrirArchivo(archivo.FullName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminarArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var archivo = ((Button)sender).DataContext as FileInfo;
                if (archivo != null)
                {
                    var result = MessageBox.Show($"¿Está seguro de eliminar '{archivo.Name}'?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _documentoService.EliminarArchivo(archivo.FullName);
                        CargarArchivos();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
