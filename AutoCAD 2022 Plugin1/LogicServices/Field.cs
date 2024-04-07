using AutoCAD_2022_Plugin1.Models;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
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
        public List<Field> Fields { get; set; } = new List<Field>();
        public Point2d StartPoint { get; set; }
        public static Point2d CurrentStartPoint { get; set; }
        public static int ColorIndexForField { get; set; }
        public static int ColorIndexForViewport { get; set; }

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

        public void AddField(string nameLayout, string LayoutFormat, string PlotterName)
        {
            if ((Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(nameLayout)) && CheckPageFormat(LayoutFormat, PlotterName) && CheckPlotter(PlotterName))
            {
                Field field = new Field(nameLayout, LayoutFormat, PlotterName);
                Fields.Add(field);
                CurrentStartPoint = IncreaseStart();
            }
        }
    }

    /// <summary>
    /// Класс содержащий в себе область объектов в видовых экранах, относящихся к определенному листу
    /// </summary>
    public class Field
    {
        public Identificator Id { get; private set; }
        public ObjectId ContourField { get; set; }
        // Параметры размеров
        public static string DownScale { get; set; } = "1:1";
        public string NameLayout { get; set; }

        // Формат макета
        private string _LayoutFormat;
        public string LayoutFormat 
        {
            get { return _LayoutFormat; }
            set 
            {
                if (_LayoutFormat != value)
                {
                    CreateLayoutModel.DeleteObjects(ContourField);
                    Draw();
                    _LayoutFormat = value;
                    this.UpdatePaperSize();
                }
            } 
        }

        // Плоттер макета
        private string _PlotterName;
        public string PlotterName 
        {
            get { return _PlotterName; }
            set 
            {
                _PlotterName = value;
            } 
        }

        public Size OriginalSizeLayout { get; set; }
        public Size DownScaleSizeLayout { get; set; }
        // Общие параметры
        public State StateInModel { get; set; } = State.NoExist;
        public Point2d StartPoint { get; set; }
        public List<ViewportInField> Viewports = new List<ViewportInField>();

        public Field(string NameLayout, string LayoutFormat, string PlotterName)
        {
            this.NameLayout = NameLayout;
            this.PlotterName = PlotterName;
            this.LayoutFormat = LayoutFormat;
            Id = new Identificator();
            UpdatePaperSize();
        }

        /// <summary>
        /// Добавление видового экрана, а также перерасчет некоторых важных параметров для Field
        /// </summary>
        /// <param name="AnnotationScaleViewport"></param>
        /// <param name="ObjectsId"></param>
        public ViewportInField AddViewport(string AnnotationScaleViewport, ObjectIdCollection ObjectsId, string NameViewport)
        {
            // Добавляем стартовую точку
            if (StateInModel == State.NoExist)
            {
                StartPoint = new Point2d(FieldList.CurrentStartPoint.X - DownScaleSizeLayout.Width * 0.5,
                                         FieldList.CurrentStartPoint.Y);
            }
            DistributionViewportOnField PointVP = new DistributionViewportOnField(StartPoint);
            // Добавляем параметры видового экрана
            var viewport = new ViewportInField(AnnotationScaleViewport, ObjectsId, PointVP, NameLayout, NameViewport);
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
        public ObjectId ContourObjects { get; set; }
        public ObjectIdCollection ObjectsIDs { get; set; }
        public string NameLayout { get; set; }
        public string NameViewport { get; set; }
        // Параметры размеров
        private string _AnnotationScaleViewport;
        public string AnnotationScaleViewport 
        {   get { return _AnnotationScaleViewport; }
            set 
            {
                if (_AnnotationScaleViewport != value)
                {
                    _AnnotationScaleViewport = value;
                    this.UpdateSizeVP();
                    CreateLayoutModel.DeleteObjects(ContourObjects);
                    Draw();
                }
            }
        }
        public double CustomScaleViewport { get; set; }
        public Size SizeObjectsWithoutScaling { get; set; }
        public Size SizeObjectsWithScaling { get; set; }
        // Общие параметры
        public Point2d CenterPoint { get; set; }
        public State StateInModel { get; set; } = State.NoExist;
        public DistributionViewportOnField StartPoint { get; set; }

        public ViewportInField(string AnnotationScaleViewport, ObjectIdCollection ObjectsIDs, DistributionViewportOnField StartDrawPointVP, string NameLayout, string NameViewport)
        {
            this.Id = new Identificator();
            this.ObjectsIDs = ObjectsIDs;
            this.StartPoint = StartDrawPointVP;
            this.NameLayout = NameLayout;
            this.NameViewport = NameViewport;
            this.AnnotationScaleViewport = AnnotationScaleViewport;

            SizeObjectsWithoutScaling = CheckModelSize(ObjectsIDs);
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithoutScaling, AnnotationScaleViewport);
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
            SizeObjectsWithoutScaling = CheckModelSize(ObjectsIDs);
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithoutScaling, AnnotationScaleViewport);
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
