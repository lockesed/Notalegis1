using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;
using Notalegis.Negocio.Servicios;
using System;
using System.Windows;

namespace Notalegis.Views
{
    public partial class EditarTurnoWindow : Window
    {
        private Turno turno;
        private readonly TurnoDAO turnoDAO = new TurnoDAO();
        private readonly ClienteDAO clienteDAO = new ClienteDAO();
        private readonly AuditoriaService _auditoria = new AuditoriaService();
        private List<Cliente> clientes;
        private bool _guardado = false;

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
            "Pendiente",
            "Confirmado",
            "En Proceso",
            "Finalizado",
            "Cancelado"
        };

        public EditarTurnoWindow(Turno turnoSeleccionado)
        {
            InitializeComponent();
            this.turno = turnoSeleccionado;

            CargarClientes();
            CargarEstados();
            CargarTramites();

            var cliente = clienteDAO.ObtenerPorId(turnoSeleccionado.IdCliente);
            cmbCliente.SelectedItem = clientes.FirstOrDefault(c => c.Id == cliente.Id);
            cmbCliente.IsEnabled = false;

            dpFecha.SelectedDate = turnoSeleccionado.FechaHora.Date;
            txtHora.SelectedTime = DateTime.Today.Add(turnoSeleccionado.FechaHora.TimeOfDay);
            cmbTramite.Text = turnoSeleccionado.TipoTramite;
            cmbEstado.Text = turnoSeleccionado.Estado;
            txtObservaciones.Text = turnoSeleccionado.Observaciones;
        }

        private void CargarClientes()
        {
            clientes = clienteDAO.ObtenerTodos();
            cmbCliente.ItemsSource = clientes;
            cmbCliente.DisplayMemberPath = "NombreCompleto";
        }

        private void CargarTramites()
        {
            cmbTramite.ItemsSource = tramites;
        }

        private void CargarEstados()
        {
            cmbEstado.ItemsSource = estados;
        }

        private void CargarTurno()
        {

            cmbCliente.Text = clientes.FirstOrDefault(c => c.Id == turno.IdCliente)?.ToString();
            dpFecha.SelectedDate = turno.FechaHora.Date;
            txtHora.SelectedTime = DateTime.Today.Add(turno.FechaHora.TimeOfDay);
            cmbTramite.Text = turno.TipoTramite;
            cmbEstado.Text = turno.Estado;
            txtObservaciones.Text = turno.Observaciones;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            _guardado = true;
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!_guardado && !Notalegis.App.BloqueoAutomatico)
            {
                var resultado = MessageBox.Show(
                    "¿Seguro que desea cerrar? Los cambios no guardados se perderán.",
                    "Confirmar cierre",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                    e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void GuardarCambiosTurno()
        {
            DateTime fechaHoraTurno = dpFecha.SelectedDate.Value.Date + txtHora.SelectedTime.Value.TimeOfDay;

            // Validación: no permitir mover un turno a una nueva fecha anterior a hoy.
            // Si el turno ya era de una fecha pasada (y no se cambió la fecha), se permite
            // editar igual para poder actualizar estado u observaciones (ej: cliente no se presentó).
            if (fechaHoraTurno.Date < DateTime.Today && fechaHoraTurno.Date != turno.FechaHora.Date)
            {
                MessageBox.Show("No se puede mover un turno a una fecha anterior a hoy.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Verificar superposición de turnos (margen de 15 minutos)
            var turnosExistentes = turnoDAO.ObtenerTodos();
            foreach (var turnoExistente in turnosExistentes)
            {
                // Ignorar el turno actual que estamos editando
                if (turnoExistente.Id == turno.Id)
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

            turno.FechaHora = fechaHoraTurno;
            turno.IdCliente = ((Cliente)cmbCliente.SelectedItem).Id;
            turno.TipoTramite = cmbTramite.Text;
            turno.Estado = cmbEstado.Text;
            turno.Observaciones = txtObservaciones?.Text?.Trim() ?? "";
            turno.Confirmado = cmbEstado.Text == "Confirmado"; // Determinar confirmado según el estado

            try
            {
                turnoDAO.Modificar(turno);
                _auditoria.Registrar(SesionActual.NombreUsuario, "MODIFICACION", "Turnos", turno.Id, $"Turno ID {turno.Id} ({turno.TipoTramite}) editado desde ventana de edición");
                MessageBox.Show("Turno actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                _guardado = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el turno: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validación: Cliente obligatorio
            if (cmbCliente.SelectedItem == null)
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
            if (txtHora.SelectedTime == null)
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

            // Validación: Estado obligatorio
            if (string.IsNullOrWhiteSpace(cmbEstado.Text))
            {
                MessageBox.Show("El estado del turno es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Observaciones: NO obligatorio

            // Si pasa todas las validaciones, continuar con el guardado
            GuardarCambiosTurno();
        }
    }
}