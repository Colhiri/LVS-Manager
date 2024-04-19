using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM
    {
        private ViewportInField CurrentViewport;

        /// <summary>
        /// Доступность команды применения изменений параметров видового экрана
        /// </summary>
        public bool EnabledDoneCommandViewport
        {
            get
            {
                return model.IsValidScale(AnnotationScaleObjectsVP) && CurrentViewport != null;
            }
        }

        /// <summary>
        /// Проверка редактирования некоторых частей View
        /// </summary>
        public bool EnabledFormsParamatersViewport
        {
            get
            {
                if (LayoutToDelete.Contains(FieldName) || ViewportId == null)
                {
                    return false;
                }
                return !ViewportToDelete.Contains(ViewportId);
            }
        }

        public bool InvertEnabledFormsParamatersViewport
        {
            get
            {
                if (LayoutToDelete.Contains(FieldName))
                {
                    return false;
                }
                return ViewportToDelete.Contains(ViewportId);
            }
        }

        /// Взаимодействие видовых экранов
        private List<string> _Viewports;
        public List<string> Viewports
        {
            get
            {
                return _Viewports;
            }
            set
            {
                _Viewports = value;
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
                // Если видовой экран существует, получаем его параметры, иначе null
                _ViewportId = value;
                CurrentViewport = _ViewportId != null ? CreateLayoutModel.FL.Fields.Where(x => x.Name == FieldName)
                                                         .First()
                                                         .Viewports.Where(x => x.ID.ToString() == _ViewportId).First() : null;
                AnnotationScaleObjectsVP = _ViewportId != null ? CurrentViewport.AnnotationScale : null;
                NameViewport = _ViewportId != null ? CurrentViewport.Name : null;

                OnPropertyChanged(nameof(AnnotationScaleObjectsVP));
                OnPropertyChanged(nameof(NameViewport));
                OnPropertyChanged(nameof(EnabledFormsParamatersViewport));
                OnPropertyChanged(nameof(EnabledDoneCommandViewport));
            }
        }

        private string _NameViewport;
        public string NameViewport
        {
            get { return _NameViewport; }
            set
            {
                _NameViewport = value;
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
                OnPropertyChanged(nameof(EnabledDoneCommandViewport));
            }
        }

        /// <summary>
        /// Приблизить на объекты в видовом экране
        /// </summary>
        private void ZoomFunc()
        {
            if (CurrentViewport != null)
            {
                var objectsID = CurrentViewport.ObjectIDs;
                CreateLayoutModel.ZoomToObjects(objectsID);
            }
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
