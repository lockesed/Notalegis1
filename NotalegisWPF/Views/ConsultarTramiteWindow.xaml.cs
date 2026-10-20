using Notalegis.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Notalegis.Views
{
    public partial class ConsultarTramiteWindow : Window
    {
        private Tramite tramiteConsultado;

        public ConsultarTramiteWindow(Tramite tramite)
        {
            InitializeComponent();
            
            if (tramite != null)
            {
                tramiteConsultado = tramite;
                CargarDatosTramite(tramite);
                Title = $"Consultar Trámite - {tramite.TipoTramite}";
            }
        }

        private void CargarDatosTramite(Tramite tramite)
        {
            // Cargar información del cliente
            txtCliente.Text = tramite.Cliente;

            // Cargar información del trámite
            txtTipoTramite.Text = tramite.TipoTramite;
            txtNumeroExpediente.Text = string.IsNullOrWhiteSpace(tramite.NumeroExpediente) 
                ? "No asignado" 
                : tramite.NumeroExpediente;

            // Cargar fechas
            dpFechaCreacion.SelectedDate = tramite.FechaCreacion;
            dpFechaInicio.SelectedDate = tramite.FechaInicio;
            dpFechaVencimiento.SelectedDate = tramite.FechaVencimiento;

            // Calcular días restantes
            CalcularDiasRestantes(tramite.FechaVencimiento);

            // Cargar estado y prioridad
            foreach (ComboBoxItem item in cmbEstado.Items)
            {
                if ((item.Content as string) == tramite.Estado)
                {
                    cmbEstado.SelectedItem = item;
                    break;
                }
            }
            foreach (ComboBoxItem item in cmbPrioridad.Items)
            {
                if ((item.Content as string) == tramite.Prioridad)
                {
                    cmbPrioridad.SelectedItem = item;
                    break;
                }
            }

            // Cargar documentación y monto
            txtDocumentacionCompleta.Text = tramite.DocumentacionCompleta ? "Sí" : "No";
            txtMontoEstimado.Text = tramite.MontoEstimado.HasValue 
                ? tramite.MontoEstimado.Value.ToString("N2") 
                : string.Empty;

            // Cargar observaciones
            txtObservaciones.Text = tramite.Observaciones ?? string.Empty;

            // Cargar información del sistema
            txtIdTramite.Text = $"#{tramite.Id.ToString().PadLeft(6, '0')}";
            CalcularTiempoTranscurrido(tramite.FechaCreacion);
            CalcularProgreso(tramite);

            // Aplicar estilos según el estado, prioridad y documentación
            AplicarEstilosSegunDatos(tramite.Estado, tramite.Prioridad, tramite.DocumentacionCompleta);
        }

        private void CalcularDiasRestantes(DateTime fechaVencimiento)
        {
            var diasRestantes = (fechaVencimiento - DateTime.Today).Days;
            
            if (diasRestantes < 0)
            {
                txtDiasRestantes.Text = $"Vencido hace {Math.Abs(diasRestantes)} días";
                borderDiasRestantes.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Rojo claro
                txtDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(183, 28, 28)); // Rojo oscuro
                iconDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rojo
                iconDiasRestantes.Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircle;
            }
            else if (diasRestantes == 0)
            {
                txtDiasRestantes.Text = "Vence hoy";
                borderDiasRestantes.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)); // Naranja claro
                txtDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0)); // Naranja oscuro
                iconDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Naranja
                iconDiasRestantes.Kind = MaterialDesignThemes.Wpf.PackIconKind.Clock;
            }
            else if (diasRestantes <= 7)
            {
                txtDiasRestantes.Text = $"Vence en {diasRestantes} días";
                borderDiasRestantes.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)); // Naranja claro
                txtDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0)); // Naranja oscuro
                iconDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Naranja
            }
            else
            {
                txtDiasRestantes.Text = $"Vence en {diasRestantes} días";
                borderDiasRestantes.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Verde claro
                txtDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Verde oscuro
                iconDiasRestantes.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Verde
            }
        }

        private void CalcularTiempoTranscurrido(DateTime fechaCreacion)
        {
            var tiempoTranscurrido = DateTime.Now - fechaCreacion;

            if (tiempoTranscurrido.TotalDays >= 365)
            {
                int anios = (int)(tiempoTranscurrido.TotalDays / 365);
                txtTiempoTranscurrido.Text = $"{anios} año{(anios > 1 ? "s" : "")}";
            }
            else if (tiempoTranscurrido.TotalDays >= 30)
            {
                int meses = (int)(tiempoTranscurrido.TotalDays / 30);
                txtTiempoTranscurrido.Text = $"{meses} mes{(meses > 1 ? "es" : "")}";
            }
            else if (tiempoTranscurrido.TotalDays >= 1)
            {
                int dias = (int)tiempoTranscurrido.TotalDays;
                txtTiempoTranscurrido.Text = $"{dias} día{(dias > 1 ? "s" : "")}";
            }
            else if (tiempoTranscurrido.TotalHours >= 1)
            {
                int horas = (int)tiempoTranscurrido.TotalHours;
                txtTiempoTranscurrido.Text = $"{horas} hora{(horas > 1 ? "s" : "")}";
            }
            else
            {
                int minutos = (int)tiempoTranscurrido.TotalMinutes;
                txtTiempoTranscurrido.Text = $"{minutos} minuto{(minutos > 1 ? "s" : "")}";
            }
        }

        private void CalcularProgreso(Tramite tramite)
        {
            double progreso = 0;
            string descripcion = "";

            switch (tramite.Estado?.ToLower())
            {
                case "pendiente":
                    progreso = 0.1;
                    descripcion = "10% - Trámite iniciado";
                    break;
                case "en proceso":
                    progreso = 0.5;
                    descripcion = "50% - En desarrollo";
                    break;
                case "documentación incompleta":
                    progreso = 0.3;
                    descripcion = "30% - Esperando documentos";
                    break;
                case "esperando cliente":
                    progreso = 0.7;
                    descripcion = "70% - Esperando respuesta del cliente";
                    break;
                case "finalizado":
                    progreso = 1.0;
                    descripcion = "100% - Completado";
                    break;
                case "vencido":
                case "cancelado":
                    progreso = 0;
                    descripcion = tramite.Estado == "vencido" ? "0% - Vencido" : "0% - Cancelado";
                    break;
                default:
                    progreso = 0.2;
                    descripcion = "20% - Estado indefinido";
                    break;
            }

            progressTramite.Value = progreso * 100;
            txtProgresoDescripcion.Text = descripcion;

            // Cambiar color de la barra de progreso según el estado
            if (tramite.Estado?.ToLower() == "finalizado")
            {
                progressTramite.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Verde
            }
            else if (tramite.Estado?.ToLower() == "vencido" || tramite.Estado?.ToLower() == "cancelado")
            {
                progressTramite.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Rojo
            }
            else
            {
                progressTramite.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Azul
            }
        }

        private void AplicarEstilosSegunDatos(string estado, string prioridad, bool documentacionCompleta)
        {
            // Aplicar estilo según el estado
            switch (estado?.ToLower())
            {
                case "pendiente":
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    borderEstado.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                    txtEstadoIndicador.Text = "Pendiente";
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(97, 97, 97));
                    iconEstado.Foreground = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    break;

                case "en proceso":
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(13, 71, 161));
                    borderEstado.Background = new SolidColorBrush(Color.FromRgb(227, 242, 253));
                    txtEstadoIndicador.Text = "En Proceso";
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(13, 71, 161));
                    iconEstado.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                    break;

                case "finalizado":
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                    borderEstado.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                    txtEstadoIndicador.Text = "Finalizado";
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                    iconEstado.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    break;

                case "vencido":
                case "cancelado":
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(183, 28, 28));
                    borderEstado.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238));
                    txtEstadoIndicador.Text = estado == "vencido" ? "Vencido" : "Cancelado";
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(183, 28, 28));
                    iconEstado.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                    break;

                default:
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0));
                    borderEstado.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224));
                    txtEstadoIndicador.Text = estado ?? "Sin Estado";
                    txtEstadoIndicador.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0));
                    iconEstado.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    break;
            }

            // Aplicar estilo según la prioridad
            switch (prioridad?.ToLower())
            {
                case "baja":
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    borderPrioridad.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                    txtPrioridadIndicador.Text = "Baja";
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                    iconPrioridad.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                    break;

                case "media":
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    borderPrioridad.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224));
                    txtPrioridadIndicador.Text = "Media";
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0));
                    iconPrioridad.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                    break;

                case "alta":
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                    borderPrioridad.Background = new SolidColorBrush(Color.FromRgb(255, 235, 238));
                    txtPrioridadIndicador.Text = "Alta";
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(183, 28, 28));
                    iconPrioridad.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
                    break;

                case "urgente":
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(156, 39, 176));
                    borderPrioridad.Background = new SolidColorBrush(Color.FromRgb(243, 229, 245));
                    txtPrioridadIndicador.Text = "Urgente";
                    txtPrioridadIndicador.Foreground = new SolidColorBrush(Color.FromRgb(106, 27, 154));
                    iconPrioridad.Foreground = new SolidColorBrush(Color.FromRgb(156, 39, 176));
                    break;
            }

            // Aplicar estilo según documentación
            if (documentacionCompleta)
            {
                txtDocumentacionCompleta.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                borderDocumentacion.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233));
                txtDocumentacionIndicador.Text = "Documentación Completa";
                txtDocumentacionIndicador.Foreground = new SolidColorBrush(Color.FromRgb(46, 125, 50));
                iconDocumentacion.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                iconDocumentacion.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckCircle;
            }
            else
            {
                txtDocumentacionCompleta.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0));
                borderDocumentacion.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224));
                txtDocumentacionIndicador.Text = "Documentación Incompleta";
                txtDocumentacionIndicador.Foreground = new SolidColorBrush(Color.FromRgb(230, 81, 0));
                iconDocumentacion.Foreground = new SolidColorBrush(Color.FromRgb(255, 152, 0));
                iconDocumentacion.Kind = MaterialDesignThemes.Wpf.PackIconKind.AlertCircle;
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (tramiteConsultado == null) return;

            var flowDoc = new System.Windows.Documents.FlowDocument
            {
                PageWidth = 794, // ancho aproximado de una hoja A4 a 96 DPI
                PagePadding = new Thickness(60)
            };

            flowDoc.Blocks.Add(new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run("NOTALEGIS — FICHA DE TRÁMITE"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });

            flowDoc.Blocks.Add(new System.Windows.Documents.Paragraph(
                new System.Windows.Documents.Run($"Emitido el {DateTime.Now:dd/MM/yyyy HH:mm}"))
            {
                FontSize = 10,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 24)
            });

            var campos = new (string, string)[]
            {
                ("ID:", $"#{tramiteConsultado.Id.ToString().PadLeft(6, '0')}"),
                ("Cliente:", tramiteConsultado.Cliente),
                ("Tipo de Trámite:", tramiteConsultado.TipoTramite),
                ("N° Expediente:", string.IsNullOrWhiteSpace(tramiteConsultado.NumeroExpediente) ? "No asignado" : tramiteConsultado.NumeroExpediente),
                ("Fecha de Creación:", tramiteConsultado.FechaCreacion.ToString("dd/MM/yyyy")),
                ("Fecha de Inicio:", tramiteConsultado.FechaInicio.ToString("dd/MM/yyyy")),
                ("Fecha de Vencimiento:", tramiteConsultado.FechaVencimiento.ToString("dd/MM/yyyy")),
                ("Estado:", tramiteConsultado.Estado),
                ("Prioridad:", tramiteConsultado.Prioridad),
                ("Documentación:", tramiteConsultado.DocumentacionCompleta ? "Completa" : "Incompleta"),
                ("Monto Estimado:", tramiteConsultado.MontoEstimado.HasValue ? $"$ {tramiteConsultado.MontoEstimado.Value:N2}" : "No especificado"),
                ("Observaciones:", string.IsNullOrWhiteSpace(tramiteConsultado.Observaciones) ? "—" : tramiteConsultado.Observaciones)
            };

            foreach (var (label, valor) in campos)
            {
                var p = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 4, 0, 4) };
                p.Inlines.Add(new System.Windows.Documents.Bold(new System.Windows.Documents.Run(label + "  ")));
                p.Inlines.Add(new System.Windows.Documents.Run(valor));
                flowDoc.Blocks.Add(p);
            }

            var visor = new System.Windows.Controls.FlowDocumentReader
            {
                Document = flowDoc,
                ViewingMode = System.Windows.Controls.FlowDocumentReaderViewingMode.Page
            };

            var ventanaPreview = new Window
            {
                Title = "Vista previa — Ficha de Trámite",
                Content = visor,
                Width = 850,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            ventanaPreview.ShowDialog();
        }
    }
}
