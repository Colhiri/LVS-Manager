using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;

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

    public class ManageLayoutModel : MainModel
    {
        public ManageLayout CurrentLayout { get; set; }

        public List<ManageLayout> ManageLayouts
        {
            get;
            private set;
        }

        public ManageLayoutModel() 
        {
            ManageLayouts = new List<ManageLayout>();
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
    }
}
