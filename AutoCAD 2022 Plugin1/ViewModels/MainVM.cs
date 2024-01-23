using AutoCAD_2022_Plugin1.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels
{
    public class MainVM : INotifyPropertyChanged
    {
        private Window window;

        public MainVM(Window window)
        {
            this.window = window;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) 
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Done()
        {
            window.DialogResult = true;
            window.Close();
        }
        private void Cancel()
        {
            window.DialogResult = false;
            window.Close();
        }
        private RelayCommand _DoneCommand;
        public RelayCommand DoneCommand
        {
            get
            {
                if (_DoneCommand == null)
                {
                    _DoneCommand = new RelayCommand(o => Done(), null);
                }
                return _DoneCommand;
            }
        }

        public RelayCommand _CancelCommand;
        public RelayCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(o => Cancel(), null);
                }
                return _CancelCommand;
            }
        }
    }
}
