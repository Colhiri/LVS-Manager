using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM : MainVM
    {

        public ManageLayoutViewportVM()
        {
            _LayoutToDelete = new ObservableCollection<string>();
            _ViewportToDelete = new ObservableCollection<string>();
            _NamesLayouts = new ObservableCollection<string>(CreateLayoutModel.FL.Fields.Select(x => x.NameLayout));
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
        /// Проверка редактирования некоторых частей View
        /// </summary>
        public bool EnabledFormsParamatersViewport
        {
            get
            {
                if (LayoutToDelete.Contains(FieldName))
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

        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete()
        {
            switch (ActiveTab.Name)
            {
                case "Layout":
                    LayoutToDelete.Add(FieldName);
                    break;
                case "Viewport":
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
            switch (ActiveTab.Name)
            {
                case "Layout":
                    LayoutToDelete.Remove(FieldName);
                    break;
                case "Viewport":
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
            switch (ActiveTab.Name)
            {
                case "Layout":
                    if (!LayoutToDelete.Contains(FieldName))
                    {
                        CurrentField.NameLayout = EditFieldName;
                        CurrentField.PlotterName = PlotterName;
                        CurrentField.LayoutFormat = LayoutFormat;
                        NamesLayouts[NamesLayouts.IndexOf(FieldName)] = EditFieldName;
                    }
                    if (LayoutToDelete.Contains(FieldName))
                    {
                        foreach (ViewportInField vp in CurrentField.Viewports)
                        {
                            CreateLayoutModel.DeleteObjects(vp.ContourObjects);
                        }
                        CreateLayoutModel.DeleteObjects(CurrentField.ContourField);
                        CreateLayoutModel.FL.Fields.Remove(CurrentField);
                        NamesLayouts.Remove(FieldName);
                    }
                    break;
                case "Viewport":
                    if (!ViewportToDelete.Contains(ViewportId))
                    {
                        CurrentViewport.NameViewport = NameViewport;
                        CurrentViewport.AnnotationScaleViewport = AnnotationScaleObjectsVP;
                    }
                    if (ViewportToDelete.Contains(ViewportId))
                    {
                        CreateLayoutModel.DeleteObjects(CurrentViewport.ContourObjects);
                        CurrentField.Viewports.Remove(CurrentViewport);
                        Viewports.Remove(ViewportId);
                    }
                    break;
            }
            // Перерисовываем если есть изменения в формате макета
            CreateLayoutModel.FL.RedrawFieldsViewports();
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
