using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Notalegis.Views
{
    public class ColumnaExportable
    {
        public string Nombre { get; set; }
        public bool Seleccionada { get; set; } = true;
    }

    public partial class SeleccionarColumnasDialog : Window
    {
        private List<ColumnaExportable> columnas;
        public List<string> ColumnasSeleccionadas { get; private set; }
        public bool Confirmado { get; private set; }

        public SeleccionarColumnasDialog(IEnumerable<string> nombresColumnas)
        {
            InitializeComponent();
            columnas = nombresColumnas.Select(n => new ColumnaExportable { Nombre = n }).ToList();
            itemsColumnas.ItemsSource = columnas;
        }

        private void btnSelTodo_Click(object sender, RoutedEventArgs e)
        {
            bool todasSeleccionadas = columnas.All(c => c.Seleccionada);
            foreach (var c in columnas)
                c.Seleccionada = !todasSeleccionadas;
            itemsColumnas.Items.Refresh();
        }

        private void btnExportar_Click(object sender, RoutedEventArgs e)
        {
            ColumnasSeleccionadas = columnas.Where(c => c.Seleccionada).Select(c => c.Nombre).ToList();
            if (ColumnasSeleccionadas.Count == 0)
            {
                MessageBox.Show("Seleccione al menos una columna para exportar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Confirmado = true;
            DialogResult = true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            DialogResult = false;
        }
    }
}
