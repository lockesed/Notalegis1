using Notalegis.Datos.Modelos;
using Notalegis.Negocio.Servicios;
using Notalegis.Models;
using System.Windows;
using System.Windows.Controls;
using Xceed.Words.NET;
using Microsoft.Win32;
using System.Printing;
using System.Windows.Documents;
using ClosedXML.Excel;
using Notalegis.Datos.Repositorios;
using Notalegis.Views;
using System.Text.RegularExpressions;
using System.Windows.Input; 




namespace NotalegisWPF.Views
{
    public partial class ClientesView : Window
    {
        private readonly ClienteService clienteService = new ClienteService();
        private List<Cliente> _todosLosClientes = new List<Cliente>();
        private List<Cliente> _clientesFiltrados = new List<Cliente>();
        private int _paginaActual = 1;
        private const int TamanoPagina = 50;

        public ClientesView()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            _todosLosClientes = clienteService.ObtenerTodos();
            _paginaActual = 1;
            AplicarFiltroYPaginar();
        }

        private void AplicarFiltroYPaginar()
        {
            string textoBuscar = txtBuscar?.Text?.Trim().ToLower() ?? "";
            string estadoSel = (cmbFiltroEstado?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";

            _clientesFiltrados = _todosLosClientes.Where(c =>
            {
                string nombre = c.Nombre?.ToLower() ?? "";
                string apellido = c.Apellido?.ToLower() ?? "";
                string dni = c.DNI?.ToLower() ?? "";
                bool textoOk = string.IsNullOrEmpty(textoBuscar)
                               || nombre.Contains(textoBuscar)
                               || apellido.Contains(textoBuscar)
                               || $"{nombre} {apellido}".Contains(textoBuscar)
                               || dni.Contains(textoBuscar);
                bool estadoOk = estadoSel == "Todos" || c.Estado == estadoSel;
                return textoOk && estadoOk;
            }).ToList();

            int totalPaginas = Math.Max(1, (int)Math.Ceiling(_clientesFiltrados.Count / (double)TamanoPagina));
            if (_paginaActual > totalPaginas) _paginaActual = totalPaginas;

            var pagina = _clientesFiltrados
                .Skip((_paginaActual - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToList();

            dgvClientes.ItemsSource = pagina;
            borderSinClientes.Visibility = _clientesFiltrados.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            txtInfoPagina.Text = $"Página {_paginaActual} de {totalPaginas}  ({_clientesFiltrados.Count} registros)";
            btnPaginaAnterior.IsEnabled = _paginaActual > 1;
            btnPaginaSiguiente.IsEnabled = _paginaActual < totalPaginas;
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validación: Nombre obligatorio y solo letras (con tildes y espacios)
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtNombre.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El nombre solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Apellido obligatorio y solo letras (con tildes y espacios)
            if (string.IsNullOrWhiteSpace(txtApellido.Text))
            {
                MessageBox.Show("El apellido es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtApellido.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageBox.Show("El apellido solo debe contener letras.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: DNI obligatorio y solo números
            if (string.IsNullOrWhiteSpace(txtDocumento.Text))
            {
                MessageBox.Show("El DNI es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!txtDocumento.Text.All(char.IsDigit))
            {
                MessageBox.Show("El DNI solo debe contener números.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (txtDocumento.Text.Trim().Length < 7 || txtDocumento.Text.Trim().Length > 11)
            {
                MessageBox.Show("El DNI/CUIT debe tener entre 7 y 11 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (clienteService.ExisteDNI(txtDocumento.Text.Trim()))
            {
                MessageBox.Show("Ya existe un cliente registrado con ese DNI/CUIT.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Teléfono obligatorio y solo números con caracteres especiales permitidos (+, -, espacios, paréntesis)
            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MessageBox.Show("El teléfono es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtTelefono.Text, @"^[0-9+\-\s()]+$"))
            {
                MessageBox.Show("El teléfono solo debe contener números y los caracteres: + - ( ) espacios", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int cantidadDigitosTelefono = txtTelefono.Text.Count(char.IsDigit);
            if (cantidadDigitosTelefono < 6 || cantidadDigitosTelefono > 15)
            {
                MessageBox.Show("El teléfono debe tener entre 6 y 15 dígitos.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Email obligatorio y debe contener @
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("El email es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtEmail.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("El email debe tener un formato válido (ejemplo@ejemplo.com).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Dirección obligatoria y debe contener letras y números
            if (string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MessageBox.Show("La dirección es obligatoria.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!Regex.IsMatch(txtDireccion.Text, @"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s,.#-]+$"))
            {
                MessageBox.Show("La dirección solo debe contener letras, números y caracteres básicos (. , # -).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validación: Estado obligatorio
            if (string.IsNullOrWhiteSpace(cmbEstado.Text))
            {
                MessageBox.Show("Debe seleccionar un estado.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var cliente = new Cliente
            {
                Nombre = txtNombre.Text,
                Apellido = txtApellido.Text,
                DNI = txtDocumento.Text,
                Telefono = txtTelefono.Text,
                Email = txtEmail.Text,
                Direccion = txtDireccion.Text,
                Estado = cmbEstado.Text,
                Observaciones = txtObservaciones.Text.Trim()
            };

            clienteService.GuardarCliente(cliente, SesionActual.NombreUsuario);
            MessageBox.Show("Cliente guardado correctamente.");
            CargarClientes();
            LimpiarFormulario();
        }

        private void FiltrarClientes()
        {
            _paginaActual = 1;
            AplicarFiltroYPaginar();
        }

        private void ClientesView_Loaded(object sender, RoutedEventArgs e)
        {
            cmbFiltroEstado.SelectedIndex = 0;
            CargarClientes();
        }
        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarClientes();
        }

        private void cmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarClientes();
        }
        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtDocumento.Text = "";
            txtTelefono.Text = "";
            txtEmail.Text = "";
            txtDireccion.Text = "";
            cmbEstado.SelectedIndex = -1;
            txtObservaciones.Text = "";
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                TurnoDAO turnoDAO = new TurnoDAO();
                bool tieneTurnos = turnoDAO.ExisteTurnoParaCliente(cliente.Id);

                if (tieneTurnos)
                {
                    MessageBox.Show("No se puede eliminar el cliente porque tiene turnos asociados.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"¿Estás seguro que querés eliminar a {cliente.Nombre}?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    clienteService.EliminarCliente(cliente.Id, SesionActual.NombreUsuario);
                    MessageBox.Show("Cliente eliminado correctamente.");
                    CargarClientes();
                }
            }
        }

        private void btnConsultar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente clienteSeleccionado)
            {
                var ventana = new ConsultarClienteWindow(clienteSeleccionado);
                ventana.ShowDialog();
            }
            else
            {
                MessageBox.Show("Seleccioná un cliente primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                var ventanaEditar = new EditarClienteWindow(cliente);
                ventanaEditar.ShowDialog();
                CargarClientes(); 
            }
            else
            {
                MessageBox.Show("Seleccioná un cliente para editar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDocumentos_Click(object sender, RoutedEventArgs e)
        {
            if (dgvClientes.SelectedItem is Cliente cliente)
            {
                string nombreCarpeta = $"{cliente.Id}_{cliente.Nombre}{cliente.Apellido}".Replace(" ", "");
                var ventana = new GestionDocumentosWindow("Clientes", nombreCarpeta, $"{cliente.Nombre} {cliente.Apellido}");
                ventana.ShowDialog();
            }
            else
            {
                MessageBox.Show("Seleccione un cliente para ver sus documentos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnGenerarLista_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden exportar el listado completo de clientes.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var todasColumnas = new[] { "ID", "Nombre", "Apellido", "DNI/CUIT", "Teléfono", "Email", "Estado", "Observaciones" };

            var selectorDialog = new SeleccionarColumnasDialog(todasColumnas) { Owner = this };
            if (selectorDialog.ShowDialog() != true) return;

            var colsSel = selectorDialog.ColumnasSeleccionadas;
            var clientes = clienteService.ObtenerTodos();

            var saveDialog = new SaveFileDialog
            {
                Filter = "Archivo Excel (*.xlsx)|*.xlsx",
                FileName = $"ListaClientes_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true) return;

            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Clientes");

            for (int col = 0; col < colsSel.Count; col++)
                hoja.Cell(1, col + 1).Value = colsSel[col];

            var enc = hoja.Range(1, 1, 1, colsSel.Count);
            enc.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
            enc.Style.Font.FontColor = XLColor.White;
            enc.Style.Font.Bold = true;
            enc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var mapaValor = new System.Collections.Generic.Dictionary<string, System.Func<Notalegis.Datos.Modelos.Cliente, object>>
            {
                ["ID"]            = c => c.Id,
                ["Nombre"]        = c => c.Nombre ?? "",
                ["Apellido"]      = c => c.Apellido ?? "",
                ["DNI/CUIT"]      = c => c.DNI ?? "",
                ["Teléfono"]      = c => c.Telefono ?? "",
                ["Email"]         = c => c.Email ?? "",
                ["Estado"]        = c => c.Estado ?? "",
                ["Observaciones"] = c => c.Observaciones ?? ""
            };

            for (int i = 0; i < clientes.Count; i++)
            {
                int fila = i + 2;
                for (int col = 0; col < colsSel.Count; col++)
                    if (mapaValor.TryGetValue(colsSel[col], out var getter))
                        hoja.Cell(fila, col + 1).Value = getter(clientes[i])?.ToString() ?? "";

                if (i % 2 == 1)
                    hoja.Row(fila).Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F6F9");
            }

            hoja.Columns().AdjustToContents();
            hoja.PageSetup.Footer.Right.AddText($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
            workbook.SaveAs(saveDialog.FileName);
            MessageBox.Show("Lista exportada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            if (MessageBox.Show("¿Desea abrir el archivo?", "Abrir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = saveDialog.FileName, UseShellExecute = true });
        }

        private void btnEmitirLista_Click(object sender, RoutedEventArgs e)
        {
            if (!SesionActual.EsAdministrador)
            {
                MessageBox.Show("Solo los administradores pueden imprimir el listado completo de clientes.",
                                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();
                doc.Blocks.Add(new Paragraph(new Run("Lista de Clientes")) { FontSize = 16, FontWeight = FontWeights.Bold });

                foreach (Cliente cliente in clienteService.ObtenerTodos())
                {
                    doc.Blocks.Add(new Paragraph(new Run(
                        $"{cliente.Nombre} {cliente.Apellido} - DNI: {cliente.DNI} - Tel: {cliente.Telefono} - Estado: {cliente.Estado}")));
                }

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Impresión de Lista de Clientes");
            }
        }


        private void btnVerInactivos_Click(object sender, RoutedEventArgs e)
        {
            var ventanaInactivos = new ClientesInactivosView();
            ventanaInactivos.ShowDialog();
            // Recargar la lista principal por si se reactivó alguno
            CargarClientes(); 
            FiltrarClientes();
        }

        // Validación en tiempo real: Solo letras para Nombre
        private void txtNombre_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]$");
        }

        // Validación en tiempo real: Solo letras para Apellido
        private void txtApellido_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]$");
        }

        // Validación en tiempo real: Solo números para DNI
        private void txtDocumento_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        // Validación en tiempo real: Números y caracteres especiales para Teléfono
        private void txtTelefono_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9+\-\s()]$");
        }

        private void btnPaginaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                AplicarFiltroYPaginar();
            }
        }

        private void btnPaginaSiguiente_Click(object sender, RoutedEventArgs e)
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(_clientesFiltrados.Count / (double)TamanoPagina));
            if (_paginaActual < totalPaginas)
            {
                _paginaActual++;
                AplicarFiltroYPaginar();
            }
        }
    }
}
