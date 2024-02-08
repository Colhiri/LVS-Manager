using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageLayoutVM : MainVM, IMyTabContentViewModel
    {
        public ManageLayoutVM()
        {
            _LayoutToDelete = new ObservableCollection<string>();
        }

        #region Properties
        /// Позволяет блокировать Tab Viewport'a
        public bool CheckTabEnabled
        {
            get
            {
                return EnabledFormsParamatersLayout;
            }
            set { }
        }

        /// Static model functions to interaction with Autocad
        private CreateLayoutModel model = new CreateLayoutModel();

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
        public bool EnabledFormsParamatersLayout
        {
            get
            {
                return !LayoutToDelete.Contains(Name);
            }
        }

        /// Взаимодействие с именами макетов
        private ObservableCollection<string> _NamesLayouts;
        public ObservableCollection<string> NamesLayouts
        {
            get
            {
                _NamesLayouts = new ObservableCollection<string>(CreateLayoutModel.FL.GetNames());
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
                OnPropertyChanged(nameof(EditName));
                OnPropertyChanged(nameof(LayoutFormat));
                OnPropertyChanged(nameof(PlotterName));
                OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
            }
        }
        /// Отредактированное имя поля
        private string _EditName;
        public string EditName
        {
            get
            {
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
                _Plotters = new ObservableCollection<string>(CreateLayoutModel.GetPlotters());
                return _Plotters;
            }
        }
        /// Имя плоттера
        private string _PlotterName;
        public string PlotterName
        {
            get
            {
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
                _Formats = new ObservableCollection<string>(CreateLayoutModel.GetAllCanonicalScales(PlotterName));
                return _Formats;
            }
        }
        /// Формат макета
        private string _LayoutFormat;
        public string LayoutFormat
        {
            get
            {
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
            _LayoutToDelete.Add(Name);
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
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
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
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
            throw new System.Exception("Сделай применение изменений в макете!");
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