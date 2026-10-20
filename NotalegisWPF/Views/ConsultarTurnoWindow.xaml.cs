using System;
using System.Windows;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Views
{
    public partial class ConsultarTurnoWindow : Window
    {
        public ConsultarTurnoWindow(Turno turno)
        {
            InitializeComponent();

            var cliente = new ClienteDAO().ObtenerTodos()
                .FirstOrDefault(c => c.Id == turno.IdCliente);

            txtCliente.Text = cliente != null ? $"{cliente.Nombre} {cliente.Apellido}" : "⚠ Cliente no encontrado";
            txtFecha.Text = turno.FechaHora.ToShortDateString();
            txtHora.Text = turno.FechaHora.ToString("HH:mm");
            txtTipoTramite.Text = turno.TipoTramite;
            txtEstado.Text = turno.Estado;
            txtObservaciones.Text = turno.Observaciones;
            txtConfirmado.Text = turno.Confirmado ? "Sí" : "No";
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
