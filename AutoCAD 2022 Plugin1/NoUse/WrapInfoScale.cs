using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Linq;

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Класс обертки для получения аннотационного масштаба и его параметров
    /// </summary>
    public class WrapInfoScale
    {
        public static int Id { get; set; } = 1;
        public string Name { get; set; }
        public double CustomScale { get; set; }
        public int FirstNumberScale { get; set; }
        public int SecondNumberScale { get; set; }
        public StandardScaleType standardScale { get; set; }

        public WrapInfoScale(string Name, double CustomScale)
        {
            this.Name = Name;
            int[] scales = Name.Split(':').Select(x => int.Parse(x)).ToArray();
            this.CustomScale = CustomScale; // scales[0] / scales[1];
            Id++;
            this.FirstNumberScale = scales[0];
            this.SecondNumberScale = scales[1];

            foreach (string std in Enum.GetNames(typeof(StandardScaleType)))
            {
            }
        }
    }
}
