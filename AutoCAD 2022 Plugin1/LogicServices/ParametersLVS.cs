using System;

namespace AutoCAD_2022_Plugin1.Services
{
    public class ParametersLVS
    {
        public string NameLayout { get; set; }
        public string PlotterName { get; set; }
        public string LayoutFormat { get; set; }
        public Func<string> CheckFieldName { get; set; }
        public Func<bool> CheckTabEnabled { get; set; }
        public string AnnotationScaleObjectsVP { get; set; }
    }
}