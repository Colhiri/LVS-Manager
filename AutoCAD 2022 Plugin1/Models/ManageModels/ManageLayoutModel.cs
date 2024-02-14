using AutoCAD_2022_Plugin1.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace AutoCAD_2022_Plugin1.Models
{
    /// <summary>
    /// Прослойка для сохранения состояния и отслеживания изменений
    /// </summary>
    public class ManageLayout
    {
        public string Name { get; set; }
        public string Plotter { get; set; }
        public string Format { get; set; }
        public bool Delete { get; set; }

        private string OldName;
        private string OldPlotterName;
        private string OldFormatLayout;

        public ManageLayout(string Name, string Plotter, string Format, bool Delete = false)
        {
            this.Name = OldName = Name;
            this.Plotter = OldPlotterName = Plotter;
            this.Format = OldFormatLayout = Format;
            this.Delete = Delete;
        }

        public bool NeedNameUpdate => Name == OldName;
        public bool NeedPlotterUpdate => Plotter == OldPlotterName;
        public bool NeedFormatUpdate => Format == OldFormatLayout;
    }

    public class ManageLayoutModel : MainModel, IObserver
    {
        private CurrentLayoutObservable obs;
        private ParametersLVS parameters;
        public ManageLayout CurrentLayout { get; set; }

        public List<ManageLayout> ManageLayouts
        {
            get;
            private set;
        }

        public ManageLayoutModel(ParametersLVS parameters, CurrentLayoutObservable obs) 
        {
            this.parameters = parameters;
            ManageLayouts = new List<ManageLayout>();
            this.obs = obs;
            obs.AddObserver(this);
            foreach (LayoutModel layout in FL.Fields)
            {
                ManageLayouts.Add(new ManageLayout(layout.Name, layout.Plotter, layout.Format));
            }
        }
        
        public bool CheckToDelete() => CurrentLayout.Delete;

        public void ApplyParameters()
        {
            DeleteLayouts();
            UpdateLayouts();
        }

        private void UpdateLayouts()
        {
            foreach (ManageLayout Layout in ManageLayouts)
            {
                LayoutModel Lay = FL.Fields.Where(x => x.Name == Layout.Name)
                                 .First();
                if (!Layout.Delete) 
                {
                    if (Layout.NeedNameUpdate) Lay.Name = Layout.Name;
                    if (Layout.NeedPlotterUpdate) Lay.Plotter = Layout.Plotter; 
                    if (Layout.NeedFormatUpdate) Lay.Format = Layout.Format;
                }
            }
        }

        private void DeleteLayouts()
        {
            foreach (ManageLayout Layout in ManageLayouts)
            {
                if (Layout.Delete) FL.Fields.RemoveAll(x => x.Name == Layout.Name);
            }
        }

        public ObservableCollection<string> GetNames() => new ObservableCollection<string>(FL.Fields.Select(x => x.Name).ToList());
        public ObservableCollection<string> GetPlotters() => new ObservableCollection<string>(CadUtilityLib.GetPlotters());
        public ObservableCollection<string> GetFormats() => new ObservableCollection<string>(CadUtilityLib.GetAllCanonicalScales(CurrentLayout.Plotter));
        public string GetCurrentPlotter() => CurrentLayout.Plotter;
        public string GetCurrentFormat() => CurrentLayout.Format;

        public void SetDelete() => CurrentLayout.Delete = true;
        public void RemoveDelete() => CurrentLayout.Delete = false;
        public ManageLayout GetCurrentLayout(string Name) => ManageLayouts.Where(lay => lay.Name == Name).First();
        public void Update()
        {
            CurrentLayout = GetCurrentLayout(obs.CurrentLayout);
        }
    }
}
