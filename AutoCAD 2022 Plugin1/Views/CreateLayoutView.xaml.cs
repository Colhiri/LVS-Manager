using AutoCAD_2022_Plugin1.ViewModels;
using System.Windows;

namespace AutoCAD_2022_Plugin1.Views
{
    /// <summary>
    /// Interaction logic for CreateLayoutView.xaml
    /// </summary>
    public partial class CreateLayoutView : Window
    {
        private CreateLayoutVM _Data;
        public CreateLayoutView(CreateLayoutVM _Data)
        {
            InitializeComponent();
            this.DataContext = _Data;
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
