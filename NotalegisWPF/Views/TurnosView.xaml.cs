using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class TurnosView : Window
    {
        private TurnoDAO turnoDAO = new TurnoDAO();
        private ClienteDAO clienteDAO = new ClienteDAO();
        private readonly AuditoriaService _auditoria = new AuditoriaService();
        private Turno turnoSeleccionado;
        private List<Cliente> clientes;

        public TurnosView()
        {
            InitializeComponent();

            CargarClientes();  // PRIMERO: obtener los clientes
            CargarTramites();
            CargarEstados();

            CargarTurnos();
        }

        private void CargarClientes()
        {
            clientes = clienteDAO.ObtenerTodos();
            cmbCliente.ItemsSource = clientes;
            cmbCliente.DisplayMemberPath = "NombreCompleto";
        }

        private void CargarTurnos()
        {
            var turnos = turnoDAO.ObtenerTodos();

            var listaVista = turnos.Select(t =>
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == t.IdCliente);
                string nombreCliente = cliente == null
    ? "⚠ Cliente no encontrado"
    : $"{cliente.Nombre} {cliente.Apellido}";

                return new TurnoVista
                {
                    Id = t.Id,
                    Cliente = nombreCliente,
                    Fecha = t.FechaHora.ToShortDateString(),
                    Hora = t.FechaHora.ToString("HH:mm"),
                    TipoTramite = t.TipoTramite,
                    Estado = t.Estado,
                    Confirmado = t.Confirmado
                };
            }).ToList();

            dgTurnos.ItemsSource = listaVista;
            borderSinTurnos.Visibility = listaVista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LimpiarFormulario()
        {
            cmbCliente.SelectedIndex = -1;
            dpFecha.SelectedDate = DateTime.Today;
            tpHora.SelectedTime = null;
            cmbTramite.SelectedIndex = -1;
            cmbEstado.SelectedIndex = 0;
            txtObservaciones.Text = "";
            turnoSeleccionado = null;
            dgTurnos.SelectedItem = null; // Deseleccionar el turno del DataGrid
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filtro = txtBuscar.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(filtro))
            {
                CargarTurnos();
                return;
            }

            var turnos = turnoDAO.ObtenerTodos();
            var resultados = turnos.Where(t =>
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == t.IdCliente);
                string nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}".ToLower() : "";
                return nombreCompleto.Contains(filtro) || t.TipoTramite.ToLower().Contains(filtro);
            })
            .Select(t =>
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == t.IdCliente);
                string nombreCliente = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "⚠ Cliente no encontrado";

                return new TurnoVista
                {
                    Id = t.Id,
                    Cliente = nombreCliente,
                    Fecha = t.FechaHora.ToShortDateString(),
                    Hora = t.FechaHora.ToString("HH:mm"),
                    TipoTramite = t.TipoTramite,
                    Estado = t.Estado,
                    Confirmado = t.Confirmado
                };
            }).ToList();

            dgTurnos.ItemsSource = resultados;
            borderSinTurnos.Visibility = resultados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void FiltrarTurnos(string filtro)
        {
            filtro = filtro.ToLower();
            var textoBuscado = QuitarAcentos(txtBuscar.Text.Trim().ToLower());

            var turnos = turnoDAO.ObtenerTodos(); // 👈 asegurate que esto no sea null
            var listaFiltrada = turnos.Where(t =>
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == t.IdCliente);
                string nombreCompleto = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "";

                return QuitarAcentos(nombreCompleto).Contains(textoBuscado) ||
                       QuitarAcentos(t.TipoTramite).Contains(textoBuscado);
            }).ToList();

            var resultados = listaFiltrada.Select(t =>
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == t.IdCliente);
                string nombreCliente = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "⚠ Cliente no encontrado";

                return new TurnoVista
                {
                    Id = t.Id,
                    Cliente = nombreCliente,
                    Fecha = t.FechaHora.ToShortDateString(),
                    Hora = t.FechaHora.ToString("HH:mm"),
                    TipoTramite = t.TipoTramite,
                    Estado = t.Estado,
                    Confirmado = t.Confirmado
                };
            }).ToList();

            dgTurnos.ItemsSource = resultados;
            borderSinTurnos.Visibility = resultados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnAlertaTurno_Click(object sender, RoutedEventArgs e)
        {
            if (dgTurnos.SelectedItem is TurnoVista seleccionado)
            {
                var turnoOriginal = turnoDAO.ObtenerTodos().FirstOrDefault(t => t.Id == seleccionado.Id);

                if (turnoOriginal != null)
                {
                    var cliente = clienteDAO.ObtenerPorId(turnoOriginal.IdCliente);
                    string telefono = NormalizarTelefonoWhatsApp(cliente.Telefono);

                    string mensaje = $"Hola {cliente.Nombre}, le recordamos que tiene un turno el día {turnoOriginal.FechaHora:dd/MM/yyyy} a las {turnoOriginal.FechaHora:HH:mm} en la Escribania Ejemplo (Direccion: Calle Falsa 123) . Por favor confirme si va a asistir.";
                    string mensajeCodificado = Uri.EscapeDataString(mensaje);

                    string url = $"https://wa.me/{telefono}?text={mensajeCodificado}";

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("No se encontró el turno original.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un turno primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnActualizarLista_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";  // <- Agregalo acá
            cmbFiltroEstado.SelectedIndex = 0;
            CargarTurnos();
        }

        public static string QuitarAcentos(string texto)
        {
            return new string(
                texto.Normalize(NormalizationForm.FormD)
                     .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                     .ToArray()
            ).Normalize(NormalizationForm.FormC).ToLower();
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string texto = txtBuscar.Text.Trim();
            if (string.IsNullOrEmpty(texto))
            {
                CargarTurnos();
            }
            else
            {
                FiltrarTurnos(texto); // Usa el método que te pasé antes
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validación: Cliente obligatorio
            if (!(cmbCliente.SelectedItem is Cliente clienteElegido))
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Fecha obligatoria
            if (dpFecha.SelectedDate == null)
            {
                MessageBox.Show("La fecha del turno es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Hora obligatoria
            if (tpHora.SelectedTime == null)
            {
                MessageBox.Show("La hora del turno es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Tipo de trámite obligatorio
            if (string.IsNullOrWhiteSpace(cmbTramite.Text))
            {
                MessageBox.Show("El tipo de trámite es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idCliente = clienteElegido.Id;

            DateTime fechaHoraTurno = dpFecha.SelectedDate.Value + tpHora.SelectedTime.Value.TimeOfDay;

            // Validación: no permitir agendar/modificar turnos en fechas anteriores a hoy
            if (fechaHoraTurno.Date < DateTime.Today)
            {
                MessageBox.Show("No se puede agendar un turno en una fecha anterior a hoy.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Verificar superposición de turnos (margen de 15 minutos)
            var turnosExistentes = turnoDAO.ObtenerTodos();
            foreach (var turnoExistente in turnosExistentes)
            {
                // Si estamos modificando, ignorar el turno actual
                if (turnoSeleccionado != null && turnoExistente.Id == turnoSeleccionado.Id)
                    continue;

                // Verificar si es el mismo día
                if (turnoExistente.FechaHora.Date == fechaHoraTurno.Date)
                {
                    // Calcular diferencia en minutos
                    double diferenciaMinutos = Math.Abs((turnoExistente.FechaHora - fechaHoraTurno).TotalMinutes);

                    if (diferenciaMinutos < 15)
                    {
                        MessageBox.Show($"Ya existe un turno a las {turnoExistente.FechaHora:HH:mm} el mismo día. Los turnos deben tener al menos 15 minutos de diferencia.", 
                            "Conflicto de Horario", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            // Todo turno nuevo se agenda siempre en estado Pendiente.
            // Confirmar/Cancelar se hacen después desde los botones de acciones del listado.
            var turno = new Turno
            {
                IdCliente = idCliente,
                FechaHora = fechaHoraTurno,
                Motivo = cmbTramite.Text,
                TipoTramite = cmbTramite.Text,
                Estado = "Pendiente",
                Observaciones = txtObservaciones.Text,
                Confirmado = false
            };

            turnoDAO.Insertar(turno);
            _auditoria.Registrar(SesionActual.NombreUsuario, "ALTA", "Turnos", null, $"Turno de '{turno.TipoTramite}' para '{clienteElegido.Nombre} {clienteElegido.Apellido}' el {turno.FechaHora:dd/MM/yyyy HH:mm} creado");
            MessageBox.Show("Turno guardado correctamente.");

            CargarTurnos();
            LimpiarFormulario();
        }

        private void btnSalir_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private readonly List<string> tramites = new List<string>
{
    "Escritura de Compraventa",
    "Constitución de Sociedad",
    "Poder General",
    "Certificado de Dominio",
    "Consulta General",
    "Testamento",
    "Donación",
    "Hipoteca"
};

        private readonly List<string> estados = new List<string>
{
    "Todos los Estados", "Pendiente", "Confirmado", "En Proceso", "Finalizado", "Cancelado"
};

        private void CargarEstados()
        {
            cmbFiltroEstado.ItemsSource = estados;
            cmbFiltroEstado.SelectedIndex = 0;

            // Un turno nuevo siempre se agenda en Pendiente; el resto de estados
            // solo se alcanzan luego desde los botones de acciones del listado.
            cmbEstado.ItemsSource = new List<string> { "Pendiente" };
            cmbEstado.SelectedIndex = 0;
        }
        private void CargarTramites()
        {
            cmbTramite.ItemsSource = tramites;
        }

        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string estadoSeleccionado = cmbFiltroEstado.SelectedItem as string;

            var turnos = turnoDAO.ObtenerTodos();

            if (estadoSeleccionado == "Todos los Estados")
            {
                CargarTurnos(); // ya usa TurnoVista
            }
            else
            {
                var filtrados = turnos
                    .Where(t => t.Estado != null && t.Estado.Equals(estadoSeleccionado, StringComparison.OrdinalIgnoreCase))
                    .Select(t =>
                    {
                        var cliente = clientes.FirstOrDefault(c => c.Id == t.IdCliente);
                        string nombreCliente = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "⚠ Cliente no encontrado";

                        return new TurnoVista
                        {
                            Id = t.Id,
                            Cliente = nombreCliente,
                            Fecha = t.FechaHora.ToShortDateString(),
                            Hora = t.FechaHora.ToString("HH:mm"),
                            TipoTramite = t.TipoTramite,
                            Estado = t.Estado,
                            Confirmado = t.Confirmado
                        };
                    }).ToList();

                dgTurnos.ItemsSource = filtrados;
                borderSinTurnos.Visibility = filtrados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            TurnoVista seleccionado = null;

            if (sender is Button boton && boton.DataContext is TurnoVista vista)
            {
                seleccionado = vista;
            }
            else if (dgTurnos.SelectedItem is TurnoVista vistaSeleccionada)
            {
                seleccionado = vistaSeleccionada;
            }

            if (seleccionado == null)
            {
                MessageBox.Show("Seleccioná un turno para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var turnoOriginal = turnoDAO.ObtenerTodos().FirstOrDefault(t => t.Id == seleccionado.Id);
            if (turnoOriginal != null)
            {
                var editarWindow = new EditarTurnoWindow(turnoOriginal);
                editarWindow.ShowDialog();
                CargarTurnos(); // refresca la lista luego de cerrar la ventana
                LimpiarFormulario();
            }
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.DataContext is TurnoVista vista)
            {
                if (MessageBox.Show($"¿Estás seguro que querés eliminar el turno ID {vista.Id}?",
                                    "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    turnoDAO.EliminarTurno(vista.Id);
                    _auditoria.Registrar(SesionActual.NombreUsuario, "BAJA", "Turnos", vista.Id, $"Turno de '{vista.Cliente}' ({vista.TipoTramite} el {vista.Fecha}) eliminado");
                    MessageBox.Show("Turno eliminado correctamente.");
                    CargarTurnos();
                    LimpiarFormulario();
                }
            }
            else
            {
                MessageBox.Show("No se pudo determinar qué turno eliminar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            TurnoVista seleccionado = null;

            if (sender is Button boton && boton.DataContext is TurnoVista vista)
            {
                seleccionado = vista;
            }
            else if (dgTurnos.SelectedItem is TurnoVista vistaSeleccionada)
            {
                seleccionado = vistaSeleccionada;
            }

            if (seleccionado == null)
            {
                MessageBox.Show("Seleccioná un turno para confirmar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var turnoOriginal = turnoDAO.ObtenerTodos().FirstOrDefault(t => t.Id == seleccionado.Id);
            if (turnoOriginal == null)
            {
                MessageBox.Show("No se encontró el turno original.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (turnoOriginal.Estado == "Confirmado")
            {
                MessageBox.Show("Este turno ya se encuentra en estado confirmado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("¿Querés marcar este turno como Confirmado?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            turnoDAO.Confirmar(seleccionado.Id);
            _auditoria.Registrar(SesionActual.NombreUsuario, "MODIFICACION", "Turnos", turnoOriginal.Id, $"Turno ID {turnoOriginal.Id} ({turnoOriginal.TipoTramite}) confirmado");
            CargarTurnos();
            LimpiarFormulario();
        }

        private void btnCancelarTurno_Click(object sender, RoutedEventArgs e)
        {
            TurnoVista seleccionado = null;

            if (sender is Button boton && boton.DataContext is TurnoVista vista)
            {
                seleccionado = vista;
            }
            else if (dgTurnos.SelectedItem is TurnoVista vistaSeleccionada)
            {
                seleccionado = vistaSeleccionada;
            }

            if (seleccionado == null)
            {
                MessageBox.Show("Seleccioná un turno para cancelar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var turnoOriginal = turnoDAO.ObtenerTodos().FirstOrDefault(t => t.Id == seleccionado.Id);
            if (turnoOriginal == null)
            {
                MessageBox.Show("No se encontró el turno original.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (turnoOriginal.Estado == "Cancelado")
            {
                MessageBox.Show("Este turno ya se encuentra en estado cancelado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("¿Querés marcar este turno como Cancelado?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            turnoOriginal.Estado = "Cancelado";
            turnoOriginal.Confirmado = false;
            turnoDAO.Modificar(turnoOriginal);
            _auditoria.Registrar(SesionActual.NombreUsuario, "MODIFICACION", "Turnos", turnoOriginal.Id, $"Turno ID {turnoOriginal.Id} ({turnoOriginal.TipoTramite}) cancelado");
            CargarTurnos();
            LimpiarFormulario();
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void dgTurnos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Solo almacenar el turno seleccionado, NO cargar datos en el formulario
            if (dgTurnos.SelectedItem is TurnoVista vista)
            {
                turnoSeleccionado = turnoDAO.ObtenerTodos().FirstOrDefault(t => t.Id == vista.Id);
            }
            else
            {
                // Si no hay selección, limpiar turnoSeleccionado
                turnoSeleccionado = null;
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e) => Close();
        private void btnEmitirTurno_Click(object sender, RoutedEventArgs e)
        {
            if (turnoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un turno para emitir.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cliente = clientes.FirstOrDefault(c => c.Id == turnoSeleccionado.IdCliente);
            string nombre = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "Desconocido";

            var flowDoc = new System.Windows.Documents.FlowDocument
            {
                PageWidth = 794, // ancho aproximado de una hoja A4 a 96 DPI
                PagePadding = new Thickness(60)
            };

            flowDoc.Blocks.Add(new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run("NOTALEGIS — COMPROBANTE DE TURNO"))
            {
                FontSize = 18, FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 8)
            });

            flowDoc.Blocks.Add(new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run($"Emitido el {DateTime.Now:dd/MM/yyyy HH:mm}"))
            {
                FontSize = 10, TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 24)
            });

            var campos = new (string, string)[]
            {
                ("Cliente:", nombre),
                ("Fecha:", turnoSeleccionado.FechaHora.ToString("dd/MM/yyyy")),
                ("Hora:", turnoSeleccionado.FechaHora.ToString("HH:mm")),
                ("Tipo de Trámite:", turnoSeleccionado.TipoTramite),
                ("Estado:", turnoSeleccionado.Estado),
                ("Confirmado:", turnoSeleccionado.Confirmado ? "Sí" : "No"),
                ("Observaciones:", turnoSeleccionado.Observaciones ?? "—")
            };

            foreach (var (label, valor) in campos)
            {
                var p = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 4, 0, 4) };
                p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(label + "  ")));
                p.Inlines.Add(new System.Windows.Documents.Run(valor));
                flowDoc.Blocks.Add(p);
            }

            var visor = new System.Windows.Controls.FlowDocumentReader
            {
                Document = flowDoc,
                ViewingMode = System.Windows.Controls.FlowDocumentReaderViewingMode.Page
            };

            var ventanaPreview = new Window
            {
                Title = "Vista previa — Comprobante de Turno",
                Content = visor,
                Width = 850,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            ventanaPreview.ShowDialog();
        }
        private void btnConsultarTurno_Click(object sender, RoutedEventArgs e)
        {
            if (dgTurnos.SelectedItem is TurnoVista seleccionado)
            {
                var turnoOriginal = turnoDAO.ObtenerTodos().FirstOrDefault(t => t.Id == seleccionado.Id);
                if (turnoOriginal != null)
                {
                    var consultarWindow = new ConsultarTurnoWindow(turnoOriginal);
                    consultarWindow.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Seleccioná un turno para consultar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Los teléfonos demo se guardan con formato local (ej: "11-5101-1017"), sin
        // código de país. wa.me necesita el número completo en dígitos con el 54 (Argentina)
        // + 9 (celular) al frente, o el link abre una página de "número inválido".
        private static string NormalizarTelefonoWhatsApp(string telefono)
        {
            string soloDigitos = new string((telefono ?? "").Where(char.IsDigit).ToArray());
            if (soloDigitos.StartsWith("54")) return soloDigitos;
            return "549" + (soloDigitos.StartsWith("0") ? soloDigitos.Substring(1) : soloDigitos);
        }

        private void btnConfirmarWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            if (turnoSeleccionado == null)
            {
                MessageBox.Show("Seleccioná un turno para confirmar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cliente = clienteDAO.ObtenerPorId(turnoSeleccionado.IdCliente);
            if (cliente == null || string.IsNullOrWhiteSpace(cliente.Telefono))
            {
                MessageBox.Show("No se encontró el teléfono del cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string telefono = NormalizarTelefonoWhatsApp(cliente.Telefono);

            string mensaje =
                $"Hola {cliente.Nombre}, le confirmamos que su turno ha sido *CONFIRMADO*:\n\n" +
                $"📅 *Fecha:* {turnoSeleccionado.FechaHora:dd/MM/yyyy}\n" +
                $"🕐 *Hora:* {turnoSeleccionado.FechaHora:HH:mm}\n" +
                $"📋 *Trámite:* {turnoSeleccionado.TipoTramite}\n\n" +
                $"Por favor, llegue 5 minutos antes. Si necesita cancelar o reprogramar, responda este mensaje.\n\n" +
                $"Saludos, *Equipo Notalegis*";

            string url = $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje)}";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
    }
}