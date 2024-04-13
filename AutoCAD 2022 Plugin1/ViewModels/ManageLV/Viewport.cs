using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM
    {
        private ViewportInField CurrentViewport;

        public bool EnabledDoneCommandViewport
        {
            get
            {
                return model.IsValidScale(AnnotationScaleObjectsVP);
            }
        }

        /// Взаимодействие видовых экранов
        private ObservableCollection<string> _Viewports;
        public ObservableCollection<string> Viewports
        {
            get
            {
                return _Viewports;
            }
            set
            {
                _Viewports = value;
                ViewportId = Viewports.First();
                OnPropertyChanged(nameof(ViewportId));
            }
        }
        private string _ViewportId;
        public string ViewportId
        {
            get
            {
                return _ViewportId;
            }
            set
            {
                _ViewportId = value == null ? Viewports.First() : value;
                CurrentViewport = CreateLayoutModel.FL.Fields.Where(x => x.NameLayout == FieldName)
                                                             .First()
                                                             .Viewports.Where(x => x.Id.ToString() == _ViewportId).First();
                AnnotationScaleObjectsVP = CurrentViewport.AnnotationScaleViewport;
                OnPropertyChanged(nameof(AnnotationScaleObjectsVP));

                NameViewport = CurrentViewport.NameViewport;
                OnPropertyChanged(nameof(NameViewport));
                OnPropertyChanged(nameof(EnabledFormsParamatersViewport));
            }
        }

        private string _NameViewport;
        public string NameViewport
        {
            get { return _NameViewport; }
            set
            {
                _NameViewport = value;
                CurrentViewport.NameViewport = _NameViewport;
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


        /// <summary>
        /// Приблизить на объекты в видовом экране
        /// </summary>
        private void ZoomFunc()
        {
            var objectsID = CurrentViewport.ObjectsIDs;
            CreateLayoutModel.ZoomToObjects(objectsID);
        }
        private RelayCommand _ZoomCommand;
        public RelayCommand ZoomCommand
        {
            get
            {
                if (_ZoomCommand == null)
                {
                    _ZoomCommand = new RelayCommand(o => ZoomFunc(), null);
                }
                return _ZoomCommand;
            }
        }
    }
}
