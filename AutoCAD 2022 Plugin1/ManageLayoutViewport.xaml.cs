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

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Interaction logic for ManageLayoutViewport.xaml
    /// </summary>
    public partial class ManageLayoutViewport : Window
    {
        private ManageData manageData;

        public ManageLayoutViewport(ManageData manageData)
        {
            InitializeComponent();
            this.manageData = manageData;
            this.DataContext = manageData;
        }

        private void btnDoneLayout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteLayout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteViewport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDoneViewport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnZoomViewport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Plotters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
