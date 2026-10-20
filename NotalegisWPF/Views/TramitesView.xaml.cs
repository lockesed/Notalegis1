using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class TramitesView : Window
    {

        private void txtTramitePersonalizado_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                string texto = textBox.Text.Trim();
                bool esValido = !string.IsNullOrWhiteSpace(texto) && texto.Length >= 3;

                // Mostrar/ocultar mensaje de validación
                if (string.IsNullOrWhiteSpace(texto))
                {
                    lblTramiteValidacion.Visibility = Visibility.Collapsed;
                    textBox.BorderBrush = null;
                }
                else if (texto.Length < 3)
                {
                    lblTramiteValidacion.Visibility = Visibility.Visible;
                    textBox.BorderBrush = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    lblTramiteValidacion.Visibility = Visibility.Collapsed;
                    textBox.BorderBrush = System.Windows.Media.Brushes.Green;
                }

                // Si hay texto en el trámite personalizado, limpiar la selección del ComboBox
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    cmbTipoTramite.SelectedIndex = -1;
                }
            }
        }

        private void cmbTipoTramite_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Si se selecciona algo en el ComboBox, limpiar el trámite personalizado
            if (cmbTipoTramite.SelectedIndex != -1)
            {
                txtTramitePersonalizado.Text = "";
                lblTramiteValidacion.Visibility = Visibility.Collapsed;
                txtTramitePersonalizado.BorderBrush = null;
            }
        }

        private List<Tramite> tramites;
        private Tramite tramiteSeleccionado;
        private TramiteDAO tramiteDAO = new TramiteDAO();
        private ConfiguracionService confService = new ConfiguracionService();
        private readonly AuditoriaService _auditoria = new AuditoriaService();
        private List<Tramite> _todosTramites = new List<Tramite>();
        private List<Tramite> _tramitesFiltrados = new List<Tramite>();
        private int _paginaTramitesActual = 1;
        private const int TamanoPaginaTramites = 50;

        public TramitesView()
        {
            InitializeComponent();
            
            InicializarDatos();
            CargarTramites();
        }

        private void InicializarDatos()
        {
            LimpiarFormulario();
            CargarClientesComboBox();
        }

        private void CargarClientesComboBox()
        {
            var clienteDAO = new ClienteDAO();
            var clientes = clienteDAO.ObtenerTodos();
            cmbCliente.Items.Clear();
            foreach (var cliente in clientes)
            {
                var item = new ComboBoxItem();
                item.Content = $"{cliente.Nombre} {cliente.Apellido} - DNI: {cliente.DNI}";
                item.Tag = cliente;
                cmbCliente.Items.Add(item);
            }
        }

        private void CargarTramites()
        {
            _todosTramites = tramiteDAO.ObtenerTodos();
            tramites = _todosTramites;
            _paginaTramitesActual = 1;
            AplicarFiltroTramitesYPaginar();
        }

        private void AplicarFiltroTramitesYPaginar()
        {
            if (_todosTramites == null) return;

            string filtro = txtBuscar?.Text?.Trim() ?? "";
            string estado = (cmbFiltroEstado?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string prioridad = (cmbFiltroPrioridad?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string vencimiento = (cmbFiltroVencimiento?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";

            var lista = _todosTramites.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filtro))
                lista = lista.Where(x => (x.Cliente != null && x.Cliente.ToLower().Contains(filtro.ToLower())) || (x.TipoTramite != null && x.TipoTramite.ToLower().Contains(filtro.ToLower())));
            if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos los Estados")
                lista = lista.Where(x => x.Estado == estado);
            if (!string.IsNullOrWhiteSpace(prioridad) && prioridad != "Todas las Prioridades")
                lista = lista.Where(x => x.Prioridad == prioridad);
            if (!string.IsNullOrWhiteSpace(vencimiento) && vencimiento != "Todos")
            {
                DateTime hoy = DateTime.Today;
                if (vencimiento == "Vencen Hoy")
                    lista = lista.Where(x => x.FechaVencimiento.Date == hoy);
                else if (vencimiento == "Vencen Esta Semana")
                {
                    var end = hoy.AddDays(7);
                    lista = lista.Where(x => x.FechaVencimiento.Date >= hoy && x.FechaVencimiento.Date <= end);
                }
                else if (vencimiento == "Vencen Este Mes")
                {
                    var start = new DateTime(hoy.Year, hoy.Month, 1);
                    var end = start.AddMonths(1).AddDays(-1);
                    lista = lista.Where(x => x.FechaVencimiento.Date >= start && x.FechaVencimiento.Date <= end);
                }
                else if (vencimiento == "Ya Vencidos")
                    lista = lista.Where(x => x.FechaVencimiento.Date < hoy);
            }

            _tramitesFiltrados = lista.ToList();

            int totalPaginas = Math.Max(1, (int)Math.Ceiling(_tramitesFiltrados.Count / (double)TamanoPaginaTramites));
            if (_paginaTramitesActual > totalPaginas) _paginaTramitesActual = totalPaginas;

            var pagina = _tramitesFiltrados
                .Skip((_paginaTramitesActual - 1) * TamanoPaginaTramites)
                .Take(TamanoPaginaTramites)
                .ToList();

            dgTramites.ItemsSource = pagina;
            dgTramites.Items.Refresh();
            borderSinTramites.Visibility = _tramitesFiltrados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            txtInfoPaginaTramites.Text = $"Página {_paginaTramitesActual} de {totalPaginas}  ({_tramitesFiltrados.Count} registros)";
            btnTramiteAnterior.IsEnabled = _paginaTramitesActual > 1;
            btnTramiteSiguiente.IsEnabled = _paginaTramitesActual < totalPaginas;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            cmbCliente.SelectedIndex = -1;
            cmbTipoTramite.SelectedIndex = -1;
            dpFechaInicio.SelectedDate = null;
            dpFechaVencimiento.SelectedDate = null;
            cmbEstado.SelectedIndex = -1;
            cmbPrioridad.SelectedIndex = -1;
            txtObservaciones.Text = "";
            chkDocumentacionCompleta.IsChecked = false;
            txtTramitePersonalizado.Text = "";
            lblTramiteValidacion.Visibility = Visibility.Collapsed;
            tramiteSeleccionado = null;
            dgTramites.SelectedItem = null;
        }

        private string ObtenerTipoTramiteSeleccionado()
        {
            // Priorizar trámite personalizado si tiene texto válido
            if (!string.IsNullOrWhiteSpace(txtTramitePersonalizado.Text) && txtTramitePersonalizado.Text.Trim().Length >= 3)
            {
                return txtTramitePersonalizado.Text.Trim();
            }
            
            // Si no hay trámite personalizado, usar el ComboBox
            if (cmbTipoTramite.SelectedItem != null)
            {
                return ((ComboBoxItem)cmbTipoTramite.SelectedItem).Content.ToString();
            }
            
            return null;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar que se haya seleccionado un tipo de trámite (ComboBox o personalizado)
            string tipoTramite = ObtenerTipoTramiteSeleccionado();
            
            if (cmbCliente.SelectedItem == null || string.IsNullOrWhiteSpace(tipoTramite) || dpFechaInicio.SelectedDate == null || dpFechaVencimiento.SelectedDate == null || cmbEstado.SelectedItem == null || cmbPrioridad.SelectedItem == null)
            {
                MessageBox.Show("Complete todos los campos obligatorios. Debe seleccionar un tipo de trámite del ComboBox o ingresar uno personalizado.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string cliente = ((ComboBoxItem)cmbCliente.SelectedItem).Content.ToString();
            DateTime fechaInicio = dpFechaInicio.SelectedDate.Value;
            DateTime fechaVencimiento = dpFechaVencimiento.SelectedDate.Value;

            if (fechaVencimiento <= fechaInicio)
            {
                MessageBox.Show("La fecha de vencimiento debe ser posterior a la fecha de inicio.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime fechaCreacion = DateTime.Now; // Usar fecha actual automáticamente
            string estado = ((ComboBoxItem)cmbEstado.SelectedItem).Content.ToString();
            string prioridad = ((ComboBoxItem)cmbPrioridad.SelectedItem).Content.ToString();
            string observaciones = txtObservaciones.Text;
            bool docCompleta = chkDocumentacionCompleta.IsChecked == true;

            // Este formulario ("Nuevo Trámite") siempre crea un trámite nuevo.
            // La edición de un trámite existente se hace exclusivamente desde
            // el botón "Editar" (EditarTramiteWindow), para no pisar por accidente
            // el trámite que haya quedado seleccionado en la grilla.
            Tramite nuevo = new Tramite
            {
                Cliente = cliente,
                TipoTramite = tipoTramite,
                FechaInicio = fechaInicio,
                FechaVencimiento = fechaVencimiento,
                FechaCreacion = fechaCreacion,
                Estado = estado,
                Prioridad = prioridad,
                Observaciones = observaciones,
                DocumentacionCompleta = docCompleta
            };
            tramiteDAO.Agregar(nuevo);
            _auditoria.Registrar(SesionActual.NombreUsuario, "ALTA", "Tramites", null, $"Trámite '{nuevo.TipoTramite}' para '{nuevo.Cliente}' creado");
            MessageBox.Show("El trámite fue guardado correctamente.", "Trámite guardado", MessageBoxButton.OK, MessageBoxImage.Information);

            CargarTramites();
            LimpiarFormulario();
        }

        private void dgTramites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramite)
            {
                tramiteSeleccionado = tramite;
                // No cargar datos en el formulario - solo permitir edición desde modal
            }
        }

        private void CargarTramiteEnFormulario(Tramite tramite)
        {
            // Cargar cliente
            cmbCliente.SelectedItem = cmbCliente.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == tramite.Cliente);
            
            // Cargar tipo de trámite - verificar si está en el ComboBox
            var tipoTramiteItem = cmbTipoTramite.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == tramite.TipoTramite);
            if (tipoTramiteItem != null)
            {
                // Es un tipo estándar, seleccionarlo en el ComboBox
                cmbTipoTramite.SelectedItem = tipoTramiteItem;
                txtTramitePersonalizado.Text = "";
                lblTramiteValidacion.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Es un tipo personalizado, ponerlo en el TextBox
                cmbTipoTramite.SelectedIndex = -1;
                txtTramitePersonalizado.Text = tramite.TipoTramite;
                txtTramitePersonalizado.BorderBrush = System.Windows.Media.Brushes.Green;
                lblTramiteValidacion.Visibility = Visibility.Collapsed;
            }
            
            // Cargar otros campos
            dpFechaInicio.SelectedDate = tramite.FechaInicio;
            dpFechaVencimiento.SelectedDate = tramite.FechaVencimiento;
            cmbEstado.SelectedItem = cmbEstado.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == tramite.Estado);
            cmbPrioridad.SelectedItem = cmbPrioridad.Items.Cast<ComboBoxItem>().FirstOrDefault(x => x.Content.ToString() == tramite.Prioridad);
            txtObservaciones.Text = tramite.Observaciones;
            chkDocumentacionCompleta.IsChecked = tramite.DocumentacionCompleta;
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramite)
            {
                var ventana = new EditarTramiteWindow(tramite);
                if (ventana.ShowDialog() == true && ventana.TramiteEditado != null)
                {
                    tramiteDAO.Editar(ventana.TramiteEditado);
                    _auditoria.Registrar(SesionActual.NombreUsuario, "MODIFICACION", "Tramites", ventana.TramiteEditado.Id, $"Trámite '{ventana.TramiteEditado.TipoTramite}' del cliente '{ventana.TramiteEditado.Cliente}' editado");
                    CargarTramites();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para editar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramite)
            {
                if (MessageBox.Show("¿Está seguro de eliminar el trámite?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    tramiteDAO.Eliminar(tramite.Id);
                    string expTramite = string.IsNullOrWhiteSpace(tramite.NumeroExpediente) ? "" : $", exp: {tramite.NumeroExpediente}";
                    _auditoria.Registrar(SesionActual.NombreUsuario, "BAJA", "Tramites", tramite.Id, $"Trámite '{tramite.TipoTramite}' del cliente '{tramite.Cliente}'{expTramite} eliminado");
                    CargarTramites();
                    LimpiarFormulario();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramite)
            {
                var ventana = new ConsultarTramiteWindow(tramite);
                ventana.ShowDialog();
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para consultar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnSeguimiento_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramite)
            {
                MessageBox.Show($"Seguimiento del trámite de {tramite.Cliente}:\nEstado: {tramite.Estado}", "Seguimiento", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para ver seguimiento.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDocumentos_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramite)
            {
                string nombreCarpeta = $"{tramite.Id}_{tramite.TipoTramite}".Replace(" ", "");
                var ventana = new GestionDocumentosWindow("Tramites", nombreCarpeta, $"Trámite {tramite.Id} - {tramite.TipoTramite}");
                ventana.ShowDialog();
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para ver sus documentos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void cmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            _paginaTramitesActual = 1;
            AplicarFiltroTramitesYPaginar();
        }

        private void btnListadoDiario_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden exportar el listado de trámites.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var lista = tramiteDAO.ObtenerTodos();
                if (lista.Count == 0)
                {
                    MessageBox.Show("No hay trámites para exportar.", "Sin Datos", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var todasColumnas = new[] { "ID", "Cliente", "Tipo de Trámite", "Fecha Inicio", "Fecha Vencimiento", "N° Expediente", "Fecha Creación", "Monto Estimado", "Estado", "Prioridad", "Observaciones", "Doc. Completa" };
                var selectorDialog = new SeleccionarColumnasDialog(todasColumnas) { Owner = this };
                if (selectorDialog.ShowDialog() != true) return;
                var colsSel = selectorDialog.ColumnasSeleccionadas;

                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    FileName = $"Listado_Tramites_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = "xlsx"
                };
                if (saveFileDialog.ShowDialog() != true) return;

                using var workbook = new XLWorkbook();
                var hoja = workbook.Worksheets.Add("Trámites");

                for (int col = 0; col < colsSel.Count; col++)
                    hoja.Cell(1, col + 1).Value = colsSel[col];

                var enc = hoja.Range(1, 1, 1, colsSel.Count);
                enc.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                enc.Style.Font.FontColor = XLColor.White;
                enc.Style.Font.Bold = true;
                enc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                var mapaValor = new System.Collections.Generic.Dictionary<string, System.Func<Tramite, object>>
                {
                    ["ID"]               = t => t.Id,
                    ["Cliente"]          = t => t.Cliente ?? "",
                    ["Tipo de Trámite"]  = t => t.TipoTramite ?? "",
                    ["Fecha Inicio"]     = t => t.FechaInicio.ToString("dd/MM/yyyy"),
                    ["Fecha Vencimiento"]= t => t.FechaVencimiento.ToString("dd/MM/yyyy"),
                    ["N° Expediente"]    = t => t.NumeroExpediente ?? "",
                    ["Fecha Creación"]   = t => t.FechaCreacion.ToString("dd/MM/yyyy"),
                    ["Monto Estimado"]   = t => t.MontoEstimado?.ToString("F2") ?? "",
                    ["Estado"]           = t => t.Estado ?? "",
                    ["Prioridad"]        = t => t.Prioridad ?? "",
                    ["Observaciones"]    = t => t.Observaciones ?? "",
                    ["Doc. Completa"]    = t => t.DocumentacionCompleta ? "Sí" : "No"
                };

                for (int i = 0; i < lista.Count; i++)
                {
                    int fila = i + 2;
                    for (int col = 0; col < colsSel.Count; col++)
                        if (mapaValor.TryGetValue(colsSel[col], out var getter))
                            hoja.Cell(fila, col + 1).Value = getter(lista[i])?.ToString() ?? "";

                    if (i % 2 == 1)
                        hoja.Row(fila).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F9");
                }

                hoja.Columns().AdjustToContents();
                hoja.PageSetup.Footer.Right.AddText($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                workbook.SaveAs(saveFileDialog.FileName);

                MessageBox.Show("Archivo Excel exportado exitosamente.", "Exportación Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                if (MessageBox.Show("¿Desea abrir el archivo exportado?", "Abrir Archivo", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = saveFileDialog.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar el archivo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAlertaVencimiento_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramiteSeleccionado)
            {
                EnviarAlertaFirma(tramiteSeleccionado);
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para enviar alerta de firma por WhatsApp.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EnviarAlertaFirma(Tramite tramite)
        {
            try
            {
                // Obtener el teléfono del cliente
                string telefono = ObtenerTelefonoCliente(tramite.Cliente);
                
                if (string.IsNullOrWhiteSpace(telefono))
                {
                    MessageBox.Show("No se encontró el número de teléfono del cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Limpiar el número de teléfono (solo números)
                string telefonoLimpio = LimpiarNumeroTelefono(telefono);
                
                if (string.IsNullOrWhiteSpace(telefonoLimpio))
                {
                    MessageBox.Show("El número de teléfono del cliente no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Crear mensaje de alerta de firma
                string mensaje = GenerarMensajeAlertaFirma(tramite);
                
                // Generar URL de WhatsApp Web
                string urlWhatsApp = GenerarUrlWhatsApp(telefonoLimpio, mensaje);
                
                // Abrir WhatsApp Web en el navegador predeterminado
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = urlWhatsApp,
                    UseShellExecute = true
                });

                MessageBox.Show($"Se abrió WhatsApp Web para enviar alerta de firma a {tramite.Cliente}", "Alerta de Firma", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir WhatsApp: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerarMensajeAlertaFirma(Tramite tramite)
        {
            try
            {
                string plantilla = confService.ObtenerValorString(ClavesConfiguracion.WhatsAppMensajeFirma, "");
                
                if (!string.IsNullOrEmpty(plantilla))
                {
                    string escribaniaNombre = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "Notalegis");
                    string escribaniaDireccion = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaDireccion, "nuestra oficina");
                    string nombreCliente = tramite.Cliente.Split('-')[0].Trim();
                    string fechaVencimiento = tramite.FechaVencimiento.ToString("dd/MM/yyyy");

                    return plantilla
                        .Replace("{0}", nombreCliente)
                        .Replace("{1}", tramite.TipoTramite)
                        .Replace("{2}", fechaVencimiento)
                        .Replace("{Escribania_Nombre}", escribaniaNombre)
                        .Replace("{Escribania_Direccion}", escribaniaDireccion);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error leyendo BD para mensaje firma: " + ex.Message);
            }

            // Fallback en caso de error o bd vacía
            var mensaje = $"Hola {tramite.Cliente.Split('-')[0].Trim()},\n\n" +
                         $"Le escribimos desde Escribania Ejemplo para informarle que su trámite está listo para la firma:\n\n" +
                         $"📋 *Tipo de Trámite:* {tramite.TipoTramite}\n" +
                         $"📄 *Número de Expediente:* {(string.IsNullOrWhiteSpace(tramite.NumeroExpediente) ? "Por asignar" : tramite.NumeroExpediente)}\n" +
                         $"📅 *Fecha de Vencimiento:* {tramite.FechaVencimiento:dd/MM/yyyy}\n" +
                         $"⚡ *Prioridad:* {tramite.Prioridad}\n\n" +
                         $"✍️ *IMPORTANTE: Su trámite requiere firma presencial*\n\n" +
                         $"Por favor, acérquese a nuestras oficinas para completar la firma de los documentos. " +
                         $"Le recomendamos coordinar una cita previa para agilizar el proceso.\n\n" +
                         $"📍 *Horarios de atención:*\n" +
                         $"• Lunes a Viernes: 9:00 - 18:00\n" +
                         $"• Sábados: 9:00 - 13:00\n\n" +
                         $"📞 Para coordinar su cita, puede responder este mensaje o llamarnos.\n\n" +
                         $"Gracias por confiar en nosotros.\n\n" +
                         $"Saludos cordiales,\n*Equipo Notalegis*";
            
            return mensaje;
        }

        private void btnVencimientosHoy_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VencimientosHoyWindow(DateTime.Today);
            ventana.Owner = this;
            ventana.ShowDialog();
        }

        private void btnActualizarLista_Click(object sender, RoutedEventArgs e)
        {
            CargarTramites();
        }

        private void btnEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new EstadisticasView { Owner = this };
            ventana.ShowDialog();
        }


        private void btnSeguimientoGeneral_Click(object sender, RoutedEventArgs e)
        {
            if (dgTramites.SelectedItem is Tramite tramiteSeleccionado)
            {
                EnviarMensajeWhatsApp(tramiteSeleccionado);
            }
            else
            {
                MessageBox.Show("Seleccione un trámite para enviar seguimiento por WhatsApp.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EnviarMensajeWhatsApp(Tramite tramite)
        {
            try
            {
                // Obtener el teléfono del cliente
                string telefono = ObtenerTelefonoCliente(tramite.Cliente);
                
                if (string.IsNullOrWhiteSpace(telefono))
                {
                    MessageBox.Show("No se encontró el número de teléfono del cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Limpiar el número de teléfono (solo números)
                string telefonoLimpio = LimpiarNumeroTelefono(telefono);
                
                if (string.IsNullOrWhiteSpace(telefonoLimpio))
                {
                    MessageBox.Show("El número de teléfono del cliente no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Crear mensaje genérico de seguimiento
                string mensaje = GenerarMensajeSeguimiento(tramite);
                
                // Generar URL de WhatsApp Web
                string urlWhatsApp = GenerarUrlWhatsApp(telefonoLimpio, mensaje);
                
                // Abrir WhatsApp Web en el navegador predeterminado
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = urlWhatsApp,
                    UseShellExecute = true
                });

                MessageBox.Show($"Se abrió WhatsApp Web para enviar mensaje a {tramite.Cliente}", "WhatsApp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir WhatsApp: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ObtenerTelefonoCliente(string nombreCliente)
        {
            try
            {
                var clienteDAO = new ClienteDAO();
                var clientes = clienteDAO.ObtenerTodos();
                
                // Buscar el cliente por nombre completo
                var cliente = clientes.FirstOrDefault(c => 
                    $"{c.Nombre} {c.Apellido} - DNI: {c.DNI}" == nombreCliente ||
                    $"{c.Nombre} {c.Apellido}" == nombreCliente ||
                    nombreCliente.Contains(c.DNI));
                
                return cliente?.Telefono;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string LimpiarNumeroTelefono(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return string.Empty;

            // Remover todos los caracteres que no sean números
            string numeroLimpio = new string(telefono.Where(char.IsDigit).ToArray());
            
            // Si el número empieza con 54 (código de Argentina), mantenerlo
            // Si empieza con 0, removerlo y agregar 54
            // Si no tiene código de país, agregar 549 (Argentina con 9 para celulares)
            if (numeroLimpio.StartsWith("54"))
            {
                return numeroLimpio;
            }
            else if (numeroLimpio.StartsWith("0"))
            {
                return "549" + numeroLimpio.Substring(1);
            }
            else if (numeroLimpio.Length >= 8)
            {
                return "549" + numeroLimpio;
            }
            
            return numeroLimpio;
        }

        private string GenerarMensajeSeguimiento(Tramite tramite)
        {
            string baseMsg = "";
            try
            {
                string plantilla = confService.ObtenerValorString(ClavesConfiguracion.WhatsAppMensajeSeguimiento, "");
                
                if (!string.IsNullOrEmpty(plantilla))
                {
                    string escribaniaNombre = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "Notalegis");
                    string nombreCliente = tramite.Cliente.Split('-')[0].Trim();

                    baseMsg = plantilla
                        .Replace("{0}", nombreCliente)
                        .Replace("{1}", tramite.TipoTramite)
                        .Replace("{2}", tramite.Estado)
                        .Replace("{Escribania_Nombre}", escribaniaNombre) + "\n\n";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error leyendo BD para mensaje seguimiento: " + ex.Message);
            }

            if (string.IsNullOrEmpty(baseMsg))
            {
                // Fallback si falla la BD
                baseMsg = $"Hola {tramite.Cliente.Split('-')[0].Trim()},\n\n" +
                          $"Le escribimos desde Notalegis para informarle sobre el estado de su trámite:\n\n" +
                          $"📋 *Tipo de Trámite:* {tramite.TipoTramite}\n" +
                          $"📊 *Estado Actual:* {tramite.Estado}\n" +
                          $"📅 *Fecha de Vencimiento:* {tramite.FechaVencimiento:dd/MM/yyyy}\n" +
                          $"⚡ *Prioridad:* {tramite.Prioridad}\n\n";
            }

            // Agregar información específica según el estado como info extendida
            switch (tramite.Estado?.ToLower())
            {
                case "pendiente":
                    baseMsg += "Su trámite está en cola de procesamiento. Pronto comenzaremos a trabajar en él.";
                    break;
                case "en proceso":
                    baseMsg += "Su trámite está siendo procesado por nuestro equipo.";
                    break;
                case "documentación incompleta":
                    baseMsg += "⚠️ Necesitamos documentación adicional para continuar con su trámite. Por favor, póngase en contacto con nosotros.";
                    break;
                case "esperando cliente":
                    baseMsg += "⏳ Estamos esperando su respuesta o acción para continuar con el trámite.";
                    break;
                case "finalizado":
                    baseMsg += "✅ ¡Su trámite ha sido completado exitosamente!";
                    break;
                default:
                    baseMsg += "Cualquier consulta, no dude en contactarnos.";
                    break;
            }

            string escribaniaFinal = confService.ObtenerValorString(ClavesConfiguracion.EscribaniaNombre, "Notalegis");
            baseMsg += $"\n\nSaludos cordiales,\n*Equipo {escribaniaFinal}*";
            
            return baseMsg;
        }

        private string GenerarUrlWhatsApp(string telefono, string mensaje)
        {
            // Codificar el mensaje para URL
            string mensajeCodificado = Uri.EscapeDataString(mensaje);

            // wa.me abre el chat con el mensaje precargado sin depender de que el navegador
            // ya tenga una sesión de WhatsApp Web iniciada (a diferencia de web.whatsapp.com/send,
            // que si no hay sesión activa muestra la pantalla de login/QR en lugar del chat).
            return $"https://wa.me/{telefono}?text={mensajeCodificado}";
        }

        private void btnTramiteAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaTramitesActual > 1)
            {
                _paginaTramitesActual--;
                AplicarFiltroTramitesYPaginar();
            }
        }

        private void btnTramiteSiguiente_Click(object sender, RoutedEventArgs e)
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(_tramitesFiltrados.Count / (double)TamanoPaginaTramites));
            if (_paginaTramitesActual < totalPaginas)
            {
                _paginaTramitesActual++;
                AplicarFiltroTramitesYPaginar();
            }
        }
    }

}
