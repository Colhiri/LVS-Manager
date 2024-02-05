using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageVIewportVM : MainVM
    {
        /// <summary>
        /// TEST
        /// TEST
        /// TEST
        /// TEST
        /// </summary>
        string Name = Working_functions.FL.GetNames()[0];

        private UserControl userControl;
        public ManageVIewportVM(Window window) : base(window) { }
        public ManageVIewportVM(Window window, UserControl userControl) : base(window)
        {
            this.userControl = userControl;
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
        private string _ViewportName;
        public string ViewportName
        {
            get
            {
                return _ViewportName;
            }
            set
            {
                _ViewportName = value;
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
                _AnnotationScaleObjectsVP = value.Trim();
            }
        }

        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete() => _ViewportToDelete.Add(ViewportName);
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
        private void RemoveDelete() => _ViewportToDelete.Remove(ViewportName);
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
                    if (ViewportName == null) return null;
                    var objectsID = CreateLayoutModel.FL.GetField(Name).GetViewport(ViewportName).ObjectsIDs;
                    _ZoomCommand = new RelayCommand(o => CreateLayoutModel.ZoomToObjects(objectsID), null);
                }
                return _ZoomCommand;
            }
        }
    }
}
