using AutoCAD_2022_Plugin1.Models;
using AutoCAD_2022_Plugin1.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM : MainVM
    {
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
                _LayoutToDelete = new ObservableCollection<string>();
                return _LayoutToDelete;
            }
            private set { }
        }

        /// <summary>
        /// Формирует список видовых экранов для удаления после закрытия окна
        /// </summary>
        private ObservableCollection<string> _ViewportToDelete;
        public ObservableCollection<string> ViewportToDelete
        {
            get
            {
                _ViewportToDelete = new ObservableCollection<string>();
                return _ViewportToDelete;
            }
            private set { }
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

        /// <summary>
        /// Добавление имени макета в список на удаление
        /// </summary>
        private void AddDelete()
        {
            switch (ActiveTab.Name)
            {
                case "Layout":
                    _LayoutToDelete.Add(FieldName);
                    break;
                case "Viewport":
                    _ViewportToDelete.Add(ViewportId);
                    break;
            }
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
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
                    _LayoutToDelete.Remove(FieldName);
                    break;
                case "Viewport":
                    _ViewportToDelete.Remove(ViewportId);
                    break;
            }
            OnPropertyChanged(nameof(EnabledFormsParamatersLayout));
        }

        /// <summary>
        /// Сохранить изменения в макете
        /// </summary>
        private void SaveChangesLayout()
        {
            CurrentField.PlotterName = _PlotterName;
            CurrentField.LayoutFormat = _LayoutFormat;
        }

        /// <summary>
        /// Сохранить изменения в видовом экране
        /// </summary>
        private void SaveChangesViewport()
        {
            CurrentViewport.AnnotationScaleViewport = _AnnotationScaleObjectsVP;
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
                    Action<object> act = null;
                    switch (ActiveTab.Name) 
                    {
                        case "Layout":
                            act = o => SaveChangesLayout();
                            break;
                        case "Viewport":
                            act = o => SaveChangesViewport();
                            break;
                    }
                    _DoneCommand = new RelayCommand(o => act(null), null);
                }
                return _DoneCommand;
            }
        }
    }
}
