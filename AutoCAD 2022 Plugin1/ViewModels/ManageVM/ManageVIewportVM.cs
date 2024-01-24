using System.Collections.ObjectModel;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class ManageVIewportVM : MainVM
    {
        public ManageVIewportVM(Window window) : base(window) {}

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
