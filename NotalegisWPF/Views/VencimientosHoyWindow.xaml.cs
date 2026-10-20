using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ClosedXML.Excel;
using Microsoft.Win32;
using Notalegis.Datos.Repositorios;
using Notalegis.Models;

namespace Notalegis.Views
{
    public partial class VencimientosHoyWindow : Window
    {
        private List<Tramite> tramites;

        public VencimientosHoyWindow(DateTime fecha)
        {
            InitializeComponent();
            CargarVencimientos(fecha);
        }

        private void CargarVencimientos(DateTime fecha)
        {
            var dao = new TramiteDAO();
            tramites = dao.ObtenerTodos()
                .Where(t => t.FechaVencimiento.Date == fecha.Date)
                .OrderBy(t => t.Prioridad)
                .ToList();

            dgVencimientos.ItemsSource = tramites;
            borderSinVencimientos.Visibility = tramites.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            lblTitulo.Text = $"Trámites que vencen el {fecha:dd 'de' MMMM 'de' yyyy}";
            lblSubtitulo.Text = tramites.Count == 0
                ? "No hay trámites con vencimiento para este día"
                : $"{tramites.Count} trámite{(tramites.Count != 1 ? "s" : "")} encontrado{(tramites.Count != 1 ? "s" : "")}";
            lblConteo.Text = $"Total: {tramites.Count} trámite{(tramites.Count != 1 ? "s" : "")}";
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden exportar listados.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (tramites == null || tramites.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Sin datos", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = $"Vencimientos_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (dialog.ShowDialog() != true) return;

            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Vencimientos");

            string[] cols = { "ID", "Cliente", "Tipo de Trámite", "Estado", "Prioridad", "Vencimiento", "Observaciones" };
            for (int i = 0; i < cols.Length; i++)
                hoja.Cell(1, i + 1).Value = cols[i];

            var enc = hoja.Range(1, 1, 1, cols.Length);
            enc.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#C0392B");
            enc.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            enc.Style.Font.Bold = true;
            enc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int i = 0; i < tramites.Count; i++)
            {
                var t = tramites[i];
                int fila = i + 2;
                hoja.Cell(fila, 1).Value = t.Id;
                hoja.Cell(fila, 2).Value = t.Cliente ?? "";
                hoja.Cell(fila, 3).Value = t.TipoTramite ?? "";
                hoja.Cell(fila, 4).Value = t.Estado ?? "";
                hoja.Cell(fila, 5).Value = t.Prioridad ?? "";
                hoja.Cell(fila, 6).Value = t.FechaVencimiento.ToString("dd/MM/yyyy");
                hoja.Cell(fila, 7).Value = t.Observaciones ?? "";

                if (i % 2 == 1)
                    hoja.Row(fila).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#FDEDEC");
            }

            hoja.Columns().AdjustToContents();
            workbook.SaveAs(dialog.FileName);

            if (MessageBox.Show("Exportación completada.\n¿Desea abrir el archivo?", "Listo",
                MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = dialog.FileName, UseShellExecute = true });
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e) => Close();
    }
}
