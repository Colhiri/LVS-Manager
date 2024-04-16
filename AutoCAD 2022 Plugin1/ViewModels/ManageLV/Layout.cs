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
                return !LayoutToDelete.Contains(FieldName);
            }
        }
        public bool InvertEnabledFormsParamatersLayout
        {
            get
            {
                return LayoutToDelete.Contains(FieldName);
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
                return first || EditFieldName == FieldName; 
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
                _FieldName = value;
                CurrentField = CreateLayoutModel.FL.Fields.Where(x => x.Name == _FieldName).First();

                EditFieldName = CurrentField.Name;
                
                OnPropertyChanged(nameof(EditFieldName));

                PlotterName = CurrentField.Plotter;
                OnPropertyChanged(nameof(PlotterName));

                LayoutFormat = CurrentField.Format;
                OnPropertyChanged(nameof(LayoutFormat));

                /// Если видовые экраны удалены, замещаем на нулевое значение
                Viewports = CurrentField.Viewports.Select(x => x.ID.ToString()).ToList();
                ViewportId = Viewports.FirstOrDefault();

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
