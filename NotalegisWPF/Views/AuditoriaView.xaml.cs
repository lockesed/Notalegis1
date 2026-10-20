using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class AuditoriaView : Window
    {
        private List<RegistroAuditoria> todosLosRegistros;
        private readonly AuditoriaService _servicio = new AuditoriaService();

        public AuditoriaView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbFiltroModulo.SelectedIndex = 0;
            cmbFiltroAccion.SelectedIndex = 0;
            CargarRegistros();
        }

        private void CargarRegistros()
        {
            todosLosRegistros = _servicio.ObtenerTodos(1000);
            AplicarFiltros();
        }

        private void AplicarFiltros()
        {
            if (todosLosRegistros == null) return;

            string usuario = txtFiltroUsuario.Text.Trim().ToLower();
            string modulo  = (cmbFiltroModulo.SelectedItem as ComboBoxItem)?.Content.ToString();
            string accion  = (cmbFiltroAccion.SelectedItem as ComboBoxItem)?.Content.ToString();

            var filtrados = todosLosRegistros.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(usuario))
                filtrados = filtrados.Where(r => r.Usuario.ToLower().Contains(usuario));

            if (modulo != "Todos" && !string.IsNullOrEmpty(modulo))
                filtrados = filtrados.Where(r => r.Modulo == modulo);

            if (accion != "Todas" && !string.IsNullOrEmpty(accion))
                filtrados = filtrados.Where(r => r.Accion == accion);

            var lista = filtrados.ToList();
            dgAuditoria.ItemsSource = lista;
            borderSinAuditoria.Visibility = lista.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            lblConteo.Text = $"Mostrando {lista.Count} registro{(lista.Count != 1 ? "s" : "")} de un total de {todosLosRegistros.Count}.";
        }

        private void txtFiltro_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltros();
        private void cmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltros();

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    FileName = $"Auditoria_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    DefaultExt = "xlsx"
                };
                if (dialog.ShowDialog() != true) return;

                var lista = (dgAuditoria.ItemsSource as List<RegistroAuditoria>) ?? new List<RegistroAuditoria>();

                using var workbook = new XLWorkbook();
                var hoja = workbook.Worksheets.Add("Auditoría");

                string[] columnas = { "Fecha y hora", "Usuario", "Acción", "Módulo", "ID Entidad", "Descripción" };
                for (int i = 0; i < columnas.Length; i++)
                    hoja.Cell(1, i + 1).Value = columnas[i];

                var enc = hoja.Range(1, 1, 1, columnas.Length);
                enc.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
                enc.Style.Font.FontColor = XLColor.White;
                enc.Style.Font.Bold = true;

                for (int i = 0; i < lista.Count; i++)
                {
                    int fila = i + 2;
                    var r = lista[i];
                    hoja.Cell(fila, 1).Value = r.FechaHora.ToString("dd/MM/yyyy HH:mm:ss");
                    hoja.Cell(fila, 2).Value = r.Usuario;
                    hoja.Cell(fila, 3).Value = r.Accion;
                    hoja.Cell(fila, 4).Value = r.Modulo;
                    hoja.Cell(fila, 5).Value = r.EntidadId?.ToString() ?? "";
                    hoja.Cell(fila, 6).Value = r.Descripcion ?? "";

                    if (i % 2 == 1)
                        hoja.Row(fila).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F9");
                }

                hoja.Columns().AdjustToContents();
                hoja.PageSetup.Footer.Right.AddText($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
                workbook.SaveAs(dialog.FileName);

                MessageBox.Show("Historial exportado exitosamente.", "Exportación Completada",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
