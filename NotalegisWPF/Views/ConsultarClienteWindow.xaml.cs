using System;
using System.Windows;
using System.Windows.Media;
using Notalegis.Datos.Modelos;

namespace Notalegis.Views
{
    public partial class ConsultarClienteWindow : Window
    {
        public ConsultarClienteWindow(Cliente cliente)
        {
            InitializeComponent();

            // Información personal
            txtNombre.Text = cliente.Nombre;
            txtApellido.Text = cliente.Apellido;
            txtDocumento.Text = cliente.DNI;

            // Información de contacto
            txtTelefono.Text = cliente.Telefono;
            txtEmail.Text = cliente.Email;
            txtDireccion.Text = cliente.Direccion;

            // Estado
            txtEstado.Text = cliente.Estado;
            txtEstadoIndicador.Text = cliente.Estado;

            // Indicador visual por color según estado
            switch ((cliente.Estado ?? "").ToLower())
            {
                case "activo":
                    borderEstado.Background = Brushes.LightGreen;
                    iconEstado.Foreground = Brushes.Green;
                    break;
                case "en trámite":
                    borderEstado.Background = Brushes.LightGoldenrodYellow;
                    iconEstado.Foreground = Brushes.Goldenrod;
                    break;
                case "finalizado":
                    borderEstado.Background = Brushes.LightBlue;
                    iconEstado.Foreground = Brushes.DodgerBlue;
                    break;
                case "suspendido":
                    borderEstado.Background = Brushes.MistyRose;
                    iconEstado.Foreground = Brushes.Red;
                    break;
                default:
                    borderEstado.Background = Brushes.Gray;
                    iconEstado.Foreground = Brushes.Gray;
                    break;
            }

            // Observaciones
            txtObservaciones.Text = cliente.Observaciones;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }

}
