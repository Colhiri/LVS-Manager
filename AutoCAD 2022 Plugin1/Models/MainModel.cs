using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;

namespace AutoCAD_2022_Plugin1.Models
{
    public class MainModel
    {
        public static CadUtilityLib MainWorkFunctions = CadUtilityLib.GetCurrent();

        public bool IsValidName(string Name)
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

        public bool IsValidScale(string AnnotationScaleObjectsVP)
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
