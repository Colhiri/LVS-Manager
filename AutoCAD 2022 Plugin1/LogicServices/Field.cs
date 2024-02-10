using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using static AutoCAD_2022_Plugin1.CadUtilityLib;

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

    public enum WorkObject
    {
        Field,
        Viewport,
        None
    }

    /// <summary>
    /// Создать массив Полей
    /// </summary>
    public class FieldList
    {
        // Общие параметры
        private List<Field> Fields { get; set; } = new List<Field>();
        public string CurrentLayout { get; private set; }
        public Point2d StartPoint { get; set; }
        public static Point2d CurrentStartPoint { get; private set; }
        public static int ColorIndexForField { get; set; }
        public static int ColorIndexForViewport { get; set; }
        // Свойства для Fields через Name или ObjectID
        public bool Contains(string NameField) => Fields.Select(x => x.NameLayout).Contains(NameField);
        public bool Contains(ObjectId id) => Fields.Select(x => x.ContourField).Contains(id);
        public string GetPlotter(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().PlotterName;
        public string GetPlotter(ObjectId id) => Fields.Where(x => x.ContourField == id).First().PlotterName;
        public string GetFormat(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().LayoutFormat;
        public string GetFormat(ObjectId id) => Fields.Where(x => x.ContourField == id).First().LayoutFormat;
        public string GetNameFromObjectId(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().NameLayout;
        public string GetNameFromObjectId(ObjectId id) => Fields.Where(x => x.ContourField == id).First().NameLayout;
        public string GetPlotterFromObjectId(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().PlotterName;
        public string GetPlotterFromObjectId(ObjectId id) => Fields.Where(x => x.ContourField == id).First().PlotterName;
        public string GetFormatFromObjectId(string NameField) => Fields.Where(x => x.NameLayout == NameField).First().LayoutFormat;
        public string GetFormatFromObjectId(ObjectId id) => Fields.Where(x => x.ContourField == id).First().LayoutFormat;
        public Field GetField(string NameField) => Fields.Where(x => x.NameLayout == NameField).First();
        public Field GetField(ObjectId id) => Fields.Where(x => x.ContourField == id).First();
        public List<string> GetNames() => Fields.Select(x => x.NameLayout).ToList();
        public void DeleteField(string nameLayout) => Fields.Remove(Fields.Where(x => x.NameLayout == nameLayout).First());
        public void DeleteField(ObjectId id) => Fields.Remove(Fields.Where(x => x.ContourField == id).First());


        /// <summary>
        /// Пересчитать стартовые точки, чтобы не было пересечения между макетами и видовыми экранами
        /// </summary>
        public void RefreshStartPoints()
        {

        }

        /// <summary>
        /// Увеличивает стартовый Х в зависимости от выбранного формата макета
        /// </summary>
        /// <returns></returns>
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
                CurrentLayout = nameLayout;
            }
            return GetField(nameLayout);
        }
    }

    /// <summary>
    /// Класс содержащий в себе область объектов в видовых экранах, относящихся к определенному листу
    /// </summary>
    public class Field
    {
        public Identificator Id { get; private set; }
        public ObjectId ContourField { get; private set; }
        // Параметры размеров
        public static string DownScale { get; set; } = "1:1";
        public string NameLayout { get; private set; }
        public string LayoutFormat { get; private set; }
        public string PlotterName { get; private set; }
        public Size OriginalSizeLayout { get; private set; }
        public Size DownScaleSizeLayout { get; private set; }
        // Общие параметры
        public int CountViewport => Viewports.Count;
        public State StateInModel { get; private set; } = State.NoExist;
        public Point2d StartPoint { get; private set; }
        private List<ViewportInField> Viewports { get; set; } = new List<ViewportInField>();
        // Свойства для Field
        public bool CheckViewport(Identificator Id) => Viewports.Select(x => x.Id == Id).Contains(true);
        public bool CheckViewport(ObjectId Id) => Viewports.Select(x => x.ContourObjects == Id).Contains(true);
        public bool CheckViewport(string Id) => Viewports.Select(x => x.Id.ToString() == Id).Contains(true);
        public ViewportInField GetViewport(Identificator Id) => Viewports.Where(x => x.Id == Id).First();
        public ViewportInField GetViewport(ObjectId Id) => Viewports.Where(x => x.ContourObjects == Id).First();
        public ViewportInField GetViewport(string Id) => Viewports.Where(x => x.Id.ToString() == Id).First();
        public List<Identificator> ViewportIdentificators() => Viewports.Select(x => x.Id).ToList();
        public string GetViewportScale(Identificator Id) => Viewports.Where(x => x.Id == Id).First().AnnotationScaleViewport;
        public string GetViewportScale(ObjectId Id) => Viewports.Where(x => x.ContourObjects == Id).First().AnnotationScaleViewport;
        public string GetViewportScale(string Id) => Viewports.Where(x => x.Id.ToString() == Id).First().AnnotationScaleViewport;
        public void DeleteViewport(Identificator Id) => Viewports.Remove(Viewports.Where(x => x.Id == Id).First());
        public void DeleteViewport(ObjectId Id) => Viewports.Remove(Viewports.Where(x => x.ContourObjects == Id).First());
        public void DeleteViewport(string Id) => Viewports.Remove(Viewports.Where(x => x.Id.ToString() == Id).First());
        public void SetFieldName(string newNameLayout) => this.NameLayout = newNameLayout;
        public void SetFieldPlotter(string newPlotterLayout) => this.PlotterName = newPlotterLayout;
        public void SetFieldFormat(string newFormatLayout)
        {
            this.LayoutFormat = newFormatLayout;
            this.UpdatePaperSize();
        }

        public Field(string NameLayout, string LayoutFormat, string PlotterName)
        {
            this.NameLayout = NameLayout;
            this.LayoutFormat = LayoutFormat;
            this.PlotterName = PlotterName;
            Id = new Identificator();
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
                StartPoint = new Point2d(FieldList.CurrentStartPoint.X - DownScaleSizeLayout.Width * 0.5,
                                         FieldList.CurrentStartPoint.Y);
            }
            DistributionViewportOnField PointVP = new DistributionViewportOnField(StartPoint);
            // Добавляем параметры видового экрана
            var viewport = new ViewportInField(AnnotationScaleViewport, ObjectsId, PointVP, NameLayout);
            Viewports.Add(viewport);
            return viewport;
        }

        public void UpdateStartPoint()
        {

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
        public Identificator Id { get; private set; }
        public ObjectId ContourObjects { get; private set; }
        public ObjectIdCollection ObjectsIDs { get; private set; }
        public string NameLayout { get; private set; }
        // Параметры размеров
        public string AnnotationScaleViewport { get; private set; }
        public double CustomScaleViewport { get; private set; }
        public Size SizeObjectsWithoutScale { get; private set; }
        public Size SizeObjectsWithScaling { get; private set; }
        // Общие параметры
        public Point2d CenterPoint { get; private set; }
        public State StateInModel { get; private set; } = State.NoExist;
        public DistributionViewportOnField StartPoint { get; private set; }
        public void SetScaleVP(string NewScaleVP)
        {
            this.AnnotationScaleViewport = NewScaleVP;
            this.UpdateSizeVP();
        }

        public ViewportInField(string AnnotationScaleViewport, ObjectIdCollection ObjectsIDs, DistributionViewportOnField StartDrawPointVP, string NameLayout)
        {
            this.AnnotationScaleViewport = AnnotationScaleViewport;
            this.ObjectsIDs = ObjectsIDs;
            this.StartPoint = StartDrawPointVP;
            this.NameLayout = NameLayout;
            this.Id = new Identificator();

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
            ContourObjects = DrawRectangle(StartPoint.ToPoint2d(), SizeObjectsWithScaling, FieldList.ColorIndexForViewport);
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

    /// <summary>
    /// Инкапсуляция логики идентификатора для возможных будущих изменений
    /// </summary>
    public class Identificator
    {
        private static int _ID { get; set; } = 0;
        private int ID { get; set; }
        public override string ToString() => ID.ToString();
        public Identificator()
        {
            ID = _ID;
            _ID++;
        }
    }

    public class DistributionViewportOnField
    {
        public Point2d StartPoint { get; set; }
        private Point2d PointDrawing { get; set; }
        public Point2d ToPoint2d() => StartPoint;

        public DistributionViewportOnField(Point2d StartPoint)
        {
            this.StartPoint = StartPoint;
        }
    }
}
