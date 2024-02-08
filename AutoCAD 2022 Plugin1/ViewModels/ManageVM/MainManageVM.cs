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

        /// <summary>
        /// Отслеживание активной вкладки
        /// </summary>
        private int _ActiveTab;
        public int ActiveTab
        {
            get
            {
                return _ActiveTab;
            }
            set
            {
                _ActiveTab = value;
                EnabledViewportForm();
                OnPropertyChanged(nameof(ActiveTab));
            }
        }

        /// <summary>
        /// Регулирует доступность содержимого на вкладке Viewport
        /// </summary>
        private void EnabledViewportForm()
        {
            // Проверяем доступность первой вкладки, если она заблокирована удалением Layout,
            // то блокируем вторую вкладку, выставляя значение доступности.
            bool TabEnabledVP;
            bool check = Tabs.Where(x => x.TypeView == TypeView.Layout).Select(x => x.ViewModelTab.CheckTabEnabled).First();
            if (check == false)
            {
                TabEnabledVP = false;
            }
            else
            {
                TabEnabledVP = true;
            }
            Tabs.Where(x => x.TypeView == TypeView.Viewport).Select(x => x.ViewModelTab).First().CheckTabEnabled = TabEnabledVP;
            OnPropertyChanged(nameof(IMyTabContentViewModel.CheckTabEnabled));
        }

        public MainManageVM()
        {
            Tabs = new ObservableCollection<DummyViewModel>();
            Tabs.Add(new DummyViewModel("Макет", TypeView.Layout, new ManageLayoutVM()));
            Tabs.Add(new DummyViewModel("Видовой экран", TypeView.Viewport, new ManageVIewportVM()));
            ActiveTab = 0;
        }
    }
}