using System;
using System.Windows.Input;

namespace AutoCAD_2022_Plugin1.Services
{
    public class RelayCommand : ICommand
    {
        private Action<object> _Execute;
        private Func<object, bool> _CanExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> _Execute, Func<object, bool> _CanExecute)
        {
            this._Execute = _Execute;
            this._CanExecute = _CanExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _CanExecute == null || _CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _Execute(parameter);
        }
    }
}
