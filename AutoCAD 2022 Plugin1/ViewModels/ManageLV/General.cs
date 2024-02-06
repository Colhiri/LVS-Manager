using AutoCAD_2022_Plugin1.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageLV
{
    public partial class ManageLayoutViewportVM : MainVM
    {
        /// <summary>
        /// Static model functions to iteration with Autocad
        /// </summary>
        private CreateLayoutModel model = new CreateLayoutModel();

        /// <summary>
        /// Активная вкладка для реализации команд
        /// </summary>
        private object _ActiveTab;
        public object ActiveTab
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
                return !LayoutToDelete.Contains(Name);
            }
        }
    }
}
