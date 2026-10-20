using System.Windows;

namespace Notalegis.Views
{
    public partial class SplashScreenWindow : Window
    {
        public SplashScreenWindow()
        {
            InitializeComponent();
        }

        public void SetEstado(string mensaje)
        {
            txtEstado.Text = mensaje;
        }
    }
}
