using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NLog;
using Notalegis.Models;
using Notalegis.Views;

namespace Notalegis
{
    public partial class App : Application
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public static bool BloqueoAutomatico { get; private set; } = false;

        private DispatcherTimer _timerInactividad;
        private DateTime _ultimaActividad = DateTime.Now;
        private const int MinutosInactividad = 15;

        static App()
        {
            PdfSharpCore.Fonts.GlobalFontSettings.FontResolver = RobotoFontResolver.Instance;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // TemaService deshabilitado: el diseño visual es fijo (NotalegisColors.xaml + BundledTheme Light)

            Dispatcher.BeginInvoke(new Action(async () =>
            {
                var splash = new SplashScreenWindow();
                splash.Show();

                await Task.Delay(400);
                splash.SetEstado("Verificando base de datos…");

                await Task.Run(() =>
                {
                    try
                    {
                        var dao = new Notalegis.Datos.Repositorios.UsuarioDAO();
                        dao.ObtenerTodos();
                    }
                    catch { }
                });

                await Task.Delay(600);
                splash.SetEstado("Listo.");
                await Task.Delay(300);

                var login = new LoginView();
                login.Show();
                splash.Close();
            }));
        }

        public App()
        {
            DispatcherUnhandledException += (s, e) =>
            {
                _log.Error(e.Exception, "Excepción no controlada en UI");
                MessageBox.Show("Ocurrió un error inesperado. El equipo de soporte puede consultar el archivo de log para más detalles.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                _log.Fatal(e.ExceptionObject as Exception, "Excepción fatal en dominio de aplicación");
            };

            InputManager.Current.PreProcessInput += (s, e) =>
            {
                if (e.StagingItem.Input is MouseEventArgs || e.StagingItem.Input is KeyboardEventArgs)
                    _ultimaActividad = DateTime.Now;
            };

            _timerInactividad = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            _timerInactividad.Tick += VerificarInactividad;
            _timerInactividad.Start();
        }

        private void VerificarInactividad(object sender, EventArgs e)
        {
            if (SesionActual.Usuario == null) return;

            var minutosInactivo = (DateTime.Now - _ultimaActividad).TotalMinutes;
            if (minutosInactivo < MinutosInactividad) return;

            _ultimaActividad = DateTime.Now;
            BloqueoAutomatico = true;

            SesionActual.CerrarSesion();

            foreach (Window w in Current.Windows.OfType<Window>().ToList())
            {
                try { w.Close(); } catch { }
            }

            BloqueoAutomatico = false;

            var login = new Notalegis.Views.LoginView();
            login.Show();
        }
    }
}
