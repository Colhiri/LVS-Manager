using System.Collections.ObjectModel;
using System.Security.AccessControl;
using System.Windows;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class MainManageVM : MainVM
    {
        public MainManageVM(Window window) : base(window) { }


        private ObservableCollection<Window> windows = new ObservableCollection<Window>() { };

        private string _ActiveTab;
        public string ActiveTab
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

        private Window _ActiveWindow;
        public Window ActiveWindow
        {
            get
            {
                return _ActiveWindow;
            }
            set
            {
                _ActiveWindow = value;
            }
        }
    }
}
