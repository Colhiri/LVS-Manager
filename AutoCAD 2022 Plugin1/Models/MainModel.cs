using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using static AutoCAD_2022_Plugin1.CadUtilityLib;

namespace AutoCAD_2022_Plugin1.Models
{
    public class MainModel
    {
        public static CadUtilityLib MainWorkFunctions = CadUtilityLib.GetCurrent();

        public FieldList FL = FieldList.GetInstance();

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

        public bool IsValidScale(string Scale)
        {
            if (string.IsNullOrEmpty(Scale)) return false;
            try
            {
                int[] parts = Scale.Split(':')
                                   .Select(x => int.Parse(x))
                                   .ToArray();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
