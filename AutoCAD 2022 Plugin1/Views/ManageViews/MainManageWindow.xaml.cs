using AutoCAD_2022_Plugin1.ViewModels.ManageVM;
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
        }
    }
}
