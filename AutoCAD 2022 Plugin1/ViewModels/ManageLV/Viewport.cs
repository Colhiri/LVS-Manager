using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM
    {
        private ViewportInField CurrentViewport;

        /// Взаимодействие видовых экранов
        private ObservableCollection<string> _Viewports;
        public ObservableCollection<string> Viewports
        {
            get
            {
                string[] viewportsID = CreateLayoutModel.FL.Fields.Where(x => x.NameLayout == Name)
                                                                  .First()
                                                                  .Viewports.Select(x => x.Id.ToString()).ToArray();
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
                CurrentViewport = CreateLayoutModel.FL.Fields.Where(x => x.NameLayout == Name)
                                                             .First()
                                                             .Viewports.Where(x => x.Id.ToString() == _Viewport).First();
                AnnotationScaleObjectsVP = CurrentViewport.AnnotationScaleViewport;
                OnPropertyChanged(nameof(AnnotationScaleObjectsVP));
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
        private RelayCommand _ZoomCommand;
        public RelayCommand ZoomCommand
        {
            get
            {
                if (_ZoomCommand == null)
                {
                    var objectsID = CurrentViewport.ObjectsIDs;
                    _ZoomCommand = new RelayCommand(o => CreateLayoutModel.ZoomToObjects(objectsID), null);
                }
                return _ZoomCommand;
            }
        }
    }
}
