using AutoCAD_2022_Plugin1.Models;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using static AutoCAD_2022_Plugin1.Working_functions;
using AutoCAD_2022_Plugin1.LogicServices;

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

    public enum WorkObject
    {
        Field,
        Viewport,
        None
    }

    /// <summary>
    /// Создать массив Полей
    /// </summary>
    public class Regulator
    {
        // Получаем конфигурационные параметры
        Config config = Config.GetConfig();
        // Получаем список макетов
        public FieldList<Field> Fields { get; set; } = FieldList<Field>.GetInstance();
        // Получаем распределение макетов по пространству модели
        public FieldDistributionOnModel distribution { get; set; } = FieldDistributionOnModel.GetInstance();

        public Point2d StartPoint { get; set; }
        public static Point2d CurrentStartPoint { get; set; }

        /// <summary>
        /// Перерисовывает полилинию макета, если не совпадают стартовые точки в словаре стартовых точек полей и стартой точки в классе поля. Обновляет словарь в конце
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public void RedrawFieldsViewports()
        {
            for (int i = 0; i < Fields.Count;i++)
            {
                Field f = Fields[i];

                double EndPoint = (f.StartPoint.X + f.DownScaleSize.Width + config.BorderValueLayout);

                if (i == 0 && new Point2d(EndPoint, f.StartPoint.Y) != distribution.EndsPointsFields[f.Name])
                {
                    double CorrectX = f.StartPoint.X;
                    double CorrectY = f.StartPoint.Y;
                    
                    Point2d CorrectStartFieldPoint = new Point2d(CorrectX, CorrectY);
                    f.StartPoint = CorrectStartFieldPoint;
                    CreateLayoutModel.DeleteObjects(f.ContourPolyline);
                    f.Draw();

                    distribution.StartsPointsFields[f.Name] = CorrectStartFieldPoint;
                    distribution.EndsPointsFields[f.Name] = new Point2d(CorrectStartFieldPoint.X + f.DownScaleSize.Width, CorrectStartFieldPoint.Y);

                    foreach (ViewportInField vp in f.Viewports)
                    {
                        vp.StartPoint = CorrectStartFieldPoint;
                        CreateLayoutModel.DeleteObjects(vp.ContourPolyline);
                        vp.Draw();
                    }
                }
                else
                {
                    Field pastField = Fields[i - 1];

                    if (f.StartPoint.X - (pastField.StartPoint.X + pastField.DownScaleSize.Width + config.BorderValueLayout) != 0)
                    {
                        double CorrectX = pastField.StartPoint.X + pastField.DownScaleSize.Width + config.BorderValueLayout;
                        double CorrectY = pastField.StartPoint.Y;

                        Point2d CorrectStartFieldPoint = new Point2d(CorrectX, CorrectY);
                        f.StartPoint = CorrectStartFieldPoint;
                        CreateLayoutModel.DeleteObjects(f.ContourPolyline);
                        f.Draw();

                        distribution.StartsPointsFields[f.Name] = CorrectStartFieldPoint;
                        distribution.EndsPointsFields[f.Name] = new Point2d(CorrectStartFieldPoint.X + f.DownScaleSize.Width, CorrectStartFieldPoint.Y);

                        foreach (ViewportInField vp in f.Viewports)
                        {
                            vp.StartPoint = CorrectStartFieldPoint;
                            CreateLayoutModel.DeleteObjects(vp.ContourPolyline);
                            vp.Draw();
                        }
                    }
                }
            }
        }
    }
    

    /// <summary>
    /// Базовый класс
    /// </summary>
    public abstract class Element
    {
        // Конфигурационные параметры
        protected Config config;

        // Имя
        public string Name { get; set; }
        // Идентификатор
        public Identificator ID { get; private set; }
        // Размер полилинии
        public Size OriginalSize { get; set; }
        // Размер полилинии после уменьшения масштаба
        public Size DownScaleSize { get; set; }
        // Стартовая точка полилинии объекта
        public Point2d StartPoint { get; set; }
        // Конечная точка полилинии объекта
        public Point2d EndPoint { get; set; }
        // Тип рабочего объекта (макет или видовой экран)
        public WorkObject WorkObject { get; set; }
        // Стартовая точка
        public DistribitionOnModel Distribution { get; set; }
        // Состояние полилинии в пространстве модели
        public State StateInModel { get; set; }
        // Контур полилинии (макета или видового экрана) в пространстве модели
        public ObjectId ContourPolyline { get; set; }
        // Цвет контура полилинии   
        public int ColorIndex { get; set; }

        public Element(string Name)
        {
            this.ID = new Identificator();
            this.Name = Name;
            this.config = Config.GetConfig();
            this.StateInModel = State.NoExist;
        }

        /// <summary>
        /// Создать полилинию в пространстве модели (макета или видового экрана)
        /// </summary>
        public void Draw()
        {
            ContourPolyline = DrawRectangle(Distribution.CurrentStartPoint, DownScaleSize, ColorIndex);
            // Не имеет смысла ставить слой на созданную полилинию
            // SetLayer(ContourPolyline, Name);
            StateInModel = State.Exist;
        }

        /// <summary>
        /// Обновить размер полилинии (макета или видового экрана)
        /// </summary>
        public abstract void UpdatePolylineSize();
    }

    /// <summary>
    /// Макет
    /// </summary>
    public class Field : Element
    {
        // Плоттер
        public string Plotter { get; set; }
        // Формат макета
        public string Format { get; set; }
        // Видовые экраны
        public List<ViewportInField> Viewports { get; set; }

        public Field(string Name, string Plotter, string Format) : base(Name)
        {
            this.Plotter = Plotter;
            this.Format = Format;
            this.ColorIndex = config.ColorIndexForField;
            Viewports = new List<ViewportInField>();

            UpdatePolylineSize();

            this.Distribution = FieldDistributionOnModel.GetInstance();

            this.StartPoint = Distribution.GetStartPoint();
            this.EndPoint = new Point2d(StartPoint.X + DownScaleSize.Width, StartPoint.Y);

            Draw();
        }

        /// <summary>
        /// Получить размеры полилинии макета
        /// </summary>
        public override void UpdatePolylineSize()
        {
            // Получаем оригинальный масштаб
            OriginalSize = GetSizePaper(Format, Plotter);
            // Применяем уменьшающий коэффициент
            DownScaleSize = ApplyScaleToSizeObjectsInModel(OriginalSize, config.DownScale);
        }
    }

    /// <summary>
    /// Видовой экран
    /// </summary>
    public class ViewportInField : Element
    {
        // Аннотационный масштаб видового экрана
        public string AnnotationScale { get; set; }
        // Список объектов на пространстве модели, которые будут показаны на видовом экране
        public ObjectIdCollection ObjectIDs { get; set; }

        public ViewportInField(string Name, string AnnotationScale, ObjectIdCollection ObjectIDs, Field field) : base(Name)
        {
            this.AnnotationScale = AnnotationScale;
            this.ObjectIDs = ObjectIDs;
            this.ColorIndex = config.ColorIndexForViewport;
            UpdatePolylineSize();

            Distribution = new ViewportDistributionOnModel(field, this);

            this.StartPoint = Distribution.GetStartPoint();
            this.EndPoint = Distribution.GetEndPoint();

            Draw();
        }

        /// <summary>
        /// Получить размеры полилинии видового экрана
        /// </summary>
        public override void UpdatePolylineSize()
        {
            OriginalSize = CheckModelSize(ObjectIDs);
            DownScaleSize = ApplyScaleToSizeObjectsInModel(OriginalSize, AnnotationScale);
            DownScaleSize = ApplyScaleToSizeObjectsInModel(DownScaleSize, config.DownScale);
        }
    }

    /// <summary>
    /// Базовый класс распределения
    /// </summary>
    public abstract class DistribitionOnModel
    {
        public Point2d CurrentStartPoint { get; set; }
        public Point2d CurrentEndPoint { get; set; }
        public Config config { get; set; } = Config.GetConfig();

        public abstract Point2d GetStartPoint();
        public abstract Point2d GetEndPoint();

    }

    /// <summary>
    /// Лист без дубликатов
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FieldList<T> : List<T>
    {
        private static FieldDistributionOnModel dist;

        private static FieldList<T> instance;
        public static FieldList<T> GetInstance()
        {
            if (instance == null)
            {
                instance = new FieldList<T>();
                dist = FieldDistributionOnModel.GetInstance();
            }
            return instance;
        }
        private FieldList()
        {

        }

        public new void Add(T Item)
        {
            if (!Contains(Item))
            {
                dist.StartsPointsFields.Add((Item as Field).Name, dist.GetStartPoint());
                dist.EndsPointsFields.Add((Item as Field).Name, dist.GetStartPoint());
                base.Add(Item);
            }
        }

    }

    /// <summary>
    /// Распределение макетов на пространстве модели
    /// </summary>
    public class FieldDistributionOnModel : DistribitionOnModel
    {
        /// <summary>
        /// Нужен класс, который будет отвечать за распределение макетов в пространстве модели. 
        /// Т.е. этот класс должен отвечать за предоставление данных о начальных точках для макетов, а также переставлять точки 
        /// в случае изменения параметров макета/макетов
        /// 
        /// Это значит что данный класс будет один, так как распределение макетов одно (в пространстве модели)
        /// 
        /// Данный класс будет сделан с помощью синглтона
        /// 
        /// Он реализует следующие функции:
        /// Хранить состояние параметров макетов (размер, стартовые точки, конечные точки)
        /// 
        /// Возвращать указанные состояния
        /// 
        /// Вернуть стартовую точку, исходя из параметров полей 
        /// 
        /// </summary>
        /// 

        public Dictionary<string, Point2d> StartsPointsFields = new Dictionary<string, Point2d>();
        public Dictionary<string, Point2d> EndsPointsFields = new Dictionary<string, Point2d>();

        private static FieldList<Field> fields;

        private static FieldDistributionOnModel instance;
        public static FieldDistributionOnModel GetInstance()
        {
            if (instance == null)
            {
                instance = new FieldDistributionOnModel();
                fields = FieldList<Field>.GetInstance();
            }


            return instance;
        }
        private FieldDistributionOnModel()
        {
        }

        public override Point2d GetStartPoint()
        {
            if (fields.Count == 0)
            {
                CurrentStartPoint = config.StartPointModel;
            }
            else
            {
                CurrentStartPoint = new Point2d(fields[fields.Count - 2].StartPoint.X + fields[fields.Count - 2].DownScaleSize.Width + config.BorderValueLayout, fields[fields.Count - 2].StartPoint.Y);
            }
            return CurrentStartPoint;
        }

        public override Point2d GetEndPoint()
        {
            CurrentEndPoint = new Point2d(CurrentStartPoint.X + fields[fields.Count - 1].DownScaleSize.Width, CurrentStartPoint.Y);
            return CurrentEndPoint;
        }
    }

    /// <summary>
    /// Распределение видовых экранов на пространстве модели в пределах полилинии одного макета
    /// </summary>
    public class ViewportDistributionOnModel : DistribitionOnModel
    {
        Field field;
        ViewportInField viewport;
        public ViewportDistributionOnModel(Field field, ViewportInField viewport)
        {
            this.field = field;
            this.viewport = viewport;
        }

        public override Point2d GetStartPoint()
        {
            return field.StartPoint;
        }

        public override Point2d GetEndPoint()
        {
            return new Point2d(field.StartPoint.X + viewport.DownScaleSize.Width, field.StartPoint.Y);
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
