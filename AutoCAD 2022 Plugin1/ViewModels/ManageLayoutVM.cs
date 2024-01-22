using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;
namespace AutoCAD_2022_Plugin1
{
    public class ManageLayoutVM
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value.Trim(); }
        }
        public string PlotterName { get; set; }
        public string LayoutFormat { get; set; }
        public bool IsValidName
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return false;
                try
                {
                    SymbolUtilityServices.ValidateSymbolName(Name, false);
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        private string _Scale;
        public string AnnotationScaleObjectsVP
        {
            get { return _Scale; }
            set { _Scale = value.Trim(); }
        }
        public bool IsValidScale
        {
            get
            {
                if (string.IsNullOrEmpty(AnnotationScaleObjectsVP)) return false;
                try
                {
                    int[] parts = AnnotationScaleObjectsVP.Split(':').Select(x => int.Parse(x)).ToArray();
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }
    }
}
