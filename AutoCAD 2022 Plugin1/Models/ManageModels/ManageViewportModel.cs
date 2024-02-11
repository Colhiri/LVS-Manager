using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace AutoCAD_2022_Plugin1.Models
{
    public class ManageViewport
    {
        public string Name { get; set; }
        public string Scale { get; set; }
        public bool Delete { get; set; }
        
        private string OldName;
        private string OldScale;

        public ManageViewport(string Name, string Scale, bool Delete = false)
        {
            this.Name = Name;
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
                    ManageViewports.Add(new ManageViewport(viewport.Name, viewport.Scale));
                }
            }
        }
        
        public bool CheckToDelete() => CurrentViewport.Delete;

        public void ApplyParameters()
        {
            DeleteLayouts();
            UpdateLayouts();
        }

        private void UpdateLayouts()
        {
            List<ManageViewport> ViewportToEdit = ManageViewports.Where(vp => (vp.NeedNameUpdate || vp.NeedScaleUpdate) && !vp.Delete)
                                                         .ToList();
            List<string> CheckNameVP = ViewportToEdit.Select(x => x.Name).ToList();

            FL.Fields.SelectMany(field => field.Viewports)
                     .ToList()
                     .Select(vp => CheckNameVP.Contains(vp.Name))
                     .Join(ViewportToEdit, NameVPEdit => NameVPEdit, VP => VP.Name, (NameVPEdit, VP) => new { ViewportEdit = NameVPEdit, ViewportReal = VP })
                     .ToList();

            ViewportToEdit.ForEach(vp =>
            {
                if (vp.NeedNameUpdate) .Name = vp.Name;
                if (vp.NeedScaleUpdate) Lay.Sc = vp.Scale;
            });

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
