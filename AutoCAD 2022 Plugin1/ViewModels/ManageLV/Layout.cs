using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM
    {
        private Field CurrentField;

        public bool EnabledDoneCommandLayout
        {
            get 
            { 
                return CreateLayoutModel.FL.Fields.Select(x => x.NameLayout).Contains(EditFieldName) && model.IsValidName(EditFieldName); 
            }
        }

        /// Взаимодействие с именами макетов
        private ObservableCollection<string> _NamesLayouts;
        public ObservableCollection<string> NamesLayouts
        {
            get
            {
                _NamesLayouts = new ObservableCollection<string>(CreateLayoutModel.FL.Fields.Select(x => x.NameLayout));
                return _NamesLayouts;
            }
        }
        private string _EditFieldName;
        public string EditFieldName
        {
            get { return _EditFieldName; }
            set
            {
                _EditFieldName = value;
                OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
            }
        }

        private string _FieldName;
        public string FieldName
        {
            get
            {
                return _FieldName;
            }
            set
            {
                _FieldName = value;
                CurrentField = CreateLayoutModel.FL.Fields.Where(x => x.NameLayout == _FieldName).First();
                OnPropertyChanged(nameof(NamesLayouts));

                EditFieldName = CurrentField.NameLayout;
                OnPropertyChanged(nameof(EditFieldName));

                PlotterName = CurrentField.PlotterName;
                OnPropertyChanged(nameof(PlotterName));

                LayoutFormat = CurrentField.LayoutFormat;
                OnPropertyChanged(nameof(LayoutFormat));
                OnPropertyChanged(nameof(EnabledFormsParamatersLayout));

                Viewports = new ObservableCollection<string>(CurrentField.Viewports.Select(x => x.Id.ToString()));
                OnPropertyChanged(nameof(Viewports));
                ViewportId = Viewports.First();
                OnPropertyChanged(nameof(ViewportId));
                OnPropertyChanged(nameof(AnnotationScaleObjectsVP));
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
                LayoutFormat = CurrentField.LayoutFormat;
                OnPropertyChanged(nameof(LayoutFormat));
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
    }
}
