using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;
using Notalegis.Services;

namespace Notalegis.Views
{
    public partial class GeneradorEscrituraIAWindow : Window
    {
        private readonly string _rutaPlantilla;
        private readonly string _nacionalidad;
        private readonly string _estadoCivil;
        private readonly string _nombreConyuge;
        private readonly DateTime? _fechaNacimiento;
        private readonly string _cuilCuit;
        private readonly AnthropicService _ia = new AnthropicService();
        private readonly EscrituraService _escrituraService = new EscrituraService();

        public GeneradorEscrituraIAWindow(
            Cliente cliente, TipoEscritura tipo, string rutaPlantilla,
            string nacionalidad = "", string estadoCivil = "", string nombreConyuge = "",
            DateTime? fechaNacimiento = null, string cuilCuit = "")
        {
            InitializeComponent();
            _rutaPlantilla = rutaPlantilla;
            _nacionalidad = nacionalidad;
            _estadoCivil = estadoCivil;
            _nombreConyuge = nombreConyuge;
            _fechaNacimiento = fechaNacimiento;
            _cuilCuit = cuilCuit;

            // Cliente y Tipo de escritura son opcionales acá: solo hace falta la plantilla para generar
            // el borrador. Si ya venían elegidos desde Escrituras, quedan precargados; si no, el escribano
            // los elige recién antes de guardar (igual que ya pasa con el protocolo y la fecha).
            var clientes = new ClienteDAO().ObtenerTodos();
            cmbCliente.ItemsSource = clientes;
            if (cliente != null)
                cmbCliente.SelectedItem = clientes.FirstOrDefault(c => c.Id == cliente.Id);

            var tipos = new TipoEscrituraDAO().ObtenerTodos();
            cmbTipoEscritura.ItemsSource = tipos;
            if (tipo != null)
                cmbTipoEscritura.SelectedItem = tipos.FirstOrDefault(t => t.Id == tipo.Id);

            dpFecha.SelectedDate = DateTime.Today;
        }

        private async void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            borderLoading.Visibility = Visibility.Visible;
            btnGenerar.IsEnabled = false;
            txtEstado.Text = "Generando con IA...";

            try
            {
                var clienteSeleccionado = cmbCliente.SelectedItem as Cliente;
                var tipoSeleccionado = cmbTipoEscritura.SelectedItem as TipoEscritura;

                var rutasPlantilla = new List<string> { _rutaPlantilla };
                string texto = await _ia.GenerarEscritura(
                    tipoSeleccionado?.Nombre ?? "", clienteSeleccionado, txtInstrucciones.Text, rutasPlantilla,
                    _nacionalidad, _estadoCivil, _nombreConyuge, _fechaNacimiento, _cuilCuit);

                txtBorrador.Text = texto;
                txtEstado.Text = "Generado correctamente. Podés editarlo, exportar a Word o guardar.";
                btnGuardar.IsEnabled = true;
                btnExportarWord.IsEnabled = true;
            }
            catch (Exception ex)
            {
                txtEstado.Text = "Error al generar";
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                borderLoading.Visibility = Visibility.Collapsed;
                btnGenerar.IsEnabled = true;
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtBorrador.Text))
                    return;

                if (cmbCliente.SelectedItem is not Cliente clienteSeleccionado)
                {
                    MessageBox.Show("Seleccioná el cliente antes de guardar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbTipoEscritura.SelectedItem is not TipoEscritura tipoSeleccionado)
                {
                    MessageBox.Show("Seleccioná el tipo de escritura antes de guardar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtProtocolo.Text))
                {
                    MessageBox.Show("Ingresá el número de protocolo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!dpFecha.SelectedDate.HasValue)
                {
                    MessageBox.Show("Seleccioná la fecha.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string protocolo = txtProtocolo.Text.Trim();
                if (_escrituraService.ExisteNumeroProtocolo(protocolo))
                {
                    MessageBox.Show($"Ya existe una escritura con el número de protocolo '{protocolo}'.\n\nPor favor, ingresá un número diferente.",
                        "Número de protocolo duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var docService = new DocumentoService();
                string carpeta = protocolo.Replace("/", "-").Replace("\\", "-");
                string rutaCarpeta = docService.ObtenerRutaCarpeta("Escrituras", carpeta);
                string nombreArchivo = $"ESC_IA_{carpeta}_{DateTime.Now:yyyy-MM-dd}.docx";
                string rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);

                WordExporter.Exportar(rutaArchivo, txtBorrador.Text);

                var escritura = new Escritura
                {
                    ClienteId = clienteSeleccionado.Id,
                    TipoEscrituraId = tipoSeleccionado.Id,
                    NumeroProtocolo = protocolo,
                    Fecha = dpFecha.SelectedDate.Value,
                    Estado = "Borrador",
                    RutaArchivo = rutaArchivo,
                    FormatoArchivo = ".docx",
                    Observaciones = "Generada con IA (desde plantilla)",
                    FechaCreacion = DateTime.Now,
                    UsuarioCreacion = SesionActual.NombreUsuario
                };

                _escrituraService.Guardar(escritura, SesionActual.NombreUsuario);

                if (MessageBox.Show("Escritura guardada. ¿Abrir el documento?", "Éxito",
                    MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    _escrituraService.AbrirArchivo(rutaArchivo);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportarWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBorrador.Text))
            {
                MessageBox.Show("No hay contenido para exportar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Documento Word (*.docx)|*.docx",
                FileName = $"Escritura_{DateTime.Now:yyyy-MM-dd}.docx"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    WordExporter.Exportar(dialog.FileName, txtBorrador.Text);
                    MessageBox.Show("Exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    _escrituraService.AbrirArchivo(dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txtBorrador_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            btnGuardar.IsEnabled = !string.IsNullOrWhiteSpace(txtBorrador.Text);
            btnExportarWord.IsEnabled = !string.IsNullOrWhiteSpace(txtBorrador.Text);
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
