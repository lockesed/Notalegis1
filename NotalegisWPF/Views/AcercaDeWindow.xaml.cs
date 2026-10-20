using System.Windows;

namespace Notalegis.Views
{
    public partial class AcercaDeWindow : Window
    {
        public AcercaDeWindow()
        {
            InitializeComponent();
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
