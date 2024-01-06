using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Данные для распределения
    /// </summary>
    public class TemporaryDataWPF
    {
        private string _Name;
        private string _Scale;
        public List<string> plotterNames { get; set; }
        public List<string> layoutFormats { get; set; } 
        public List<string> annotationScales { get; set; }

        public string Name 
        {
            get { return _Name; }
            set { _Name = value.Trim(); } 
        }
        public string PlotterName { get; set; }
        public string LayoutFormat { get; set; }
        public string AnnotationScaleObjectsVP
        {
            get { return _Scale; }
            set { _Scale = value.Trim(); }
        }
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

        public bool IsValidScale
        {
            get
            {
                if (string.IsNullOrEmpty(Name)) return false;
                try
                {
                    string[] parts = AnnotationScaleObjectsVP.Split(':');

                    int[] parts1 = AnnotationScaleObjectsVP.Split(':').Select(x => int.Parse(x)).ToArray();
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
