using AutoCAD_2022_Plugin1.Models;
using System.Collections.ObjectModel;
using System.Windows;

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
        /// Доступность ComboBox выбора плоттеров
        /// </summary>
        private bool _PlottersIsEnabled;
        public bool PlottersIsEnabled
        {
            get
            {
                _PlottersIsEnabled = CreateLayoutModel.FL.Contains(_Name);
                return _PlottersIsEnabled;
            }
        }

        /// <summary>
        /// Доступность ComboBox выбора плоттеров
        /// </summary>
        private bool _FormatsIsEnabled;
        public bool FormatsIsEnabled
        {
            get
            {
                _FormatsIsEnabled = CreateLayoutModel.FL.Contains(_Name);
                return _FormatsIsEnabled;
            }
        }

        private ObservableCollection<string> _Names;
        public ObservableCollection<string> Names
        {
            get
            {
                _Names = new ObservableCollection<string>(CreateLayoutModel.FL.GetNames());
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
                OnPropertyChanged(nameof(FormatsIsEnabled));
                OnPropertyChanged(nameof(PlottersIsEnabled));
            }
        }

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
    }
}
