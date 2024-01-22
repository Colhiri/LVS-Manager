﻿using Autodesk.AutoCAD.DatabaseServices;
using System.Windows;
using System.Windows.Controls;
using static AutoCAD_2022_Plugin1.Working_functions;


namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Interaction logic for ManageLayoutViewport.xaml
    /// </summary>
    public partial class ManageLayoutViewport : Window
    {
        private ManageLayoutVM manageData;

        public ManageLayoutViewport(ManageLayoutVM manageData)
        {
            InitializeComponent();
            this.manageData = manageData;
            this.DataContext = manageData;
        }

        private void btnDoneLayout_Click(object sender, RoutedEventArgs e)
        {
            if (manageData.IsValidName)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnDoneViewport_Click(object sender, RoutedEventArgs e)
        {
            manageData.AnnotationScaleObjectsVP = CheckScaleForStackPanel();
        }

        private void btnDeleteLayout_Click(object sender, RoutedEventArgs e)
        {
            FL.DeleteField(Layouts.SelectedItem.ToString());
        }

        private void btnDeleteViewport_Click(object sender, RoutedEventArgs e)
        {
            Field field = FL.GetField(Layouts.SelectedItem.ToString());
            field.DeleteViewport(ViewportsInField.SelectedItem.ToString());
        }

        private void btnZoomViewport_Click(object sender, RoutedEventArgs e)
        {
            Field field = FL.GetField(Layouts.SelectedItem.ToString());
            ViewportInField vp = field.GetViewport(ViewportsInField.SelectedItem.ToString());
            ObjectIdCollection objectVP = vp.ObjectsIDs;
            ZoomToObjects(objectVP);
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private string CheckScaleForStackPanel()
        {
            // Проверяем выбранное значение, чтобы там не было StackPanel
            string currentValue = Scales.SelectedItem.ToString();
            if (currentValue == "System.Windows.Controls.StackPanel") currentValue = ScaleTextBox.Text;
            return currentValue;
        }

        private void Plotters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ViewportsInField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewportsInField.SelectedItem.ToString();
        }

        private void Layouts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}