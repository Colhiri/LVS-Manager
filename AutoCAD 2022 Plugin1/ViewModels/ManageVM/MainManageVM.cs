using System.Windows.Controls;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class MainManageVM : MainVM
    {
        public MainManageVM()
        {
            // tabs = new ObservableCollection<DummyViewModel>();
            // tabs.Add(new DummyViewModel("Макет", new ManageLayoutVM()));
            // tabs.Add(new DummyViewModel("Видовой экран", new ManageVIewportVM()));
        }
        // public ObservableCollection<DummyViewModel> tabs { get; set; }
    }

    public class DummyViewModel
    {
        public string Header { get; set; }
        public MainVM VM { get; set; }
        public UserControl UserControlView { get; set; }

        public DummyViewModel(string Header, MainVM VM)
        {
            this.Header = Header;
            this.VM = VM;
        }
    }
}