using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models; // SesionActual
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class MainView : Window
    {

        private Usuario usuarioActual;
        public MainView()
        {
            InitializeComponent();
            // Establecer la fecha actual en el calendario
            mainCalendar.SelectedDate = DateTime.Today;
        }


        public MainView(Usuario usuario)
        {
            InitializeComponent();
            usuarioActual = usuario;

            string rolCapitalizado = char.ToUpper(usuario.Rol[0]) + usuario.Rol.Substring(1).ToLower();
            lblUsuario.Text = rolCapitalizado;
            lblUserName.Text = usuario.NombreUsuario ?? usuario.Rol;
            lblUserInitials.Text = ObtenerIniciales(usuario.NombreUsuario ?? usuario.Rol);

            ActualizarSaludo();

            // Asignar SelectedDate ya dispara mainCalendar_SelectedDatesChanged, que carga
            // la agenda del día. Volver a llamar a CargarEventosDelDiaAsync acá abajo generaba
            // una carrera entre ambas llamadas (cada una hace Clear() antes de recargar los
            // datos de forma asíncrona), duplicando los turnos del día en la agenda.
            mainCalendar.SelectedDate = DateTime.Today;

            Loaded += async (s, e) =>
            {
                await CargarKpisAsync();
                await CargarVencimientosAsync();
                MostrarRecordatorioSiHayVencimientos();
            };

            AplicarPermisos(usuario.Rol);
        }

        private static string ObtenerIniciales(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "?";
            var partes = nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 1) return partes[0][0].ToString().ToUpper();
            return $"{partes[0][0]}{partes[^1][0]}".ToUpper();
        }

        private void ActualizarSaludo()
        {
            var ahora = DateTime.Now;
            var diaSemana = ahora.ToString("dddd", new System.Globalization.CultureInfo("es-AR"));
            diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);
            var mes = ahora.ToString("MMMM", new System.Globalization.CultureInfo("es-AR"));
            lblFecha.Text = $"{diaSemana} {ahora.Day} de {mes} de {ahora.Year}";

            string saludo = ahora.Hour < 12 ? "Buenos días" : ahora.Hour < 20 ? "Buenas tardes" : "Buenas noches";
            string nombre = usuarioActual?.NombreUsuario ?? "";
            lblSaludo.Text = nombre.Length > 0 ? $"{saludo}, {nombre}" : saludo;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                string rutaManual = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Manual_Notalegis.pdf");
                if (File.Exists(rutaManual))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(rutaManual) { UseShellExecute = true });
                else
                    MessageBox.Show("El manual de usuario no se encuentra en la carpeta del programa.\nContacte al soporte técnico.",
                                    "Manual no encontrado", MessageBoxButton.OK, MessageBoxImage.Information);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        private void AplicarPermisos(string rol)
        {
            string rolNormalizado = rol?.ToLower() ?? "";

            // Módulos de administración: solo el Administrador (usuario del escribano titular).
            if (rolNormalizado != "administrador")
            {
                if (btnModuloUsuarios != null) btnModuloUsuarios.Visibility = Visibility.Collapsed;
                if (btnModuloAuditoria != null) btnModuloAuditoria.Visibility = Visibility.Collapsed;
                if (btnModuloConfiguracion != null) btnModuloConfiguracion.Visibility = Visibility.Collapsed;
                if (btnModuloRespaldo != null) btnModuloRespaldo.Visibility = Visibility.Collapsed;
            }

            // El Procurador solo gestiona expedientes: Trámites, Clientes, Turnos y Escrituras.
            // No ve información financiera (Cobros), stock interno, ni estadísticas/reportes de la escribanía.
            if (rolNormalizado == "procurador")
            {
                if (btnModuloCobros != null) btnModuloCobros.Visibility = Visibility.Collapsed;
                if (btnModuloPedidosInsumos != null) btnModuloPedidosInsumos.Visibility = Visibility.Collapsed;
                if (btnModuloEstadisticas != null) btnModuloEstadisticas.Visibility = Visibility.Collapsed;
                if (btnModuloReportes != null) btnModuloReportes.Visibility = Visibility.Collapsed;
            }
        }

        // Las ventanas de Turnos/Trámites/Cobros se abren como diálogo modal: mientras están
        // abiertas, MainView queda "congelada" de fondo. Sin este refresco, la agenda del día
        // y las tarjetas de KPI quedaban con los datos de cuando se abrió el programa, aunque
        // se hubiera cargado un turno o trámite nuevo justo después.
        private async Task RefrescarAgendaYKpisAsync()
        {
            // No se llama a MostrarRecordatorioSiHayVencimientos() acá: ese recordatorio
            // es un popup que debe aparecer una sola vez al iniciar sesión, no cada vez
            // que se cierra una ventana de Turnos/Trámites/Cobros.
            await CargarEventosDelDiaAsync(mainCalendar.SelectedDate ?? DateTime.Today);
            await CargarKpisAsync();
            await CargarVencimientosAsync();
        }

        private async Task CargarKpisAsync()
        {
            try
            {
                int turnosHoy = 0, tramitesActivos = 0, proximosVencer = 0, cobrosDelMes = 0;
                var hoy = DateTime.Today;
                var limite = hoy.AddDays(7);

                await Task.Run(() =>
                {
                    var turnos = new TurnoDAO().ObtenerTodos();
                    turnosHoy = turnos.Count(t => t.FechaHora.Date == hoy);

                    var tramites = new TramiteDAO().ObtenerTodos();
                    tramitesActivos = tramites.Count(t => t.Estado != "Finalizado" && t.Estado != "Cancelado");
                    proximosVencer = tramites.Count(t =>
                        t.FechaVencimiento.Date >= hoy &&
                        t.FechaVencimiento.Date <= limite &&
                        t.Estado != "Finalizado" && t.Estado != "Cancelado");

                    var cobros = new CobroDAO().ObtenerTodos();
                    cobrosDelMes = cobros.Count(c => c.FechaCobro.Month == hoy.Month && c.FechaCobro.Year == hoy.Year);
                });

                txtKpiTurnos.Text = turnosHoy.ToString();
                txtKpiTramitesActivos.Text = tramitesActivos.ToString();
                txtKpiVencimientos.Text = proximosVencer.ToString();
                txtKpiCobros.Text = cobrosDelMes.ToString();
            }
            catch { /* KPIs son informativos — no interrumpir la carga si falla */ }
        }

        private static readonly string[] AcentosAgenda =
        {
            "#3D7AF5", "#22C55E", "#9333EA", "#F59E0B", "#EF4444", "#06B6D4"
        };

        private async Task CargarEventosDelDiaAsync(DateTime fechaSeleccionada)
        {
            eventosPanel.Children.Clear();

            List<Turno> turnos = new();
            List<Tramite> tramites = new();
            List<Cliente> clientes = new();

            await Task.Run(() =>
            {
                turnos   = new TurnoDAO().ObtenerTodos();
                tramites = new TramiteDAO().ObtenerTodos();
                clientes = new ClienteDAO().ObtenerTodos();
            });

            var turnosDelDia   = turnos.Where(t => t.FechaHora.Date == fechaSeleccionada.Date)
                                       .OrderBy(t => t.FechaHora).ToList();
            var tramitesDelDia = tramites.Where(t => t.FechaVencimiento.Date == fechaSeleccionada.Date).ToList();

            int colorIdx = 0;

            foreach (var turno in turnosDelDia)
            {
                var cliente = clientes.FirstOrDefault(c => c.Id == turno.IdCliente);
                string nombreCliente = cliente != null
                    ? $"{cliente.Nombre} {cliente.Apellido}"
                    : $"Cliente #{turno.IdCliente}";

                string acento = AcentosAgenda[colorIdx % AcentosAgenda.Length];
                colorIdx++;

                eventosPanel.Children.Add(CrearItemAgenda(
                    turno.FechaHora.ToString("HH:mm"),
                    turno.TipoTramite,
                    nombreCliente,
                    acento));
            }

            foreach (var tramite in tramitesDelDia)
            {
                string nombreCliente = tramite.Cliente ?? "";
                if (nombreCliente.Contains(" - DNI:"))
                    nombreCliente = nombreCliente.Split(new[] { " - DNI:" }, StringSplitOptions.None)[0];

                string acento = AcentosAgenda[colorIdx % AcentosAgenda.Length];
                colorIdx++;

                eventosPanel.Children.Add(CrearItemAgenda(
                    "Vence",
                    tramite.TipoTramite,
                    $"{nombreCliente} · {tramite.Estado}",
                    "#EF4444"));
            }

            if (!turnosDelDia.Any() && !tramitesDelDia.Any())
            {
                eventosPanel.Children.Add(new TextBlock
                {
                    Text = "Sin actividad programada para hoy.",
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7A99")),
                    FontSize = 13,
                    FontStyle = FontStyles.Italic,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
            }
        }

        private static Border CrearItemAgenda(string hora, string titulo, string subtitulo, string colorHex)
        {
            var acento = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex));
            var textoPrimario = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E2D3D"));
            var textoSecundario = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7A99"));
            var bordeClaro = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E9F0"));

            var grid = new Grid { Margin = new Thickness(0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lblHora = new TextBlock
            {
                Text = hora,
                FontSize = 12,
                Foreground = textoSecundario,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI")
            };
            Grid.SetColumn(lblHora, 0);

            var barraColor = new Border
            {
                Background = acento,
                CornerRadius = new CornerRadius(2),
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(barraColor, 1);

            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(new TextBlock
            {
                Text = titulo,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = textoPrimario,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontFamily = new FontFamily("Segoe UI")
            });
            stack.Children.Add(new TextBlock
            {
                Text = subtitulo,
                FontSize = 12,
                Foreground = textoSecundario,
                Margin = new Thickness(0, 2, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontFamily = new FontFamily("Segoe UI")
            });
            Grid.SetColumn(stack, 2);

            grid.Children.Add(lblHora);
            grid.Children.Add(barraColor);
            grid.Children.Add(stack);

            return new Border
            {
                Padding = new Thickness(0, 12, 0, 12),
                BorderBrush = bordeClaro,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = grid
            };
        }

        private async void mainCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mainCalendar.SelectedDate.HasValue)
                await CargarEventosDelDiaAsync(mainCalendar.SelectedDate.Value.Date);
        }

        private void CerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            SesionActual.CerrarSesion();
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }

        private async Task CargarVencimientosAsync()
        {
            try
            {
                var hoy = DateTime.Today;
                var limite = hoy.AddDays(7);
                List<Tramite> tramites = new();

                await Task.Run(() => tramites = new TramiteDAO().ObtenerTodos());

                var proximos = tramites
                    .Where(t => t.FechaVencimiento.Date >= hoy &&
                                t.FechaVencimiento.Date <= limite &&
                                t.Estado != "Finalizado" && t.Estado != "Cancelado")
                    .OrderBy(t => t.FechaVencimiento)
                    .Take(5)
                    .ToList();

                vencimientosPanel.Children.Clear();

                if (!proximos.Any())
                {
                    lblSinVencimientos.Visibility = System.Windows.Visibility.Visible;
                    return;
                }

                lblSinVencimientos.Visibility = System.Windows.Visibility.Collapsed;

                var bordeClaro = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E9F0"));
                var textoPrimario = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E2D3D"));
                var textoSecundario = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7A99"));

                foreach (var t in proximos)
                {
                    int diasRestantes = (t.FechaVencimiento.Date - hoy).Days;
                    string etiqueta = diasRestantes == 0 ? "Vence hoy"
                        : diasRestantes == 1 ? "Mañana"
                        : $"En {diasRestantes} días";

                    string bgHex = diasRestantes == 0 ? "#FEE2E2"
                        : diasRestantes <= 2 ? "#FEF3C7"
                        : "#DBEAFE";
                    string fgHex = diasRestantes == 0 ? "#991B1B"
                        : diasRestantes <= 2 ? "#92400E"
                        : "#1D4ED8";

                    var badge = new Border
                    {
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgHex)),
                        CornerRadius = new CornerRadius(20),
                        Padding = new Thickness(8, 3, 8, 3)
                    };
                    badge.Child = new TextBlock
                    {
                        Text = etiqueta,
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgHex)),
                        FontSize = 11,
                        FontWeight = FontWeights.SemiBold,
                        FontFamily = new FontFamily("Segoe UI")
                    };

                    var nombreCliente = t.Cliente ?? "";
                    if (nombreCliente.Contains(" - DNI:"))
                        nombreCliente = nombreCliente.Split(new[] { " - DNI:" }, StringSplitOptions.None)[0];

                    var grid = new Grid { Margin = new Thickness(0) };
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = t.TipoTramite,
                        FontSize = 13, FontWeight = FontWeights.SemiBold,
                        Foreground = textoPrimario,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        FontFamily = new FontFamily("Segoe UI")
                    });
                    infoStack.Children.Add(new TextBlock
                    {
                        Text = nombreCliente,
                        FontSize = 11, Foreground = textoSecundario,
                        Margin = new Thickness(0, 2, 0, 0),
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        FontFamily = new FontFamily("Segoe UI")
                    });
                    Grid.SetColumn(infoStack, 0);

                    var badgeWrap = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
                    badgeWrap.Children.Add(badge);
                    Grid.SetColumn(badgeWrap, 1);

                    grid.Children.Add(infoStack);
                    grid.Children.Add(badgeWrap);

                    vencimientosPanel.Children.Add(new Border
                    {
                        Padding = new Thickness(0, 10, 0, 10),
                        BorderBrush = bordeClaro,
                        BorderThickness = new Thickness(0, 0, 0, 1),
                        Child = grid
                    });
                }
            }
            catch { /* panel de vencimientos es informativo, no crítico */ }
        }

        private void MostrarRecordatorioSiHayVencimientos()
        {
            try
            {
                var hoy = DateTime.Today;
                var limite = hoy.AddDays(7);
                var todos = new TramiteDAO().ObtenerTodos();
                bool hayProximos = todos.Any(t =>
                    t.FechaVencimiento.Date >= hoy &&
                    t.FechaVencimiento.Date <= limite &&
                    t.Estado != "Finalizado" && t.Estado != "Cancelado");

                if (hayProximos)
                {
                    var recordatorio = new RecordatorioVencimientosWindow { Owner = this };
                    recordatorio.ShowDialog();
                }
            }
            catch { /* Silencioso — el recordatorio es informativo, no crítico */ }
        }

        private void btnAuditoria_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden acceder a la auditoría del sistema.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new AuditoriaView { Owner = this };
            ventana.ShowDialog();
        }

        private void btnEstadisticas_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new EstadisticasView { Owner = this };
            ventana.ShowDialog();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new ReportesView { Owner = this };
            ventana.ShowDialog();
        }

        private void btnAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AcercaDeWindow { Owner = this };
            ventana.ShowDialog();
        }

        private void btnConfiguracion_Click(object sender, RoutedEventArgs e)
        {
            var ventanaConfiguracion = new ConfiguracionWindow();
            ventanaConfiguracion.ShowDialog();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden acceder a la gestión de usuarios.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventanaUsuarios = new UsuariosView();
            ventanaUsuarios.ShowDialog();
        }

        private void btnClientes_Click(object sender, RoutedEventArgs e)
        {
            var ventanaClientes = new NotalegisWPF.Views.ClientesView();
            ventanaClientes.Show();
        }

        private async void btnTurnos_Click(object sender, RoutedEventArgs e)
        {
            TurnosView ventanaTurnos = new TurnosView();
            ventanaTurnos.ShowDialog();
            await RefrescarAgendaYKpisAsync();
        }

        private void btnEscrituras_Click(object sender, RoutedEventArgs e)
        {
            EscriturasView ventanaEscrituras = new EscriturasView();
            ventanaEscrituras.ShowDialog();
        }

        private async void btnTramites_Click(object sender, RoutedEventArgs e)
        {
            TramitesView ventanaTramites = new TramitesView();
            ventanaTramites.ShowDialog();
            await RefrescarAgendaYKpisAsync();
        }

        private async void btnCobros_Click(object sender, RoutedEventArgs e)
        {
            GestionCobrosWindow ventanaCobros = new GestionCobrosWindow();
            ventanaCobros.ShowDialog();
            await RefrescarAgendaYKpisAsync();
        }

        private void btnPedidosInsumos_Click(object sender, RoutedEventArgs e)
        {
            PedidosInsumosView ventanaPedidos = new PedidosInsumosView();
            ventanaPedidos.ShowDialog();
        }

        private async void btnRespaldo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cambiar el cursor a espera y deshabilitar temporalmente la UI
                this.Cursor = System.Windows.Input.Cursors.Wait;
                this.IsEnabled = false;

                // Ejecutar el backup en un hilo secundario para no congelar la interfaz
                string rutaGuardado = await System.Threading.Tasks.Task.Run(() =>
                {
                    var backupService = new Notalegis.Negocio.Servicios.BackupService();
                    // Guardar en la ruta base del servidor
                    return backupService.GenerarCopiaSeguridad(@"C:\BackupsNotalegis");
                });

                MessageBox.Show($"Se ha generado exitosamente la copia de seguridad en el SERVIDOR.\n\nRuta de acceso local (Servidor): {rutaGuardado}\n\nConserve este archivo en un lugar seguro.",
                    "Copia de Seguridad Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al intentar generar la copia de seguridad.\n\nDetalle: {ex.Message}",
                    "Error en Respaldo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                this.IsEnabled = true;
            }
        }
    }
}
