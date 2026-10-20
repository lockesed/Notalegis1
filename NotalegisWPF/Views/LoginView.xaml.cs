using System;
using System.Windows;
using System.Windows.Input;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Views;

namespace Notalegis.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var usuarioDAO = new Notalegis.Datos.Repositorios.UsuarioDAO();
                if (usuarioDAO.ObtenerTodos().Count == 0)
                {
                    var configWindow = new PrimeraConfiguracionWindow();
                    configWindow.ShowDialog();
                    if (configWindow.UsuarioAdminCreado != null)
                        txtUsuario.Text = configWindow.UsuarioAdminCreado;
                }
            }
            catch { }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Ingresar();
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e) => Ingresar();

        private void Ingresar()
        {
            OcultarError();

            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MostrarError("Ingresá usuario y contraseña.");
                return;
            }

            var servicio = new Notalegis.Negocio.Servicios.UsuarioService();
            var (resultado, usuarioLogueado, bloqueadoHasta) =
                servicio.ValidarLoginConBloqueo(txtUsuario.Text, txtPassword.Password);

            switch (resultado)
            {
                case Notalegis.Negocio.Servicios.UsuarioService.ResultadoLogin.Ok:
                    SesionActual.IniciarSesion(usuarioLogueado);
                    new MainView(usuarioLogueado).Show();
                    Close();
                    break;

                case Notalegis.Negocio.Servicios.UsuarioService.ResultadoLogin.CuentaBloqueada:
                    var restante = (int)Math.Ceiling((bloqueadoHasta!.Value - DateTime.Now).TotalMinutes);
                    MostrarError($"Cuenta bloqueada. Intentá en {restante} minuto{(restante != 1 ? "s" : "")}.");
                    break;

                default:
                    MostrarError("Usuario o contraseña incorrectos.");
                    break;
            }
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            pnlError.Visibility = Visibility.Visible;
        }

        private void OcultarError()
        {
            pnlError.Visibility = Visibility.Collapsed;
        }
    }
}
