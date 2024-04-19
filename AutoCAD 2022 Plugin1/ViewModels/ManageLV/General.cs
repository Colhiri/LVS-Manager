using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM : MainVM
    {
        FieldDistributionOnModel dist = FieldDistributionOnModel.GetInstance();

        public ManageLayoutViewportVM()
        {
            _LayoutToDelete = new ObservableCollection<string>();
            _ViewportToDelete = new ObservableCollection<string>();
            _NamesLayouts = new ObservableCollection<string>(CreateLayoutModel.FL.Fields.Select(x => x.Name).ToList());
        }

        /// <summary>
        /// Static model functions to iteration with Autocad
        /// </summary>
        private CreateLayoutModel model = new CreateLayoutModel();

        /// <summary>
        /// Тип вкладки
        /// </summary>
        private WorkObject _TypeActiveTab;
        public WorkObject TypeActiveTab
        {
            get { return _TypeActiveTab; }
            set { _TypeActiveTab = value; }
        }

        /// <summary>
        /// Активная вкладка для реализации команд
        /// </summary>
        private TabItem _ActiveTab;
        public TabItem ActiveTab
        {
            get
            {
                return _ActiveTab;
            }
            set
            {
                _ActiveTab = value;
                switch (ActiveTab.Name)
                {
                    case "Layout":
                        TypeActiveTab = WorkObject.Field;
                        break;
                    case "Viewport":
                        TypeActiveTab = WorkObject.Viewport;
                        break;
                }
            }
        }

        /// <summary>
        /// Формирует список листов для удаления после закрытия окна
        /// </summary>
        private ObservableCollection<string> _LayoutToDelete;
        public ObservableCollection<string> LayoutToDelete
        {
            get
            {
                return _LayoutToDelete;
            }
        }

        /// <summary>
        /// Формирует список видовых экранов для удаления после закрытия окна
        /// </summary>
        private ObservableCollection<string> _ViewportToDelete;
        public ObservableCollection<string> ViewportToDelete
        {
            get
            {
                return _ViewportToDelete;
            }
        }

        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete()
        {
            switch (TypeActiveTab)
            {
                case WorkObject.Field:
                    LayoutToDelete.Add(FieldName);
                    break;
                case WorkObject.Viewport:
                    ViewportToDelete.Add(ViewportId);
                    break;
            }
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
            OnPropertyChanged(nameof(EnabledFormsParamatersViewport));
            OnPropertyChanged(nameof(InvertEnabledFormsParamatersLayout));
            OnPropertyChanged(nameof(InvertEnabledFormsParamatersViewport));
        }
        private RelayCommand _DeleteCommand;
        public RelayCommand DeleteCommand
        {
            get
            {
                if (_DeleteCommand == null)
                {
                    _DeleteCommand = new RelayCommand(o => AddDelete(), null);
                }
                return _DeleteCommand;
            }
        }

        /// <summary>
        /// Убрать макет или видовой экран из списка на удаление
        /// </summary>
        private void RemoveDelete()
        {
            switch (TypeActiveTab)
            {
                case WorkObject.Field:
                    LayoutToDelete.Remove(FieldName);
                    break;
                case WorkObject.Viewport:
                    ViewportToDelete.Remove(ViewportId);
                    break;
            }
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
            OnPropertyChanged(nameof(EnabledFormsParamatersViewport));
            OnPropertyChanged(nameof(InvertEnabledFormsParamatersLayout));
            OnPropertyChanged(nameof(InvertEnabledFormsParamatersViewport));
        }

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        private void SaveChanges()
        {
            switch (TypeActiveTab)
            {
                case WorkObject.Field:
                    if (!LayoutToDelete.Contains(FieldName))
                    {
                        CurrentField.Name = EditFieldName;
                        CurrentField.Plotter= PlotterName;
                        CurrentField.Format = LayoutFormat;
                        NamesLayouts[NamesLayouts.IndexOf(FieldName)] = EditFieldName;
                    }
                    if (LayoutToDelete.Contains(FieldName))
                    {
                        foreach (ViewportInField vp in CurrentField.Viewports)
                        {
                            CreateLayoutModel.DeleteObjects(vp.ContourPolyline);
                        }
                        CreateLayoutModel.DeleteObjects(CurrentField.ContourPolyline);
                        CreateLayoutModel.FL.Fields.Remove(CurrentField);
                        NamesLayouts.Remove(FieldName);
                        LayoutToDelete.Remove(FieldName);

                        // Назначаем новый макет, если он имеется
                        FieldName = CreateLayoutModel.FL.Fields.Count > 0 ? CreateLayoutModel.FL.Fields[0].Name : null;
                    }
                    break;
                case WorkObject.Viewport:
                    if (!ViewportToDelete.Contains(ViewportId))
                    {
                        CurrentViewport.Name = NameViewport;
                        CurrentViewport.AnnotationScale = AnnotationScaleObjectsVP;
                    }
                    if (ViewportToDelete.Contains(ViewportId))
                    {
                        CreateLayoutModel.DeleteObjects(CurrentViewport.ContourPolyline);
                        CurrentField.Viewports.Remove(CurrentViewport);
                        Viewports.Remove(ViewportId);
                        ViewportToDelete.Remove(ViewportId);

                        // Назначаем новый видовой экран, если он имеется
                        ViewportId = Viewports.Count == 0 ? null : Viewports[0];
                    }
                    break;
            }
            OnPropertyChanged(nameof(ViewportId));
            OnPropertyChanged(nameof(AnnotationScaleObjectsVP));
            OnPropertyChanged(nameof(Viewports));
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
            OnPropertyChanged(nameof(EnabledFormsParamatersViewport));
            OnPropertyChanged(nameof(InvertEnabledFormsParamatersLayout));
            OnPropertyChanged(nameof(InvertEnabledFormsParamatersViewport));
            // Перерисовываем если есть изменения в формате макета
            CreateLayoutModel.FL.CreateNewPoints();
        }

        private RelayCommand _CancelDeleteCommand;
        public RelayCommand CancelDeleteCommand
        {
            get
            {
                if (_CancelDeleteCommand == null)
                {
                    _CancelDeleteCommand = new RelayCommand(o => RemoveDelete(), null);
                }
                return _CancelDeleteCommand;
            }
        }

        private RelayCommand _DoneCommand;
        public RelayCommand DoneCommand
        {
            get
            {
                if (_DoneCommand == null)
                {
                    _DoneCommand = new RelayCommand(o => SaveChanges(), null);
                }
                return _DoneCommand;
            }
        }
    }
}
