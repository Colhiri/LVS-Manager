using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels
{
    public class ManageLayoutViewportVM : MainVM
    {
        public ManageLayoutViewportVM(Window window) : base(window) { }

        #region Properties
        /// <summary>
        /// Static model functions to iteration with Autocad
        /// </summary>
        private CreateLayoutModel model = new CreateLayoutModel();

        /// <summary>
        /// Активная вкладка для реализации команд
        /// </summary>
        private string _ActiveTab;
        public string ActiveTab
        {
            get
            {
                return _ActiveTab;
            }
            set
            {
                _ActiveTab = value;
            }
        }

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
            private set { }
        }

        /// <summary>
        /// Формирует список видовых экранов для удаления после закрытия окна
        /// </summary>
        private ObservableCollection<string> _ViewportToDelete;
        public ObservableCollection<string> ViewportToDelete
        {
            get
            {
                _ViewportToDelete = new ObservableCollection<string>();
                return _ViewportToDelete;
            }
            private set { }
        }

        /// <summary>
        /// Проверка редактирования некоторых частей View
        /// </summary>
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

        /// Взаимодействие видовых экранов
        private ObservableCollection<string> _Viewports;
        public ObservableCollection<string> Viewports
        {
            get
            {
                string[] viewportsID = CreateLayoutModel.FL.GetField(Name).ViewportIdentificators().Select(x => x.ToString()).ToArray();
                _Viewports = new ObservableCollection<string>(viewportsID);
                return _Viewports;
            }
        }
        private string _Viewport;
        public string Viewport
        {
            get
            {
                return _Viewport;
            }
            set
            {
                _Viewport = value;
            }
        }

        /// Взаимодействие с мастабами видовых экранов
        private ObservableCollection<string> _Scales;
        public ObservableCollection<string> Scales
        {
            get
            {
                _Scales = new ObservableCollection<string>(CreateLayoutModel.GetAllAnnotationScales());
                return _Scales;
            }
        }
        private string _AnnotationScaleObjectsVP;
        public string AnnotationScaleObjectsVP
        {
            get
            {
                return _AnnotationScaleObjectsVP;
            }
            set
            {
                _AnnotationScaleObjectsVP = value;
            }
        }

        #endregion

        #region Commands
        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete()
        {
            switch (ActiveTab) 
            {
                case "Layout":
                    _LayoutToDelete.Add(Name);
                    break;
                case "Viewport":
                    _ViewportToDelete.Add(Name);
                    break;
            }
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
            switch (ActiveTab)
            {
                case "Layout":
                    _LayoutToDelete.Remove(Name);
                    break;
                case "Viewport":
                    _ViewportToDelete.Remove(Name);
                    break;
            }
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
        /// Приблизить на объекты в видовом экране
        /// </summary>
        private RelayCommand _ZoomCommand;
        public RelayCommand ZoomCommand
        {
            get
            {
                if (_ZoomCommand == null)
                {
                    var objectsID = CreateLayoutModel.FL.GetField(Name).GetViewport(Viewport).ObjectsIDs;
                    _ZoomCommand = new RelayCommand(o => CreateLayoutModel.ZoomToObjects(objectsID), null);
                }
                return _ZoomCommand;
            }
        }
        #endregion
    }
}
