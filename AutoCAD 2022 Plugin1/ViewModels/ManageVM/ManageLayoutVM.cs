using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using Autodesk.AutoCAD.ApplicationServices;
using System.Collections.ObjectModel;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageLayoutVM : MainVM, IMyTabContentViewModel, IObserver
    {
        private CurrentLayoutObservable obs;
        private ManageLayoutModel Model;
        public ManageLayoutVM(ManageLayoutModel Model, CurrentLayoutObservable obs)
        {
            this.Model = Model;
            this.obs = obs;
            this.obs.AddObserver(this);
            this._Plotters = Model.GetPlotters();
        }

        #region Properties
        /// Проверка корректности имени
        private bool _ApplyButtonEnabled;
        public bool ApplyButtonEnabled
        {
            get
            {
                _ApplyButtonEnabled = Model.IsValidName(Name);
                if (_ApplyButtonEnabled == false)
                {
                    Application.ShowAlertDialog("Введено неправильное имя макета! Исправьте!");
                }
                return _ApplyButtonEnabled;
            }
        }

        /// Позволяет блокировать Tab Viewport'a
        public bool CheckTabEnabled
        {
            get
            {
                return EnabledFormsParamaters;
            }
        }

        /// Проверка редактирования некоторых частей View
        public bool EnabledFormsParamaters
        {
            get
            {
                if (Name == null) return false;
                return Model.CheckToDelete();
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
                _NamesLayouts = Model.GetNames();
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

                obs.UpdateCurrent(Name);

                OnPropertyChanged(nameof(NamesLayouts));
                OnPropertyChanged(nameof(ApplyButtonEnabled));
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
                _Formats = Model.GetFormats();
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
            //throw new System.Exception("Сделай удаление");
            Model.SetDelete();
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
            Model.RemoveDelete();
            //throw new System.Exception("Сделай отмену удаления");
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

        public void Update()
        {
            ManageLayout Current = Model.GetCurrentLayout(Name);
            _Name = Current.Name;
            _PlotterName = Current.Plotter;
            _LayoutFormat = Current.Format;
        }

        /// <summary>
        /// Применить изменения
        /// </summary>
        private void Apply() 
        {
            Model.ApplyParameters();
            //throw new System.Exception("Сделай применение изменений в макете!");
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