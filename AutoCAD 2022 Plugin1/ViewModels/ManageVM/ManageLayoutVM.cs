using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageLayoutVM : MainVM, IMyTabContentViewModel
    {
        public ManageLayoutVM(ParametersLVS parameters)
        {
            _LayoutToDelete = new ObservableCollection<string>();
            this.Name = parameters.NameLayout;
            this.PlotterName = parameters.PlotterName;
            this.LayoutFormat = parameters.LayoutFormat;
        }

        #region Properties
        /// Позволяет блокировать Tab Viewport'a
        public bool CheckTabEnabled
        {
            get
            {
                return EnabledFormsParamaters;
            }
        }

        /// Формирует список листов для удаления после закрытия окна
        private ObservableCollection<string> _LayoutToDelete;
        public ObservableCollection<string> LayoutToDelete
        {
            get
            {
                return _LayoutToDelete;
            }
        }

        /// Проверка редактирования некоторых частей View
        public bool EnabledFormsParamaters
        {
            get
            {
                if (Name == null) return false;
                return !LayoutToDelete.Contains(Name);
            }
        }

        public bool InvertEnabledForms 
        { 
            get 
            {
                if (Name == null) return false;
                return !EnabledFormsParamaters;  
            }
        }

        /// Взаимодействие с именами макетов
        private ObservableCollection<string> _NamesLayouts;
        public ObservableCollection<string> NamesLayouts
        {
            get
            {
                _NamesLayouts = new ObservableCollection<string>(CadUtilityLib.FL.GetNames());
                return _NamesLayouts;
            }
        }

        /// Имя поля
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value.Trim();
                _EditName = _Name;
                OnPropertyChanged(nameof(NamesLayouts));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(EditName));
                OnPropertyChanged(nameof(LayoutFormat));
                OnPropertyChanged(nameof(PlotterName));
                OnPropertyChanged(nameof(EnabledFormsParamaters));
                OnPropertyChanged(nameof(InvertEnabledForms));

                OnPropertyChanged(nameof(Plotters));
                OnPropertyChanged(nameof(Formats));
            }
        }
        /// Отредактированное имя поля
        private string _EditName;
        public string EditName
        {
            get
            {
                if (Name == null) return null;
                return _EditName;
            }
            set
            {
                _EditName = value.Trim();
                if (EditName != Name)
                {
                    CadUtilityLib.FL.GetField(Name).SetFieldName(_EditName);
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(NamesLayouts));
                }
            }
        }

        /// Взаимодействие с плоттерами
        private ObservableCollection<string> _Plotters;
        public ObservableCollection<string> Plotters
        {
            get
            {
                if (Name == null) return null;
                _Plotters = new ObservableCollection<string>(CadUtilityLib.GetPlotters());
                return _Plotters;
            }
        }
        /// Имя плоттера
        private string _PlotterName;
        public string PlotterName
        {
            get
            {
                if (Name == null) return null;
                return _PlotterName;
            }
            set
            {
                _PlotterName = value;
                CadUtilityLib.FL.GetField(Name).SetFieldPlotter(_PlotterName);
                OnPropertyChanged(nameof(Formats));
            }
        }

        /// Взаимодействие с форматами
        private ObservableCollection<string> _Formats;
        public ObservableCollection<string> Formats
        {
            get
            {
                if (PlotterName == null) return null;
                _Formats = new ObservableCollection<string>(CadUtilityLib.GetAllCanonicalScales(PlotterName));
                return _Formats;
            }
        }
        /// Формат макета
        private string _LayoutFormat;
        public string LayoutFormat
        {
            get
            {
                if (PlotterName == null) return null;
                return _LayoutFormat;
            }
            set
            {
                _LayoutFormat = value;
                CadUtilityLib.FL.GetField(Name).SetFieldFormat(_LayoutFormat);
            }
        }

        #endregion

        #region Commands
        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete()
        {
            if (Name == null) return;
            _LayoutToDelete.Add(Name);
            OnPropertyChanged(nameof(EnabledFormsParamaters));
            OnPropertyChanged(nameof(InvertEnabledForms));
        }
        private RelayCommand _DeleteCommand;
        public RelayCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                {
                    _DeleteCommand = new RelayCommand(o => AddDelete(), null);
                }
                return _DeleteCommand;
            }
        }

        /// <summary>
        /// Убрать макета или видовой экран из списка на удаление
        /// </summary>
        private void RemoveDelete()
        {
            _LayoutToDelete.Remove(Name);
            OnPropertyChanged(nameof(EnabledFormsParamaters));
            OnPropertyChanged(nameof(InvertEnabledForms));
        }
        private RelayCommand _CancelDeleteCommand;
        public RelayCommand CancelDeleteCommand
        {
            get
            {
                if (_CancelDeleteCommand == null)
                {
                    _CancelDeleteCommand = new RelayCommand(o => RemoveDelete(), null);
                }
                return _CancelDeleteCommand;
            }
        }

        /// <summary>
        /// Применить изменения
        /// </summary>
        private void Apply() 
        {
            foreach (string DeleteNameField in  _LayoutToDelete)
            {
                CadUtilityLib.FL.DeleteField(DeleteNameField);
            }
            _LayoutToDelete.Clear();
            OnPropertyChanged(nameof(NamesLayouts));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(EditName));
            OnPropertyChanged(nameof(LayoutFormat));
            OnPropertyChanged(nameof(PlotterName));
            OnPropertyChanged(nameof(EnabledFormsParamaters));
            OnPropertyChanged(nameof(InvertEnabledForms));

            OnPropertyChanged(nameof(Plotters));
            OnPropertyChanged(nameof(Formats));
            // throw new System.Exception("Сделай применение изменений в макете!");
        }
        private RelayCommand _ApplyCommand;
        public RelayCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new RelayCommand(o => Apply(), null);
                }
                return _ApplyCommand;
            }
        }
        #endregion
    }
}