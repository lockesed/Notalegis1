using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;

namespace Notalegis.Views
{
    public partial class RecordatorioVencimientosWindow : Window
    {
        private readonly int _diasAnticipacion;

        public RecordatorioVencimientosWindow(int diasAnticipacion = 7)
        {
            InitializeComponent();
            _diasAnticipacion = diasAnticipacion;
            Loaded += (s, e) => CargarVencimientos();
        }

        private void CargarVencimientos()
        {
            try
            {
                var hoy = DateTime.Today;
                var limite = hoy.AddDays(_diasAnticipacion);

                var todos = new TramiteDAO().ObtenerTodos();
                var proximos = todos
                    .Where(t => t.FechaVencimiento.Date >= hoy && t.FechaVencimiento.Date <= limite
                                && t.Estado != "Finalizado" && t.Estado != "Cancelado")
                    .OrderBy(t => t.FechaVencimiento)
                    .Select(t => new FilaRecordatorio
                    {
                        FechaVencimiento = t.FechaVencimiento,
                        DiasRestantes    = (t.FechaVencimiento.Date - hoy).Days == 0
                                             ? "HOY"
                                             : $"{(t.FechaVencimiento.Date - hoy).Days} días",
                        ClienteNombre    = LimpiarNombreCliente(t.Cliente),
                        TipoTramite      = t.TipoTramite,
                        Estado           = t.Estado,
                        Prioridad        = t.Prioridad
                    })
                    .ToList();

                dgVencimientos.ItemsSource = proximos;

                int n = proximos.Count;
                lblConteo.Text = n == 1 ? "1 trámite próximo a vencer" : $"{n} trámites próximos a vencer";
                lblSubtitulo.Text = $"Los siguientes trámites vencen entre hoy y el {limite:dd/MM/yyyy}";

                if (n == 0)
                    this.Close();
            }
            catch
            {
                this.Close();
            }
        }

        private static string LimpiarNombreCliente(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return "";
            if (valor.Contains(" - DNI:"))
                return valor.Split(new[] { " - DNI:" }, StringSplitOptions.None)[0];
            return valor;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnIrATramites_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var ventana = new TramitesView();
            ventana.ShowDialog();
        }
    }

    public class FilaRecordatorio
    {
        public string DiasRestantes    { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string ClienteNombre    { get; set; }
        public string TipoTramite      { get; set; }
        public string Estado           { get; set; }
        public string Prioridad        { get; set; }
    }
}
