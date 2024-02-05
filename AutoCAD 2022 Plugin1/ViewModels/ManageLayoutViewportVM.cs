using System.Collections.ObjectModel;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels
{
    public class ManageLayoutViewportVM : MainVM
    {
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
            }
        }

        private ObservableCollection<string> _Plotters;
        public ObservableCollection<string> Plotters
        {
            get
            {
                return _Plotters;
            }
            set
            {
                _Plotters = value;
                OnPropertyChanged(nameof(Formats));
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
            }
        }

        private ObservableCollection<string> _Formats;
        public ObservableCollection<string> Formats
        {
            get
            {
                return _Formats;
            }
            set
            {
                _Formats = value;
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
                return _Scales;
            }
            set
            {
                _Scales = value;
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
    }
}
