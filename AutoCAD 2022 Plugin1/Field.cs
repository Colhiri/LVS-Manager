using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using static AutoCAD_2022_Plugin1.Working_functions;


namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Состояние отрисовки видового экрана или поля в пространстве модели
    /// </summary>
    public enum State
    {
        Exist,
        NoExist
    }

    /// <summary>
    /// Где находится стартовая точка отрисовки полей и видовых экранов
    /// </summary>
    public enum LocationDraw
    {
        TopLeft, 
        TopRight, 
        BottomLeft,
        BottomRight,
        Custom
    }

    /// <summary>
    /// Создать массив Полей
    /// </summary>
    public class FieldList
    {
        private List<Field> Fields { get; set; } = new List<Field>();
        public string CurrentLayout { get; private set; }
        public Point2d StartPoint { get; set; }
        public static Point2d CurrentStartPoint { get; private set; }
        public static int ColorIndexForField {  get; set; }
        public static int ColorIndexForViewport {  get; set; }

        // Свойства для Fields
        public bool Contains(string NameField) => Fields.Select(x => x.NameLayout).Contains(NameField);
        public string GetPlotter(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().PlotterName;
        public string GetFormat(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().LayoutFormat;
        public List<string> GetNames() => Fields.Select(x => x.NameLayout).ToList();

        public Field UpdateFieldName(string oldNameLayout, string newNameLayout)
        {
            if (Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(oldNameLayout))
                throw new System.Exception($"Layout with name - {oldNameLayout} does not exists.");
            Field CurrentField = GetField(oldNameLayout);
            CurrentField.NameLayout = newNameLayout;
            return CurrentField;
        }

        public Field UpdateFieldPlotter(string oldPlotterLayout, string newPlotterLayout)
        {
            if (Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(oldPlotterLayout))
                throw new System.Exception($"Layout with name - {oldPlotterLayout} does not exists.");
            Field CurrentField = GetField(oldPlotterLayout);
            CurrentField.PlotterName = newPlotterLayout;
            return CurrentField;
        }

        public Field UpdateFieldFormat(string oldFormatLayout, string newFormatLayout)
        {
            if (Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(oldFormatLayout))
                throw new System.Exception($"Layout with name - {oldFormatLayout} does not exists.");
            Field CurrentField = GetField(oldFormatLayout);
            CurrentField.LayoutFormat = newFormatLayout;
            CurrentField.UpdatePaperSize();
            return CurrentField;
        }

        private Point2d IncreaseStart()
        {
            double newPlusX = 0;

            if (Fields.Count == 0) return StartPoint;

            for (int i = 0; i < Fields.Count; i++)
            {
                Size sizeLayout = GetSizePaper(Fields[i].LayoutFormat, Fields[i].PlotterName);
                newPlusX = newPlusX + sizeLayout.Width * 0.5 + sizeLayout.Width;
            }

            return new Point2d(StartPoint.X + newPlusX, StartPoint.Y);
        }

        public Field AddField(string nameLayout, string LayoutFormat, string PlotterName)
        {
            if ((Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(nameLayout)) && CheckPageFormat(LayoutFormat, PlotterName) && CheckPlotter(PlotterName))
            {   
                Fields.Add(new Field(nameLayout, LayoutFormat, PlotterName));
                CurrentStartPoint = IncreaseStart();
            }
            return GetField(nameLayout);
        }

        public void DeleteField(string nameLayout)
        {
            Fields.Remove(Fields[Fields.Select(x => x.NameLayout).ToList().IndexOf(nameLayout)]);
        }

        private Field GetField(string nameLayout)
        {
            CurrentLayout = nameLayout;
            return Fields[Fields.Select(x => x.NameLayout).ToList().IndexOf(nameLayout)];
        }
    }

    /// <summary>
    /// Класс содержащий в себе область объектов в видовых экранах, относящихся к определенному листу
    /// </summary>
    public class Field
    {
        public static int IdMove { get; private set; } = 0;
        public int Id { get; private set; }
        public ObjectId ContourField { get; private set; }

        // Параметры размеров
        public static string DownScale { get; set; } = "1:1";
        public string NameLayout { get; set; }
        public string LayoutFormat { get; set; }
        public string PlotterName { get; set; }
        public Size OriginalSizeLayout { get; set; }
        public Size DownScaleSizeLayout { get; set; }

        // Общие параметры
        public int CountViewport => Viewports.Count;
        public State StateInModel { get; set; } = State.NoExist;
        public Point2d StartPoint { get; private set; }
        private List<ViewportInField> Viewports { get; set; } = new List<ViewportInField>();

        public Field(string NameLayout, string LayoutFormat, string PlotterName)
        {
            this.NameLayout = NameLayout;
            this.LayoutFormat = LayoutFormat;
            this.PlotterName = PlotterName;
            Id = IdMove++;
            UpdatePaperSize();
        }

        /// <summary>
        /// Добавление видового экрана, а также перерасчет некоторых важных параметров для Field
        /// </summary>
        /// <param name="AnnotationScaleViewport"></param>
        /// <param name="ObjectsId"></param>
        public ViewportInField AddViewport(string AnnotationScaleViewport, ObjectIdCollection ObjectsId)
        {
            // Добавляем стартовую точку
            if (StateInModel == State.NoExist)
            {
                StartPoint = FieldList.CurrentStartPoint;

                StartPoint = new Point2d(StartPoint.X - DownScaleSizeLayout.Width * 0.5, StartPoint.Y);
            }

            // Добавляем параметры видового экрана
            var viewport = new ViewportInField(AnnotationScaleViewport, ObjectsId, StartPoint, NameLayout);
            Viewports.Add(viewport);

            return viewport;
        }

        public void DeleteViewport(int Id)
        {
            Viewports.Remove(Viewports[Viewports.Select(x => x.Id).ToList().IndexOf(Id)]);
        }

        public ViewportInField UpdateViewport(int Id)
        {
            return Viewports[Viewports.Select(x => x.Id).ToList().IndexOf(Id)];
        }

        public ViewportInField UpdateScaleVP(int Id, string NewScaleVP)
        {
            ViewportInField currentVP = UpdateViewport(Id);
            if (currentVP == null)
                throw new System.Exception($"Viewport with ID - {Id} does not exists.");
            currentVP.AnnotationScaleViewport = NewScaleVP;
            currentVP.UpdateSizeVP();
            return currentVP;
        }

        public void UpdatePaperSize()
        {
            // Получаем оригинальный масштаб
            OriginalSizeLayout = GetSizePaper(LayoutFormat, PlotterName);
            // Применяем уменьшающий коэффициент
            DownScaleSizeLayout = ApplyScaleToSizeObjectsInModel(OriginalSizeLayout, DownScale);
        }

        /// <summary
        /// Рисуем Field
        /// </summary>
        /// <returns></returns>
        public object Draw()
        {
            ContourField = DrawRectangle(StartPoint, DownScaleSizeLayout, FieldList.ColorIndexForField);
            SetLayer(ContourField, NameLayout);
            StateInModel = State.Exist;
            return null;
        }
    }

    /// <summary>
    /// Класс содержащий в себе информацию об отдельном видовом экране на макете
    /// </summary>
    public class ViewportInField
    {
        // Параметры идентификации
        public static int IdMove { get; private set; } = 0;
        public int Id { get; private set; }
        public ObjectId ContourObjects { get; private set; }
        public ObjectIdCollection ObjectsIDs { get; private set; }
        public string NameLayout { get; set; }

        // Параметры размеров
        public string AnnotationScaleViewport { get; set; }
        public double CustomScaleViewport { get; set; }
        public Size SizeObjectsWithoutScale { get; private set; }
        public Size SizeObjectsWithScaling { get; private set; }

        // Общие параметры
        public Point2d CenterPoint { get; private set; }
        public State StateInModel { get; set; } = State.NoExist;
        public Point2d StartDrawPointVP { get; private set; }

        public ViewportInField(string AnnotationScaleViewport, ObjectIdCollection ObjectsIDs, Point2d StartDrawPointVP, string NameLayout)
        {
            this.AnnotationScaleViewport = AnnotationScaleViewport;
            this.ObjectsIDs = ObjectsIDs;
            this.StartDrawPointVP = StartDrawPointVP;
            this.NameLayout = NameLayout;
            this.Id = IdMove++;

            SizeObjectsWithoutScale = CheckModelSize(ObjectsIDs);
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithoutScale, AnnotationScaleViewport);
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithScaling, Field.DownScale);
            CenterPoint = Point3dTo2d(CheckCenterModel(ObjectsIDs));
        }

        /// <summary>
        /// Рисуем контур объектов в пространстве модели
        /// </summary>
        /// <returns></returns>
        public object Draw()
        {
            ContourObjects = DrawRectangle(StartDrawPointVP, SizeObjectsWithScaling, FieldList.ColorIndexForViewport);
            SetLayer(ContourObjects, NameLayout);
            StateInModel = State.Exist;
            return null;
        }

        public void UpdateSizeVP()
        {
            SizeObjectsWithoutScale = CheckModelSize(ObjectsIDs);
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithoutScale, AnnotationScaleViewport);
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithScaling, Field.DownScale);
        }
    }
}
