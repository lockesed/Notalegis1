using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class EscriturasView : Window
    {
        private readonly EscrituraService escrituraService = new EscrituraService();
        private readonly ClienteDAO clienteDAO = new ClienteDAO();
        private readonly AnthropicService _ia = new AnthropicService();
        private readonly DocumentoService documentoService = new DocumentoService();
        private Escritura escrituraSeleccionada;
        private string archivoAdjuntoTemporal; // Ruta temporal del archivo seleccionado
        private string _plantillaTemporalRuta; // Ruta de la plantilla adjunta para IA

        public EscriturasView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarClientes();
            CargarTiposEscritura();
            CargarEscrituras();
            dpFecha.SelectedDate = DateTime.Today;
        }

        private void CargarClientes()
        {
            try
            {
                var clientes = clienteDAO.ObtenerTodos();
                cmbCliente.ItemsSource = clientes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarTiposEscritura()
        {
            try
            {
                var tipos = escrituraService.ObtenerTipos();
                cmbTipoEscritura.ItemsSource = tipos;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar tipos de escritura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarEscrituras()
        {
            try
            {
                var escrituras = escrituraService.ObtenerTodas();
                dgEscrituras.ItemsSource = escrituras;
                borderSinEscrituras.Visibility = escrituras.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar escrituras: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones
                if (cmbCliente.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbTipoEscritura.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un tipo de escritura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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

                // Validar que el número de protocolo no exista
                if (ValidarNumeroProtocoloExistente(txtNumeroProtocolo.Text.Trim()))
                {
                    MessageBox.Show($"Ya existe una escritura con el número de protocolo '{txtNumeroProtocolo.Text.Trim()}'.\n\nPor favor, ingrese un número diferente.", 
                        "Número de protocolo duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNumeroProtocolo.Focus();
                    txtNumeroProtocolo.SelectAll();
                    return;
                }

                // Crear nueva escritura (siempre en Borrador)
                var escritura = new Escritura();
                escritura.ClienteId = (int)cmbCliente.SelectedValue;
                escritura.TipoEscrituraId = (int)cmbTipoEscritura.SelectedValue;
                escritura.NumeroProtocolo = txtNumeroProtocolo.Text.Trim();
                escritura.Fecha = dpFecha.SelectedDate.Value;
                escritura.Estado = "Borrador"; // Siempre en Borrador al crear
                escritura.Observaciones = txtObservaciones.Text.Trim();
                escritura.FechaCreacion = DateTime.Now;
                escritura.UsuarioCreacion = SesionActual.NombreUsuario;

                // Si hay archivo adjunto temporal, procesarlo
                if (!string.IsNullOrEmpty(archivoAdjuntoTemporal))
                {
                    // Usar DocumentoService para guardar el archivo
                    var documentoService = new DocumentoService();
                    string nombreCarpeta = $"{escritura.NumeroProtocolo}".Replace("/", "-");

                    // Guardar el archivo y obtener la ruta final (con timestamp si fue renombrado)
                    string rutaFinal = documentoService.GuardarArchivo(archivoAdjuntoTemporal, "Escrituras", nombreCarpeta);

                    escritura.RutaArchivo = rutaFinal;
                    escritura.FormatoArchivo = System.IO.Path.GetExtension(archivoAdjuntoTemporal);
                }

                escrituraService.Guardar(escritura, SesionActual.NombreUsuario);

                MessageBox.Show("Escritura guardada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                LimpiarFormulario();
                CargarEscrituras();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar escritura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdjuntarPlantilla_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Documentos (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt|Todos (*.*)|*.*",
                Title = "Seleccionar plantilla para la IA"
            };

            if (dialog.ShowDialog() == true)
            {
                _plantillaTemporalRuta = dialog.FileName;
                txtPlantillaAdjunta.Text = Path.GetFileName(dialog.FileName);
                borderPlantillaAdjunta.Visibility = Visibility.Visible;
                btnGenerarConIA.IsEnabled = true;
            }
        }

        private void btnQuitarPlantilla_Click(object sender, RoutedEventArgs e)
        {
            _plantillaTemporalRuta = null;
            borderPlantillaAdjunta.Visibility = Visibility.Collapsed;
            btnGenerarConIA.IsEnabled = false;
        }

        private void btnVerPlantilla_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_plantillaTemporalRuta) && File.Exists(_plantillaTemporalRuta))
            {
                var previsualizador = new PrevisualizadorDocumentoWindow(_plantillaTemporalRuta) { Owner = this };
                previsualizador.ShowDialog();
            }
        }

        private async void btnGenerarConIA_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // El único requisito para generar el borrador con IA es tener una plantilla adjunta.
                // Cliente y Tipo de escritura son opcionales acá: si no se eligieron todavía, se piden
                // recién dentro del generador antes de guardar la escritura.
                if (string.IsNullOrWhiteSpace(_plantillaTemporalRuta))
                {
                    MessageBox.Show("Adjunte una plantilla primero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var cliente = cmbCliente.SelectedItem as Cliente;
                var tipo = cmbTipoEscritura.SelectedItem as TipoEscritura;

                btnGenerarConIA.IsEnabled = false;

                // Mostrar ventana de generación
                string nacionalidad   = (cmbNacionalidad.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                string estadoCivil   = (cmbEstadoCivil.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                string nombreConyuge = txtNombreConyuge.Visibility == Visibility.Visible ? txtNombreConyuge.Text.Trim() : "";
                DateTime? fechaNac   = dpFechaNacimiento.SelectedDate;
                string cuilCuit      = txtCuilCuit.Text.Trim();

                var ventanaGenerador = new GeneradorEscrituraIAWindow(
                    cliente, tipo, _plantillaTemporalRuta,
                    nacionalidad, estadoCivil, nombreConyuge, fechaNac, cuilCuit)
                {
                    Owner = this
                };

                if (ventanaGenerador.ShowDialog() == true)
                {
                    CargarEscrituras();
                }

                btnGenerarConIA.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnGenerarConIA.IsEnabled = true;
            }
        }

        private void btnAdjuntarArchivo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validaciones
                if (cmbCliente.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbTipoEscritura.SelectedValue == null)
                {
                    MessageBox.Show("Debe seleccionar un tipo de escritura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNumeroProtocolo.Text))
                {
                    MessageBox.Show("Debe ingresar el número de protocolo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Abrir diálogo de archivo
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Documentos Word (*.docx)|*.docx|Documentos PDF (*.pdf)|*.pdf|Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                    Title = "Seleccionar archivo de escritura"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Guardar ruta temporal (NO copiar ni guardar aún)
                    archivoAdjuntoTemporal = openFileDialog.FileName;

                    // Mostrar indicador visual
                    txtArchivoSeleccionado.Text = System.IO.Path.GetFileName(archivoAdjuntoTemporal);
                    borderArchivoSeleccionado.Visibility = Visibility.Visible;

                    MessageBox.Show("Archivo seleccionado. Presione GUARDAR para confirmar.", "Archivo listo", MessageBoxButton.OK, MessageBoxImage.Information);
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
                if (escrituraSeleccionada == null)
                {
                    MessageBox.Show("Debe seleccionar una escritura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string ruta = ObtenerRutaDocumentoParaVer(escrituraSeleccionada);
                if (ruta == null)
                {
                    MessageBox.Show("Esta escritura no tiene un archivo asociado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                escrituraService.AbrirArchivo(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (escrituraSeleccionada == null)
                {
                    MessageBox.Show("Debe seleccionar una escritura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Abrir ventana de edición
                int idEditada = escrituraSeleccionada.Id;
                var ventanaEdicion = new EditarEscrituraWindow(escrituraSeleccionada);
                if (ventanaEdicion.ShowDialog() == true)
                {
                    // Si se guardó, recargar la lista y volver a seleccionar la misma escritura
                    // (CargarEscrituras reemplaza el ItemsSource, lo que pierde la selección y
                    // deshabilita "Ver Documento" aunque se haya adjuntado un archivo recién)
                    CargarEscrituras();
                    foreach (var item in dgEscrituras.Items)
                    {
                        if (item is Escritura esc && esc.Id == idEditada)
                        {
                            dgEscrituras.SelectedItem = esc;
                            break;
                        }
                    }
                    MessageBox.Show("Escritura actualizada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar escritura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (escrituraSeleccionada == null)
                {
                    MessageBox.Show("Debe seleccionar una escritura.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"¿Está seguro de eliminar la escritura N° {escrituraSeleccionada.NumeroProtocolo}?\n\nNota: El archivo físico no será eliminado.",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    escrituraService.Eliminar(escrituraSeleccionada.Id, SesionActual.NombreUsuario);
                    MessageBox.Show("Escritura eliminada exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    LimpiarFormulario();
                    CargarEscrituras();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar escritura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDocumentos_Click(object sender, RoutedEventArgs e)
        {
            if (escrituraSeleccionada != null)
            {
                string nombreCarpeta = $"{escrituraSeleccionada.NumeroProtocolo}".Replace("/", "-");
                var ventana = new GestionDocumentosWindow("Escrituras", nombreCarpeta, $"Protocolo {escrituraSeleccionada.NumeroProtocolo}");
                ventana.ShowDialog();

                // Puede haberse subido o eliminado un adjunto: recalcular si "Ver documento" corresponde habilitarlo.
                btnVerDocumento.IsEnabled = ObtenerRutaDocumentoParaVer(escrituraSeleccionada) != null;
            }
            else
            {
                MessageBox.Show("Seleccione una escritura para ver sus documentos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            escrituraSeleccionada = null;
            cmbCliente.SelectedIndex = -1;
            cmbTipoEscritura.SelectedIndex = -1;
            txtNumeroProtocolo.Clear();
            dpFecha.SelectedDate = DateTime.Today;
            txtObservaciones.Clear();

            // Limpiar archivo adjunto temporal
            archivoAdjuntoTemporal = null;
            borderArchivoSeleccionado.Visibility = Visibility.Collapsed;
            txtArchivoSeleccionado.Text = "";

            // Limpiar campos adicionales IA
            cmbNacionalidad.SelectedIndex = -1;
            cmbEstadoCivil.SelectedIndex = -1;
            txtNombreConyuge.Clear();
            txtNombreConyuge.Visibility = Visibility.Collapsed;
            dpFechaNacimiento.SelectedDate = null;
            txtCuilCuit.Clear();

            // Limpiar plantilla IA
            _plantillaTemporalRuta = null;
            borderPlantillaAdjunta.Visibility = Visibility.Collapsed;
            btnGenerarConIA.IsEnabled = false;
        }

        private void dgEscrituras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            escrituraSeleccionada = dgEscrituras.SelectedItem as Escritura;
            btnVerDocumento.IsEnabled = ObtenerRutaDocumentoParaVer(escrituraSeleccionada) != null;
        }

        // El archivo principal de la escritura se puede cargar de dos formas independientes:
        // adjuntándolo en el formulario de Nueva/Editar escritura (queda en Escritura.RutaArchivo),
        // o subiéndolo suelto desde "Documentos adjuntos" (queda en la carpeta de DocumentoService,
        // sin relación con RutaArchivo). "Ver documento"/"Abrir archivo" deben poder mostrar
        // cualquiera de los dos, priorizando el archivo principal si existe.
        private string ObtenerRutaDocumentoParaVer(Escritura escritura)
        {
            if (escritura == null) return null;

            if (!string.IsNullOrEmpty(escritura.RutaArchivo) && System.IO.File.Exists(escritura.RutaArchivo))
                return escritura.RutaArchivo;

            string nombreCarpeta = $"{escritura.NumeroProtocolo}".Replace("/", "-");
            var adjuntos = documentoService.ListarArchivos("Escrituras", nombreCarpeta);
            return adjuntos.Count > 0 ? adjuntos[0].FullName : null;
        }

        private void btnVerDocumento_Click(object sender, RoutedEventArgs e)
        {
            string ruta = ObtenerRutaDocumentoParaVer(escrituraSeleccionada);
            if (ruta == null)
                return;

            var previsualizador = new PrevisualizadorDocumentoWindow(ruta)
            {
                Owner = this
            };
            previsualizador.ShowDialog();
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string filtro = txtBuscar.Text.Trim();
                var escrituras = escrituraService.Buscar(filtro);
                dgEscrituras.ItemsSource = escrituras;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarEscrituras();
            txtBuscar.Clear();
        }

        private void btnGestionarTipos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ventana = new GestionTiposEscrituraWindow();
                ventana.ShowDialog();
                CargarTiposEscritura();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir gestión de tipos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void btnEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new EstadisticasView { Owner = this };
            ventana.ShowDialog();
        }

        private void btnQuitarArchivo_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar archivo adjunto temporal
            archivoAdjuntoTemporal = null;
            borderArchivoSeleccionado.Visibility = Visibility.Collapsed;
            txtArchivoSeleccionado.Text = "";
        }

        private bool ValidarNumeroProtocoloExistente(string numeroProtocolo)
        {
            try
            {
                return escrituraService.ExisteNumeroProtocolo(numeroProtocolo);
            }
            catch
            {
                return false; // En caso de error, permitir continuar
            }
        }

        private void cmbEstadoCivil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtNombreConyuge == null) return;
            string seleccion = (cmbEstadoCivil.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            bool mostrar = seleccion is "Casado/a en primeras nupcias" or "Casado/a en segundas nupcias";
            txtNombreConyuge.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
            if (!mostrar) txtNombreConyuge.Clear();
        }

    }
}
