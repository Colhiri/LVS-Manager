using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageVIewportVM : MainVM, IMyTabContentViewModel
    {
        public ManageVIewportVM() 
        {
            _ViewportToDelete = new ObservableCollection<string>();
        }

        private bool _CheckTabEnabled;
        public bool CheckTabEnabled
        {
            get
            {
                return _CheckTabEnabled;
            }
            set
            {
                _CheckTabEnabled = value;
            }
        }

        /// <summary>
        /// Формирует список видовых экранов для удаления после закрытия окна
        /// </summary>
        private ObservableCollection<string> _ViewportToDelete;
        public ObservableCollection<string> ViewportToDelete
        {
            get
            {
                return _ViewportToDelete;
            }
        }

        /// Взаимодействие видовых экранов
        private ObservableCollection<string> _Viewports;
        public ObservableCollection<string> Viewports
        {
            get
            {
                /// <summary>
                /// TEST
                /// TEST
                /// TEST
                /// TEST
                /// </summary>
                //string Name = Working_functions.FL.GetNames()[0];
                //string[] viewportsID = CreateLayoutModel.FL.GetField(Name).ViewportIdentificators().Select(x => x.ToString()).ToArray();
                _Viewports = new ObservableCollection<string>(new string[] { "1", "2"});
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
        
        private void ZoomTest()
        {
            if (ViewportName != null)
            {
                var objectsID = CreateLayoutModel.FL.GetField(NameField).GetViewport(ViewportName).ObjectsIDs;
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
                    _ZoomCommand = new RelayCommand(o => ZoomTest(), null);
                }
                return _ZoomCommand;
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
    }
}
