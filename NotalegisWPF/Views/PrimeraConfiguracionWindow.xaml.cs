using System.Windows;
using System.Windows.Controls;
using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class PrimeraConfiguracionWindow : Window
    {
        private readonly UsuarioService usuarioService = new UsuarioService();
        public string UsuarioAdminCreado { get; private set; }

        public PrimeraConfiguracionWindow()
        {
            InitializeComponent();
        }

        private void ValidarFormulario(object sender, object e)
        {
            bool valido = true;

            // Validar usuario
            string usuario = txtUsuarioAdmin.Text?.Trim() ?? "";
            if (usuario.Length < 4)
            {
                txtErrorUsuario.Text = "El nombre de usuario debe tener al menos 4 caracteres.";
                txtErrorUsuario.Visibility = Visibility.Visible;
                valido = false;
            }
            else
            {
                txtErrorUsuario.Visibility = Visibility.Collapsed;
            }

            // Validar contraseña
            string pass = txtContrasenia.Password;
            if (pass.Length < 6)
            {
                txtErrorContrasenia.Text = "La contraseña debe tener al menos 6 caracteres.";
                txtErrorContrasenia.Visibility = Visibility.Visible;
                valido = false;
            }
            else
            {
                txtErrorContrasenia.Visibility = Visibility.Collapsed;
            }

            // Validar confirmación
            string confirmar = txtConfirmarContrasenia.Password;
            if (pass.Length >= 6 && confirmar != pass)
            {
                txtErrorConfirmar.Text = "Las contraseñas no coinciden.";
                txtErrorConfirmar.Visibility = Visibility.Visible;
                valido = false;
            }
            else if (confirmar == pass && pass.Length >= 6)
            {
                txtErrorConfirmar.Visibility = Visibility.Collapsed;
            }

            btnCrearAdmin.IsEnabled = valido && usuario.Length >= 4;
        }

        private void btnCrearAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var nuevoAdmin = new Usuario
                {
                    NombreUsuario = txtUsuarioAdmin.Text.Trim(),
                    Contraseña = txtContrasenia.Password,
                    Rol = "Administrador"
                };

                usuarioService.Agregar(nuevoAdmin);

                UsuarioAdminCreado = nuevoAdmin.NombreUsuario;

                // Mostrar éxito y cerrar
                txtExito.Visibility = Visibility.Visible;
                btnCrearAdmin.IsEnabled = false;
                txtUsuarioAdmin.IsEnabled = false;
                txtContrasenia.IsEnabled = false;
                txtConfirmarContrasenia.IsEnabled = false;

                MessageBox.Show(
                    $"El usuario administrador '{nuevoAdmin.NombreUsuario}' fue creado correctamente.\n\nAhora puede iniciar sesión.",
                    "Configuración completa",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al crear el administrador: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
