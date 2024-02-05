using AutoCAD_2022_Plugin1.ViewModels;
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
            this.DataContext = _Data;
        }
    }
}
