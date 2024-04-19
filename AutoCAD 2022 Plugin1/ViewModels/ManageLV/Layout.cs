using AutoCAD_2022_Plugin1.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM
    {
        private Field CurrentField;

        /// <summary>
        /// Проверка редактирования некоторых частей View
        /// </summary>
        public bool EnabledFormsParamatersLayout
        {
            get
            {
                return !LayoutToDelete.Contains(FieldName) && CurrentField != null;
            }
        }
        public bool InvertEnabledFormsParamatersLayout
        {
            get
            {
                return LayoutToDelete.Contains(FieldName) && CurrentField != null;
            }
        }

        /// <summary>
        /// Доступность кнопки применения изменения параметров макета
        /// </summary>
        public bool EnabledDoneCommandLayout
        {
            get 
            { 
                bool first = !CreateLayoutModel.FL.Fields.Select(x => x.Name).Contains(EditFieldName) && model.IsValidName(EditFieldName);
                return  CurrentField != null && (first || EditFieldName == FieldName); 
            }
        }

        /// Взаимодействие с именами макетов
        private ObservableCollection<string> _NamesLayouts;
        public ObservableCollection<string> NamesLayouts
        {
            get
            {
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
                OnPropertyChanged(nameof(EnabledDoneCommandLayout));

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
                // Если макет существует, получаем его параметры, иначе null
                _FieldName = value;

                CurrentField = _FieldName != null ? CreateLayoutModel.FL.Fields.Where(x => x.Name == _FieldName).First() : null;

                EditFieldName = _FieldName != null ? CurrentField.Name : null;
                PlotterName = _FieldName != null ? CurrentField.Plotter : null;
                LayoutFormat = _FieldName != null ? CurrentField.Format : null;

                /// Если видовые экраны удалены, замещаем на нулевое значение
                Viewports = _FieldName != null ? CurrentField.Viewports.Select(x => x.ID.ToString()).ToList() : null;
                ViewportId = _FieldName != null ? Viewports.FirstOrDefault() : null;

                OnPropertyChanged(nameof(EditFieldName));
                OnPropertyChanged(nameof(PlotterName));
                OnPropertyChanged(nameof(LayoutFormat));
                OnPropertyChanged(nameof(Viewports));
                OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
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
                LayoutFormat = CurrentField.Format;
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
