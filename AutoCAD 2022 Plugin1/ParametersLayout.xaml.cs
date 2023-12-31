using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static AutoCAD_2022_Plugin1.Working_functions;

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Interaction logic for ParametersLayout.xaml
    /// </summary>
    public partial class ParametersLayout : Window
    {
        TemporaryDataWPF tempData;

        public ParametersLayout(TemporaryDataWPF tempData)
        {
            InitializeComponent();
            this.tempData = tempData;
            this.DataContext = tempData;
            Plotters.ItemsSource = GetPlotters();
            Formats.ItemsSource = GetAllCanonicalScales(tempData.PlotterName);
            Scales.ItemsSource = GetAllAnnotationScales();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            if (tempData.IsValidName)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Plotters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Formats.ItemsSource = GetAllCanonicalScales(tempData.PlotterName);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FL.Contains(NameLayoutTextBox.Text))
            {
                Plotters.SelectedValue = FL.GetPlotter(NameLayoutTextBox.Text);
                Formats.SelectedValue = FL.GetFormat(NameLayoutTextBox.Text);

                Plotters.IsEnabled = false;
                Formats.IsEnabled = false;
            }
            else
            {
                Plotters.IsEnabled = true;
                Formats.IsEnabled = true;
            }
        }
    }
}
