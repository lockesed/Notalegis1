using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Notalegis.Views
{
    public partial class SeleccionMesWindow : Window
    {
        public int MesSeleccionado { get; private set; }
        public int AnioSeleccionado { get; private set; }
        public string NombreMes { get; private set; }

        public SeleccionMesWindow()
        {
            InitializeComponent();
            CargarMeses();
            CargarAnios();
        }

        private void CargarMeses()
        {
            var meses = new List<MesItem>();
            var cultura = new CultureInfo("es-ES");

            for (int i = 1; i <= 12; i++)
            {
                string nombreMes = cultura.DateTimeFormat.GetMonthName(i);
                nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1);
                meses.Add(new MesItem { Numero = i, Nombre = nombreMes });
            }

            cmbMes.ItemsSource = meses;
            cmbMes.DisplayMemberPath = "Nombre";
            cmbMes.SelectedValuePath = "Numero";
            cmbMes.SelectedIndex = DateTime.Now.Month - 1;
        }

        private void CargarAnios()
        {
            var anios = new List<int>();
            int anioActual = DateTime.Now.Year;

            for (int i = anioActual - 5; i <= anioActual + 1; i++)
            {
                anios.Add(i);
            }

            cmbAnio.ItemsSource = anios;
            cmbAnio.SelectedItem = anioActual;
        }

        private void btnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbMes.SelectedItem == null || cmbAnio.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un mes y un año.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mesItem = cmbMes.SelectedItem as MesItem;
            MesSeleccionado = mesItem.Numero;
            NombreMes = mesItem.Nombre;
            AnioSeleccionado = (int)cmbAnio.SelectedItem;
            
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private class MesItem
        {
            public int Numero { get; set; }
            public string Nombre { get; set; }
        }
    }
}
