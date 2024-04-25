using AutoCAD_2022_Plugin1.Services;
using AutoCAD_2022_Plugin1.ViewModels.ManageVM;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) 
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
