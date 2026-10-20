using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;

namespace Notalegis.Views
{
    public partial class ConfiguracionWindow : Window
    {
        private readonly ConfiguracionService confService;
        private List<Configuracion> configuracionesActuales;
        public ConfiguracionWindow()
        {
            InitializeComponent();
            confService = new ConfiguracionService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarConfiguraciones();
        }

        private void CargarConfiguraciones()
        {
            try
            {
                configuracionesActuales = confService.ObtenerTodas();

                if (configuracionesActuales == null || !configuracionesActuales.Any())
                {
                    MessageBox.Show("No se encontraron configuraciones en la base de datos. Por favor, asegúrese de haber ejecutado el script SQL inicial.", 
                                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cargar Textos en UI
                txtEscribaniaNombre.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaNombre);
                txtEscribaniaDireccion.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaDireccion);
                txtEscribaniaTelefono.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaTelefono);
                txtEscribaniaEmail.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaEmail);
                txtEscribaniaCiudad.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaCiudad);
                txtEscribaniaProvincia.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaProvincia);
                txtEscribaniaNumeroRegistro.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaNumeroRegistro);
                txtEscribaniaEscribano.Text = ObtenerValorLocal(ClavesConfiguracion.EscribaniaEscribano);

                txtIva.Text = ObtenerValorLocal(ClavesConfiguracion.ImpuestoIVAPorc);
                txtGanancias.Text = ObtenerValorLocal(ClavesConfiguracion.ImpuestoGananciasPorc);
                txtSellos.Text = ObtenerValorLocal(ClavesConfiguracion.ImpuestoSellosPorc);
                txtIti.Text = ObtenerValorLocal(ClavesConfiguracion.ImpuestoITIPorc);
                txtHonorarios.Text = ObtenerValorLocal(ClavesConfiguracion.HonorariosGeneralesPorc);
                txtTasaGeneral.Text = ObtenerValorLocal(ClavesConfiguracion.ImpuestoTasaGeneralPorc);

                txtWhatsappFirma.Text = ObtenerValorLocal(ClavesConfiguracion.WhatsAppMensajeFirma);
                txtWhatsappSeguimiento.Text = ObtenerValorLocal(ClavesConfiguracion.WhatsAppMensajeSeguimiento);

                // IA
                pwdApiKey.Password = ObtenerValorLocal(ClavesConfiguracion.IAClaudeApiKey);
                txtIAMaxTokens.Text = ObtenerValorLocal(ClavesConfiguracion.IAMaxTokens);
                if (string.IsNullOrWhiteSpace(txtIAMaxTokens.Text)) txtIAMaxTokens.Text = "8192";
                string rutaNormativa = ObtenerValorLocal(ClavesConfiguracion.IARutaNormativaPDF);
                txtRutaNormativaPDF.Text = string.IsNullOrWhiteSpace(rutaNormativa)
                    ? "(ninguno seleccionado)" : rutaNormativa;
                txtPromptBase.Text = ObtenerValorLocal(ClavesConfiguracion.IAPromptBase);
                string modeloGuardado = ObtenerValorLocal(ClavesConfiguracion.IAModelo);
                foreach (System.Windows.Controls.ComboBoxItem item in cmbModeloIA.Items)
                {
                    if (item.Tag?.ToString() == modeloGuardado)
                    {
                        cmbModeloIA.SelectedItem = item;
                        break;
                    }
                }
                if (cmbModeloIA.SelectedItem == null) cmbModeloIA.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la configuración: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ObtenerValorLocal(string clave)
        {
            var conf = configuracionesActuales?.FirstOrDefault(c => c.Clave == clave);
            return conf != null ? conf.Valor : string.Empty;
        }

        private void ActualizarValorEnLista(string clave, string nuevoValor, string tipoConfiguracion = "General", string descripcion = null)
        {
            var conf = configuracionesActuales?.FirstOrDefault(c => c.Clave == clave);
            if (conf != null)
            {
                conf.Valor = nuevoValor;
            }
            else
            {
                if (configuracionesActuales == null) configuracionesActuales = new List<Configuracion>();
                configuracionesActuales.Add(new Configuracion
                {
                    Clave = clave,
                    Valor = nuevoValor,
                    TipoConfiguracion = tipoConfiguracion,
                    Descripcion = descripcion,
                    FechaActualizacion = DateTime.Now
                });
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validación de números
                if (!DecimalValido(txtIva.Text) || !DecimalValido(txtGanancias.Text) || 
                    !DecimalValido(txtSellos.Text) || !DecimalValido(txtIti.Text) || 
                    !DecimalValido(txtHonorarios.Text) || !DecimalValido(txtTasaGeneral.Text))
                {
                    MessageBox.Show("Por favor, verifique que todos los impuestos y tasas sean números válidos (Ejemplo: 21.00 o 21,00).", 
                                    "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Actualizar la lista local con los valores de los TextBox
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaNombre, txtEscribaniaNombre.Text.Trim(), "General", "Nombre completo de la Escribanía");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaDireccion, txtEscribaniaDireccion.Text.Trim(), "General", "Dirección comercial de la Escribanía");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaTelefono, txtEscribaniaTelefono.Text.Trim(), "General", "Teléfono principal (Ej: 549...)");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaEmail, txtEscribaniaEmail.Text.Trim(), "General", "Email de contacto oficial");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaCiudad, txtEscribaniaCiudad.Text.Trim(), "General", "Ciudad de otorgamiento de actos notariales");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaProvincia, txtEscribaniaProvincia.Text.Trim(), "General", "Provincia donde funciona la escribanía");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaNumeroRegistro, txtEscribaniaNumeroRegistro.Text.Trim(), "General", "Número de Registro Notarial");
                ActualizarValorEnLista(ClavesConfiguracion.EscribaniaEscribano, txtEscribaniaEscribano.Text.Trim(), "General", "Nombre del escribano/a titular");

                // Formatear decimales reemplazando coma por punto para la BD si es necesario
                ActualizarValorEnLista(ClavesConfiguracion.ImpuestoIVAPorc, FormatearDecimal(txtIva.Text), "Impuestos", "Porcentaje de IVA por defecto");
                ActualizarValorEnLista(ClavesConfiguracion.ImpuestoGananciasPorc, FormatearDecimal(txtGanancias.Text), "Impuestos", "Porcentaje de retención de Ganancias por defecto");
                ActualizarValorEnLista(ClavesConfiguracion.ImpuestoSellosPorc, FormatearDecimal(txtSellos.Text), "Impuestos", "Porcentaje de Impuesto a los Sellos por defecto");
                ActualizarValorEnLista(ClavesConfiguracion.ImpuestoITIPorc, FormatearDecimal(txtIti.Text), "Impuestos", "Porcentaje de ITI por defecto");
                ActualizarValorEnLista(ClavesConfiguracion.HonorariosGeneralesPorc, FormatearDecimal(txtHonorarios.Text), "Impuestos", "Porcentaje de Honorarios sugerido");
                ActualizarValorEnLista(ClavesConfiguracion.ImpuestoTasaGeneralPorc, FormatearDecimal(txtTasaGeneral.Text), "Impuestos", "Porcentaje de Tasa General por defecto");

                ActualizarValorEnLista(ClavesConfiguracion.WhatsAppMensajeFirma, txtWhatsappFirma.Text.Trim(), "Alertas", "Plantilla para aviso de firma (0=Nombre, 1=Tipo, 2=Venc.)");
                ActualizarValorEnLista(ClavesConfiguracion.WhatsAppMensajeSeguimiento, txtWhatsappSeguimiento.Text.Trim(), "Alertas", "Plantilla para seguimiento (0=Nombre, 1=Tipo, 2=Estado)");

                // IA
                ActualizarValorEnLista(ClavesConfiguracion.IAClaudeApiKey, pwdApiKey.Password.Trim(), "IA", "API Key de Claude (Anthropic)");
                string maxTokensStr = txtIAMaxTokens.Text.Trim();
                if (!int.TryParse(maxTokensStr, out int maxTokensVal) || maxTokensVal < 1024)
                    maxTokensStr = "8192";
                ActualizarValorEnLista(ClavesConfiguracion.IAMaxTokens, maxTokensStr, "IA", "Límite de tokens por respuesta (min. 1024)");
                string rutaNormativaGuardar = txtRutaNormativaPDF.Text.Trim();
                if (rutaNormativaGuardar == "(ninguno seleccionado)") rutaNormativaGuardar = "";
                ActualizarValorEnLista(ClavesConfiguracion.IARutaNormativaPDF, rutaNormativaGuardar, "IA", "Ruta al PDF de normativa vigente del Colegio de Escribanos");
                ActualizarValorEnLista(ClavesConfiguracion.IAPromptBase, txtPromptBase.Text.Trim(), "IA", "Instrucciones específicas de la escribanía para el prompt de IA");
                string modeloSeleccionado = (cmbModeloIA.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "claude-haiku-4-5-20251001";
                ActualizarValorEnLista(ClavesConfiguracion.IAModelo, modeloSeleccionado, "IA", "Modelo de Claude a utilizar");

                // Guardar en Base de Datos
                confService.ActualizarConfiguraciones(configuracionesActuales);

                MessageBox.Show("Las configuraciones se han guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar las modificaciones: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSeleccionarNormativaPDF_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Archivos PDF (*.pdf)|*.pdf",
                Title = "Seleccionar PDF de normativa del Colegio de Escribanos"
            };
            if (dialog.ShowDialog() == true)
                txtRutaNormativaPDF.Text = dialog.FileName;
        }

        private void btnQuitarNormativaPDF_Click(object sender, RoutedEventArgs e)
        {
            txtRutaNormativaPDF.Text = "(ninguno seleccionado)";
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool DecimalValido(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return true; // Puede ser vacío si no aplica
            return decimal.TryParse(texto.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
        }

        private string FormatearDecimal(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "0";
            return texto.Replace(",", ".").Trim();
        }
    }
}
