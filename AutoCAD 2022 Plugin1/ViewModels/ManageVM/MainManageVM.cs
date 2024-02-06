using System.Collections.ObjectModel;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class MainManageVM : MainVM
    {
        public ObservableCollection<DummyViewModel> Tabs { get; set; }
        public MainManageVM()
        {
            Tabs = new ObservableCollection<DummyViewModel>();
            Tabs.Add(new DummyViewModel("Lay", new ManageLayoutVM()));
            Tabs.Add(new DummyViewModel("VP", new ManageVIewportVM()));
        }
    }

    public class DummyViewModel
    {
        public string NameTab { get; set; }
        public MainVM ViewModelTab { get; set; }

        public DummyViewModel(string NameTab, MainVM VM)
        {
            this.NameTab = NameTab;
            this.ViewModelTab = VM;
        }
    }
}