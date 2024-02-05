using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageLayoutVM : MainVM
    {
        public ManageLayoutVM(Window window) : base(window) { }

        #region Properties
        /// <summary>
        /// Формирует список листов для удаления после закрытия окна
        /// </summary>
        private ObservableCollection<string> _LayoutToDelete;
        public ObservableCollection<string> LayoutToDelete
        {
            get 
            {
                _LayoutToDelete = new ObservableCollection<string>();
                return _LayoutToDelete;
            }
            private set {}
        }

        /// <summary>
        /// Проверка редактирования некоторых частей View
        /// </summary>
        public bool EnabledFormsParamatersLayout
        {
            get
            {
                return LayoutToDelete.Contains(Name);
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
                if (_EditName != _Name)
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
                _Formats = new ObservableCollection<string>(CreateLayoutModel.GetAllCanonicalScales(_PlotterName));
                return _Formats;
            }
        }
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
        private void AddLayoutDelete() => _LayoutToDelete.Add(Name);
        private RelayCommand _DeleteLayoutCommand;
        public RelayCommand DeleteLayoutCommand
        {
            get
            {
                if (_DeleteLayoutCommand == null)
                {
                    _DeleteLayoutCommand = new RelayCommand(o => AddLayoutDelete(), null);
                }
                return _DeleteLayoutCommand;
            }
        }

        /// <summary>
        /// Убрать имя макета из списка на удаление
        /// </summary>
        private void RemoveLayoutDelete() => _LayoutToDelete.Remove(Name);
        private RelayCommand _CancelDeleteLayoutCommand;
        public RelayCommand CancelDeleteLayoutCommand
        {
            get
            {
                if (_CancelDeleteLayoutCommand == null)
                {
                    _CancelDeleteLayoutCommand = new RelayCommand(o => RemoveLayoutDelete(), null);
                }
                return _CancelDeleteLayoutCommand;
            }
        }
        #endregion
    }
}
