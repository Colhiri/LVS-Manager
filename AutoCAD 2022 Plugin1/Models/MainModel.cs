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

        public static Point2d StartPoint = new Point2d(0, 0);
        public static int ColorIndexLayout = 3;
        public static int ColorIndexViewport = 4;
        public static string DownScale = "1:1";

        public FieldList FL = FieldList.GetInstance(StartPoint, ColorIndexLayout, ColorIndexViewport, DownScale);

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

    /// <summary>
    /// Создать массив Полей
    /// </summary>
    public class FieldList
    {
        // Общие параметры
        private static FieldList Instance;
        public List<LayoutModel> Fields { get; private set; } = new List<LayoutModel>();
        public LayoutModel CurrentLayout { get; private set; }
        public static Point2d CurrentStartPoint { get; private set; }

        // Получаем с конфига
        public Point2d StartPoint { get; set; }
        public int ColorIndexLayout { get; set; }
        public int ColorIndexViewport { get; set; }
        public string DownScale { get; set; }

        private FieldList(Point2d StartPoint, int ColorIndexLayout, int ColorIndexViewport, string DownScale)
        {
            this.StartPoint = StartPoint;
            this.ColorIndexLayout = ColorIndexLayout;
            this.ColorIndexViewport = ColorIndexViewport;
            this.DownScale = DownScale;
        }

        public static FieldList GetInstance(Point2d StartPoint, int ColorIndexLayout, int ColorIndexViewport, string DownScale)
        {
            if (Instance == null)
            {

                Instance = new FieldList(StartPoint, ColorIndexLayout, ColorIndexViewport, DownScale);
            }
            return Instance;
        }


        /// <summary>
        /// Пересчитать стартовые точки, чтобы не было пересечения между макетами и видовыми экранами
        /// </summary>
        public void RefreshStartPoints()
        {

        }

        /// <summary>
        /// Увеличивает стартовый Х в зависимости от выбранного формата макета
        /// </summary>
        private Point2d IncreaseStart()
        {
            double newPlusX = 0;

            if (Fields.Count == 0) return StartPoint;

            for (int i = 0; i < Fields.Count; i++)
            {
                Size sizeLayout = GetSizePaper(Fields[i].Format, Fields[i].Plotter);
                newPlusX = newPlusX + sizeLayout.Width * 0.5 + sizeLayout.Width;
            }
            return new Point2d(StartPoint.X + newPlusX, StartPoint.Y);
        }

        /// <summary>
        /// Добавить Макет или вернуть текущий макет
        /// </summary>
        public LayoutModel AddLayout(string nameLayout, string LayoutFormat, string PlotterName)
        {
            bool Checking = (Fields.Count == 0 || !Fields.Select(x => x.Name).Contains(nameLayout)) && CheckPageFormat(LayoutFormat, PlotterName) && CheckPlotter(PlotterName);

            if (Checking)
            {
                LayoutModel NewLayout = CurrentLayout = new LayoutModel(Name: nameLayout, ColorIndex: ColorIndexLayout, DownScale: DownScale,
                                                                        Plotter: PlotterName, Format: LayoutFormat);
                Fields.Add(NewLayout);
                CurrentStartPoint = IncreaseStart();
                return NewLayout;
            }
            return CurrentLayout;
        }
    }

    public abstract class FieldModel
    {
        public Identificator ID { get; set; }
        public string Name { get; set; }
        public WorkObject WorkObjectType { get; set; }
        public State StateType { get; set; }
        public int ColorIndex { get; set; }
        public Size OriginalSize { get; set; }
        public Size DownScaleSize { get; set; }
        public ObjectId ContourObject { get; set; }
        public Point2d StartPoint { get; set; }
        public string DownScale { get; set; }
        public FieldModel(string Name, int ColorIndex, string DownScale)
        {
            this.Name = Name;
            this.ColorIndex = ColorIndex;
            this.DownScale = DownScale;
            this.StateType = State.NoExist;
            this.ID = new Identificator();
        }

        public void Draw()
        {
            ContourObject = DrawRectangle(StartPoint, DownScaleSize, ColorIndex);
            SetLayer(ContourObject, Name);
            StateType = State.Exist;
        }
    }

    public class LayoutModel : FieldModel
    {
        public string Plotter { get; set; }
        public string Format { get; set; }
        public List<ViewportModel> Viewports { get; set; }
        public ViewportModel CurrentViewport { get; set; }
        public LayoutModel(string Name, int ColorIndex, string DownScale, string Plotter, string Format) : base(Name, ColorIndex, DownScale)
        {
            this.Plotter = Plotter;
            this.Format = Format;
            this.Viewports = new List<ViewportModel>();
            this.WorkObjectType = WorkObject.Layout;
        }
    }

    public class ViewportModel : FieldModel
    {
        public string Scale { get; set; }
        public ObjectIdCollection SelectedObjects { get; set; }
        public ViewportModel(string Name, int ColorIndex, string DownScale, ObjectIdCollection SelectedObjects, string Scale) : base(Name, ColorIndex, DownScale)
        {
            this.SelectedObjects = SelectedObjects;
            this.Scale = Scale;
            this.WorkObjectType = WorkObject.Viewport;
        }
    }
}
