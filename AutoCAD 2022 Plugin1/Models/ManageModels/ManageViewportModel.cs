using Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Packaging;
using System.Linq;

namespace AutoCAD_2022_Plugin1.Models
{
    public class ManageViewport
    {
        public string Name { get; set; }
        public string Field { get; }
        public string Scale { get; set; }
        public bool Delete { get; set; }
        public string Layout { get; set; }
        public ObjectIdCollection SelectedObjects { get; set; }
        
        private string OldName;
        private string OldScale;

        public ManageViewport(string Name, string Scale, string Field, ObjectIdCollection SelectedObjects, bool Delete = false)
        {
            this.Name = Name;
            this.Field = Field;
            this.SelectedObjects = SelectedObjects;
            this.Scale = OldScale = Scale;
            this.Delete = Delete;
        }

        public bool NeedNameUpdate => Name == OldName;
        public bool NeedScaleUpdate => Scale == OldScale;
    }

    public class ManageViewportModel : MainModel
    {
        public ManageViewport CurrentViewport { get; set; }
        public List<ManageViewport> ManageViewports
        {
            get;
            private set;
        }
        
        public ManageViewportModel()
        {
            ManageViewports = new List<ManageViewport>();
            foreach (LayoutModel layout in FL.Fields)
            {
                foreach (ViewportModel viewport in layout.Viewports)
                {
                    ManageViewports.Add(new ManageViewport(viewport.Name, viewport.Scale, layout.Name, viewport.SelectedObjects));
                }
            }
        }
        
        public bool CheckToDelete() => CurrentViewport.Delete;
        public ObservableCollection<string> GetViewports(string LayoutName)
        {
            return new ObservableCollection<string>(ManageViewports.Where(x => x.Layout == LayoutName)
                                                                   .Select(x => x.Name)
                                                                   .ToList());
        }
        public ManageViewport GetCurrentViewport() => CurrentViewport;
        public ObjectIdCollection GetObjects() => CurrentViewport.SelectedObjects;
        public void SetDelete() => CurrentViewport.Delete = true;
        public void RemoveDelete() => CurrentViewport.Delete = false;
        public void ApplyParameters()
        {
            DeleteLayouts();
            UpdateLayouts();
        }
        private void UpdateLayouts()
        {
            List<ManageViewport> ViewportToEditManage = ManageViewports.Where(vp => (vp.NeedNameUpdate || vp.NeedScaleUpdate) && !vp.Delete)
                                                         .ToList();

            var query = FL.Fields.SelectMany(field => field.Viewports)
                     .ToList()
                     .Join(ViewportToEditManage,
                           NameVPEdit => NameVPEdit.Name,
                           VP => VP.Name,
                           (NameVPEdit, VP) => new { ViewportEdit = VP, ViewportReal = NameVPEdit});

            foreach (var elementQ in query)
            {
                if (elementQ.ViewportEdit.NeedNameUpdate) elementQ.ViewportReal.Name = elementQ.ViewportEdit.Name;
                if (elementQ.ViewportEdit.NeedScaleUpdate) elementQ.ViewportReal.Scale = elementQ.ViewportEdit.Scale;
            }
        }

        private void DeleteLayouts()
        {
            List<string> ViewportToDelete = ManageViewports.Where(vp => vp.Delete)
                                                           .Select(vp => vp.Name)
                                                           .ToList();
                                                           
            FL.Fields.Select(field => field.Viewports)
                     .ToList()
                     .ForEach(vps => vps.RemoveAll(vp => ViewportToDelete.Contains(vp.Name)));
        }
    }
}
