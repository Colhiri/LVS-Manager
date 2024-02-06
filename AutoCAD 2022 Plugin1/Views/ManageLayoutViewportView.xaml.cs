using AutoCAD_2022_Plugin1.ViewModels.ManageLV;
using System.Windows;


namespace AutoCAD_2022_Plugin1.Views
{
    /// <summary>
    /// Interaction logic for ManageLayoutViewportView.xaml
    /// </summary>
    public partial class ManageLayoutViewportView : Window
    {
        private ManageLayoutViewportVM _Data;
        public ManageLayoutViewportView(ManageLayoutViewportVM _Data)
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
