using AutoCAD_2022_Plugin1.ViewModels.ManageVM;
using System.Reflection;
using System.Windows;

namespace AutoCAD_2022_Plugin1.Views.ManageViews
{
    /// <summary>
    /// Логика взаимодействия для MainManageWindow.xaml
    /// </summary>
    public partial class MainManageWindow : Window
    {
        private MainManageVM _Data;
        public MainManageWindow(MainManageVM _Data)
        {
            InitializeComponent();
            this.DataContext = _Data;
            //TABCONT.ItemsSource = _Data.tabs;
        }

        public void DoneCommand(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        public void CancelCommand(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
