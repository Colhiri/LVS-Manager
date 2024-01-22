﻿using System.Windows;
using System.Windows.Controls;
using static AutoCAD_2022_Plugin1.Working_functions;

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Interaction logic for ParametersLayout.xaml
    /// </summary>
    public partial class ParametersLayout : Window
    {
        private LayoutData tempData;

        public ParametersLayout(LayoutData tempData)
        {
            InitializeComponent();
            this.tempData = tempData;
            this.DataContext = tempData;

            // Работа со списком имен МАКЕТОВ с возможностью введения значений
            var stackPanelNames = NamesLayoutComboBox.Items[0];
            NamesLayoutComboBox.Items.Clear();
            NamesLayoutComboBox.Items.Add(stackPanelNames);
            foreach (string name in FL.GetNames()) NamesLayoutComboBox.Items.Add(name);

            // Работа со списком имен МАСШТАБОВ с возможностью введения значений
            var stackPanelScales = Scales.Items[0];
            Scales.Items.Clear();
            Scales.Items.Add(stackPanelScales);
            foreach (string name in GetAllAnnotationScales()) Scales.Items.Add(name);

            Plotters.ItemsSource = GetPlotters();
            Formats.ItemsSource = GetAllCanonicalScales(tempData.PlotterName);
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем имя на соответствие
            tempData.Name = CheckNameForStackPanel();
            tempData.AnnotationScaleObjectsVP = CheckScaleForStackPanel();

            if (tempData.IsValidName && tempData.IsValidScale)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private string CheckNameForStackPanel()
        {
            // Проверяем выбранное значение, чтобы там не было StackPanel
            string currentValue = NamesLayoutComboBox.SelectedItem.ToString();
            if (currentValue == "System.Windows.Controls.StackPanel") currentValue = NamesLayoutTextBox.Text;
            return currentValue;
        }

        private string CheckScaleForStackPanel()
        {
            // Проверяем выбранное значение, чтобы там не было StackPanel
            string currentValue = Scales.SelectedItem.ToString();
            if (currentValue == "System.Windows.Controls.StackPanel") currentValue = ScaleTextBox.Text;
            return currentValue;
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

        private void NamesLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FL.Contains(CheckNameForStackPanel()))
            {
                Plotters.SelectedValue = FL.GetPlotter(NamesLayoutComboBox.SelectedItem.ToString());
                Formats.SelectedValue = FL.GetFormat(NamesLayoutComboBox.SelectedItem.ToString());

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