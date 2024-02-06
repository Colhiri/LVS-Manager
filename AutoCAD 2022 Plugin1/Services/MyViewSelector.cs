using System.Windows.Controls;
using System.Windows;
using AutoCAD_2022_Plugin1.ViewModels.ManageVM;

namespace AutoCAD_2022_Plugin1.Services
{
    public class MyViewSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object inItem, DependencyObject container)
        {
            var checking = (inItem as DummyViewModel).ViewModelTab;

            if (checking == null)
            {
                return LayoutTemplate;
            }
            if (checking is ManageLayoutVM)
            {
                return LayoutTemplate;
            }
            if (checking is ManageVIewportVM)
            {
                return ViewportTemplate;
            }
            return LayoutTemplate;
        }

        public DataTemplate LayoutTemplate { get; set; }
        public DataTemplate ViewportTemplate { get; set; }

    }
}
