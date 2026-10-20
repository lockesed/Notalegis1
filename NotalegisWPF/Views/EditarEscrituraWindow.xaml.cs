using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class EditarEscrituraWindow : Window
    {
        private readonly EscrituraService escrituraService = new EscrituraService();
        private Escritura escritura;
        private bool _guardado = false;

        public EditarEscrituraWindow(Escritura escrituraAEditar)
        {
            InitializeComponent();
            this.escritura = escrituraAEditar;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            try
            {
                // Recargar escritura completa desde BD para tener todos los datos
                escritura = escrituraService.ObtenerPorId(escritura.Id);

                if (escritura == null)
                {
                    MessageBox.Show("No se pudo cargar la escritura.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                // Título
                txtTitulo.Text = $"Protocolo N° {escritura.NumeroProtocolo}";

                // Datos del formulario
                txtCliente.Text = escritura.NombreCliente;
                txtTipoEscritura.Text = escritura.TipoEscritura;
                txtNumeroProtocolo.Text = escritura.NumeroProtocolo;
                dpFecha.SelectedDate = escritura.Fecha;
                txtObservaciones.Text = escritura.Observaciones;

                // Estado
                foreach (ComboBoxItem item in cmbEstado.Items)
                {
                    if (item.Content.ToString() == escritura.Estado)
                    {
                        cmbEstado.SelectedItem = item;
                        break;
                    }
                }

                // Información adicional
                txtRutaArchivo.Text = escritura.RutaArchivo ?? "Sin archivo";
                txtFechaCreacion.Text = escritura.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
                txtFechaProtocolizacion.Text = escritura.FechaProtocolizacion?.ToString("dd/MM/yyyy HH:mm") ?? "No protocolizada";

                // Habilitar/deshabilitar botón de abrir archivo
                btnAbrirArchivo.IsEnabled = !string.IsNullOrEmpty(escritura.RutaArchivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(txtNumeroProtocolo.Text))
                {
                    MessageBox.Show("Debe ingresar el número de protocolo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpFecha.SelectedDate.HasValue)
                {
                    MessageBox.Show("Debe seleccionar una fecha.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbEstado.SelectedItem == null)
                {
                    MessageBox.Show("Debe seleccionar un estado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string numeroProtocolo = txtNumeroProtocolo.Text.Trim();
                if (escrituraService.ExisteNumeroProtocolo(numeroProtocolo, escritura.Id))
                {
                    MessageBox.Show($"Ya existe otra escritura con el número de protocolo '{numeroProtocolo}'.\n\nPor favor, ingrese un número diferente.",
                        "Número de protocolo duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumeroProtocolo.Focus();
                    txtNumeroProtocolo.SelectAll();
                    return;
                }

                // Actualizar datos
                escritura.NumeroProtocolo = numeroProtocolo;
                escritura.Fecha = dpFecha.SelectedDate.Value;
                escritura.Estado = ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString();
                escritura.Observaciones = txtObservaciones.Text.Trim();

                // Si cambió a Protocolizada y no tenía fecha, se actualiza automáticamente por el trigger
                
                // Guardar
                escrituraService.Guardar(escritura, SesionActual.NombreUsuario);

                _guardado = true;
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cambios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void btnAdjuntarArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Abrir diálogo de archivo
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Documentos Word (*.docx)|*.docx|Documentos PDF (*.pdf)|*.pdf|Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                    Title = "Seleccionar archivo de escritura"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Obtener datos necesarios
                    var clienteDAO = new ClienteDAO();
                    var cliente = clienteDAO.ObtenerPorId(escritura.ClienteId);
                    var tipo = new TipoEscritura { Id = escritura.TipoEscrituraId, Nombre = escritura.TipoEscritura };

                    // Copiar archivo a la ubicación organizada
                    string rutaArchivo = escrituraService.CopiarArchivoAdjunto(openFileDialog.FileName, escritura, cliente, tipo);

                    // Actualizar escritura
                    escritura.RutaArchivo = rutaArchivo;
                    escritura.FormatoArchivo = System.IO.Path.GetExtension(openFileDialog.FileName);

                    // Actualizar en BD
                    escrituraService.Guardar(escritura, SesionActual.NombreUsuario);

                    // Actualizar interfaz
                    txtRutaArchivo.Text = rutaArchivo;

                    MessageBox.Show("Archivo adjuntado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Habilitar botón de abrir archivo
                    btnAbrirArchivo.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al adjuntar archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAbrirArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(escritura.RutaArchivo))
                {
                    MessageBox.Show("Esta escritura no tiene un archivo asociado.\n\nUse el botón 'ADJUNTAR ARCHIVO' para agregar uno.", 
                        "Sin archivo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!System.IO.File.Exists(escritura.RutaArchivo))
                {
                    MessageBox.Show($"El archivo no existe en la ruta:\n{escritura.RutaArchivo}\n\nPuede haber sido movido o eliminado.", 
                        "Archivo no encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                escrituraService.AbrirArchivo(escritura.RutaArchivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
