using System.Collections.ObjectModel;
using System.Linq;
using AutoCAD_2022_Plugin1.Services;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class MainManageVM : MainVM
    {
        /// <summary>
        /// Существует только две и только две вкладки, если на одной вкладке происходит удаление листа, 
        /// то вторая становится недоступна, так как она напрямую отображает содержимое выбранного макета на первой вкладке
        /// </summary>

        /// Список для отслеживания вкладкок в Manage
        public ObservableCollection<DummyViewModel> Tabs { get; set; }

        public CurrentLayoutObservable CurrentLayout { get; set; }

        /// <summary>
        /// Отслеживание активной вкладки
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
                EnabledViewportForm();
                FieldNameEdit();
            }
        }

        /// <summary>
        /// Регулирует доступность содержимого на вкладке Viewport
        /// </summary>
        private bool EnabledViewportForm()
        {
            // Проверяем доступность первой вкладки, если она заблокирована удалением Layout,
            // то блокируем вторую вкладку, выставляя значение доступности.
            return Tabs.Where(x => x.TypeView == TypeView.Layout)
                             .Select(x => x.ViewModelTab.CheckTabEnabled)
                             .First();
        }

        /// <summary>
        /// Передает имя выбранного Макета на вкладке Макета
        /// </summary>
        /// <returns></returns>
        private string FieldNameEdit()
        {
            return (Tabs.Where(x => x.TypeView == TypeView.Layout)
                                     .Select(x => x.ViewModelTab)
                                     .First() as ManageLayoutVM).Name;
        }

        public MainManageVM(ParametersLVS parameters)
        {
            CurrentLayoutObservable obs = new CurrentLayoutObservable(parameters.NameLayout);

            parameters.CheckFieldName = FieldNameEdit;
            parameters.CheckTabEnabled = EnabledViewportForm;

            Tabs = new ObservableCollection<DummyViewModel>();
            Tabs.Add(new DummyViewModel("Макет", TypeView.Layout, new ManageLayoutVM(parameters, obs)));
            Tabs.Add(new DummyViewModel("Видовой экран", TypeView.Viewport, new ManageVIewportVM(parameters, obs)));
            ActiveTab = Tabs[0];
        }
    }
}