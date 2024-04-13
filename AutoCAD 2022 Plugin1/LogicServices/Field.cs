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
        public double BorderValueLayout { get; set; } = 300;

        public static Dictionary<string, Point2d> StartsPointsFields = new Dictionary<string, Point2d>();
        public static Dictionary<string, Point2d> EndsPointsFields = new Dictionary<string, Point2d>();

        public void AddField(string nameLayout, string LayoutFormat, string PlotterName)
        {
            if ((Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(nameLayout)) && CheckPageFormat(LayoutFormat, PlotterName) && CheckPlotter(PlotterName))
            {
                Field field = new Field(nameLayout, LayoutFormat, PlotterName);
                Fields.Add(field);
                
                StartsPointsFields.Add(nameLayout, CurrentStartPoint);
                
                Point2d EndPoint = new Point2d(field.StartPoint.X + field.DownScaleSizeLayout.Width, field.StartPoint.Y);

                EndsPointsFields.Add(nameLayout, EndPoint);

                CurrentStartPoint = new Point2d(EndPoint.X + BorderValueLayout, StartPoint.Y);
            }
        }

        /// <summary>
        /// Обновить словари конечных и начальных точек для сравнения точек при переотрисовке полилиний макетов и видовых экранов
        /// </summary>
        public void UpdateDictionary()
        {
            StartsPointsFields.Clear();
            EndsPointsFields.Clear();

            foreach (Field f in Fields)
            {
                StartsPointsFields.Add(f.NameLayout, f.StartPoint);
                EndsPointsFields.Add(f.NameLayout, new Point2d(f.StartPoint.X + f.DownScaleSizeLayout.Width, f.StartPoint.Y));
            }
        }

        /// <summary>
        /// Перерисовывает полилинию макета, если не совпадают стартовые точки в словаре стартовых точек полей и стартой точки в классе поля. Обновляет словарь в конце
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public void RedrawFieldsViewports()
        {
            for (int i = 0; i < Fields.Count;i++)
            {
                Field f = Fields[i];

                double EndPoint = (f.StartPoint.X + f.DownScaleSizeLayout.Width + BorderValueLayout);

                if (i == 0 && new Point2d(EndPoint, f.StartPoint.Y) != EndsPointsFields[f.NameLayout])
                {
                    double CorrectX = f.StartPoint.X;
                    double CorrectY = f.StartPoint.Y;
                    
                    Point2d CorrectStartFieldPoint = new Point2d(CorrectX, CorrectY);
                    f.StartPoint = CorrectStartFieldPoint;
                    CreateLayoutModel.DeleteObjects(f.ContourField);
                    f.Draw();

                    StartsPointsFields[f.NameLayout] = CorrectStartFieldPoint;
                    EndsPointsFields[f.NameLayout] = new Point2d(CorrectStartFieldPoint.X + f.DownScaleSizeLayout.Width, CorrectStartFieldPoint.Y);

                    foreach (ViewportInField vp in f.Viewports)
                    {
                        vp.StartPoint.StartPoint = CorrectStartFieldPoint;
                        CreateLayoutModel.DeleteObjects(vp.ContourObjects);
                        vp.Draw();
                    }
                }
                else
                {
                    Field pastField = Fields[i - 1];

                    if (f.StartPoint.X - (pastField.StartPoint.X + pastField.DownScaleSizeLayout.Width + BorderValueLayout) != 0)
                    {
                        double CorrectX = pastField.StartPoint.X + pastField.DownScaleSizeLayout.Width + BorderValueLayout;
                        double CorrectY = pastField.StartPoint.Y;

                        Point2d CorrectStartFieldPoint = new Point2d(CorrectX, CorrectY);
                        f.StartPoint = CorrectStartFieldPoint;
                        CreateLayoutModel.DeleteObjects(f.ContourField);
                        f.Draw();

                        StartsPointsFields[f.NameLayout] = CorrectStartFieldPoint;
                        EndsPointsFields[f.NameLayout] = new Point2d(CorrectStartFieldPoint.X + f.DownScaleSizeLayout.Width, CorrectStartFieldPoint.Y);

                        foreach (ViewportInField vp in f.Viewports)
                        {
                            vp.StartPoint.StartPoint = CorrectStartFieldPoint;
                            CreateLayoutModel.DeleteObjects(vp.ContourObjects);
                            vp.Draw();
                        }
                    }
                }
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

        public string _NameLayout;
        public string NameLayout
        {
            get { return _NameLayout; }
            set
            {
                if (_NameLayout != null && _NameLayout != value)
                {
                    FieldList.StartsPointsFields.Add(value, FieldList.StartsPointsFields[_NameLayout]);
                    FieldList.EndsPointsFields.Add(value, FieldList.EndsPointsFields[_NameLayout]);
                    FieldList.StartsPointsFields.Remove(_NameLayout);
                    FieldList.EndsPointsFields.Remove(_NameLayout);
                }
                _NameLayout = value;
            }
        }

        // Формат макета
        private string _LayoutFormat;
        public string LayoutFormat 
        {
            get { return _LayoutFormat; }
            set 
            {
                if (_LayoutFormat != value)
                {
                    _LayoutFormat = value;
                    if (ContourField != ObjectId.Null) 
                    {
                        CreateLayoutModel.DeleteObjects(ContourField);
                    }
                    this.UpdatePaperSize();
                    Draw();
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
            Id = new Identificator();

            StartPoint = new Point2d(FieldList.CurrentStartPoint.X, FieldList.CurrentStartPoint.Y);

            this.NameLayout = NameLayout;
            this.PlotterName = PlotterName;
            this.LayoutFormat = LayoutFormat;
        }

        /// <summary>
        /// Добавление видового экрана, а также перерасчет некоторых важных параметров для Field
        /// </summary>
        /// <param name="AnnotationScaleViewport"></param>
        /// <param name="ObjectsId"></param>
        public ViewportInField AddViewport(string AnnotationScaleViewport, ObjectIdCollection ObjectsId, string NameViewport)
        {
            DistributionViewportOnField PointVP = new DistributionViewportOnField(StartPoint);
            // Добавляем параметры видового экрана
            var viewport = new ViewportInField(AnnotationScaleViewport, ObjectsId, PointVP, NameLayout, NameViewport);
            Viewports.Add(viewport);
            return viewport;
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
        public void Draw()
        {
            ContourField = DrawRectangle(StartPoint, DownScaleSizeLayout, FieldList.ColorIndexForField);
            SetLayer(ContourField, NameLayout);
            StateInModel = State.Exist;
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
                    if (ContourObjects != ObjectId.Null) 
                    {
                        CreateLayoutModel.DeleteObjects(ContourObjects);
                    }
                    _AnnotationScaleViewport = value;
                    this.UpdateSizeVP();
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
        public void Draw()
        {
            ContourObjects = DrawRectangle(StartPoint.ToPoint2d(), SizeObjectsWithScaling, FieldList.ColorIndexForViewport);
            SetLayer(ContourObjects, NameLayout);
            StateInModel = State.Exist;
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
