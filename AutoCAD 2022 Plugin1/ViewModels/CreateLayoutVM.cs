﻿using AutoCAD_2022_Plugin1.Models;
using System.Collections.ObjectModel;

namespace AutoCAD_2022_Plugin1.ViewModels
{
    public class CreateLayoutVM : MainVM
    {
        private CreateLayoutModel model = new CreateLayoutModel();

        /// <summary>
        /// Доступность Button "Применить"
        /// </summary>
        private bool _DoneButtonIsEnabled;
        public bool DoneButtonIsEnabled
        {
            get
            {
                _DoneButtonIsEnabled = model.IsValidName(_Name) && model.IsValidScale(_AnnotationScaleObjectsVP);
                return _DoneButtonIsEnabled;
            }
        }

        /// <summary>
        /// Можно выбрать плоттер и формат когда макет еще не создан, если наоборот, то нужно менять плоттер  и формат макета через Managing
        /// </summary>
        public bool EnabledForms
        {
            get
            {
                return !CadUtilityLib.FL.Contains(_Name);
            }
        }

        private ObservableCollection<string> _Names;
        public ObservableCollection<string> Names
        {
            get
            {
                _Names = new ObservableCollection<string>(CadUtilityLib.FL.GetNames());
                return _Names;
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
                _Name = value;
                OnPropertyChanged(nameof(Plotters));
                OnPropertyChanged(nameof(PlotterName));

                OnPropertyChanged(nameof(Formats));
                OnPropertyChanged(nameof(LayoutFormat));

                OnPropertyChanged(nameof(Scales));
                OnPropertyChanged(nameof(AnnotationScaleObjectsVP));

                OnPropertyChanged(nameof(DoneButtonIsEnabled));
                OnPropertyChanged(nameof(EnabledForms));
            }
        }

        private ObservableCollection<string> _Plotters;
        public ObservableCollection<string> Plotters
        {
            get
            {
                _Plotters = new ObservableCollection<string>(CadUtilityLib.GetPlotters());
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

        private ObservableCollection<string> _Formats;
        public ObservableCollection<string> Formats
        {
            get
            {
                _Formats = new ObservableCollection<string>(CadUtilityLib.GetAllCanonicalScales(_PlotterName));
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

        private ObservableCollection<string> _Scales;
        public ObservableCollection<string> Scales
        {
            get
            {
                _Scales = new ObservableCollection<string>(CadUtilityLib.GetAllAnnotationScales());
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
    }
}
