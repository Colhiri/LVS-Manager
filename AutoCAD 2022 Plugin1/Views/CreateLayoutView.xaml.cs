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
            this._Data = _Data;
            this.DataContext = _Data;
        }
        public void DoneClose(object obj, RoutedEventArgs args)
        {
            this.DialogResult = true;
            this.Close();
        }

        public void Cancel(object obj, RoutedEventArgs args)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
