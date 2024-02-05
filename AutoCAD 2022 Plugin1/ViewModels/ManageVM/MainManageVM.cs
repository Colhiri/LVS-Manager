using AutoCAD_2022_Plugin1.Views.ManageViews;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AutoCAD_2022_Plugin1.ViewModels.ManageVM
{
    public class MainManageVM : MainVM
    {
        public ManageLayoutView WindowManageLayout;
        public ManageViewportView WindowManageViewport;
        public ManageLayoutVM ManageLayout { get; private set; }
        public ManageVIewportVM ManageVIewport { get; private set; }
        public ObservableCollection<DummyViewModel> tabs { get; set; }

        public MainManageVM(Window window) : base(window)
        {
            

            // tabs = new ObservableCollection<DummyViewModel>();
            // tabs.Add(new DummyViewModel("Макет", ManageLayout, WindowManageLayout));
            // tabs.Add(new DummyViewModel("Видовой экран", ManageVIewport, WindowManageViewport));
        }
    }

    public class DummyViewModel
    {
        public string Header { get; set; }
        public MainVM VM { get; set; }
        public UserControl UserControlView { get; set; }

        public DummyViewModel(string Header, MainVM VM, UserControl UserControlView)
        {
            this.Header = Header;
            this.VM = VM;
            this.UserControlView = UserControlView;
        }
    }
}