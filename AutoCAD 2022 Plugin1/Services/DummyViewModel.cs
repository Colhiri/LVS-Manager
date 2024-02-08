namespace AutoCAD_2022_Plugin1.Services
{
    /// <summary>
    /// Класс, объединяющий разные по наполнению View
    /// </summary>
    public class DummyViewModel
    {
        public string NameTab { get; set; }
        public TypeView TypeView { get; set; }
        public IMyTabContentViewModel ViewModelTab { get; set; }

        public DummyViewModel(string NameTab, TypeView TypeView, IMyTabContentViewModel VM)
        {
            this.NameTab = NameTab;
            this.TypeView = TypeView;
            this.ViewModelTab = VM;
        }
    }
}