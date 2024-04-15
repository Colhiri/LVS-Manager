using AutoCAD_2022_Plugin1.Models;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
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

    public enum WorkObject
    {
        Field,
        Viewport,
        None
    }

    /// <summary>
    /// Конфигурация для параметров отрисовки объектов на полилинии
    /// </summary>
    public class Config
    {
        // Путь к конфигурации (Автоматический путь)
        private string PathToConfigFile = $"ConfigFile.json";

        // Единственный экземпляр класса (Синглтон)
        private static Config instance;
        
        // Цвет полилинии макета
        public int ColorIndexForField { get; set; }
        // Цвет полилинии видового экрана
        public int ColorIndexForViewport { get; set; }
        // Граница между полилинями макетов
        public double BorderValueLayout { get; set; }
        // Дополнительное общее уменьшение размера полилиний макетов и видовых экранов
        public string DownScale { get; set; }
        // Стартовый плоттер
        public string DefaultPlotter { get; set; }
        // Стартовая точка для полилиний
        public Point2d StartPointModel { get; set; }

        /// <summary>
        /// Получить текущий экземпляр класса
        /// </summary>
        /// <param name="PathToConfigFile"></param>
        /// <returns></returns>
        public static Config GetConfig()
        {
            if (instance == null)
            {
                instance = new Config();
            }
            return instance;
        }

        /// <summary>
        /// Закрытый конструктор для синглтона
        /// </summary>
        private Config() 
        {
            if (!File.Exists(PathToConfigFile))
            {
                Application.ShowAlertDialog("Configuration does not exists file. \nI will create a new file with default parameters!");
                DefaultInitialize();
                using (FileStream f = File.OpenWrite(PathToConfigFile)) 
                {
                    JsonSerializer.Serialize<Config>(f, this);
                }
            }
            else
            {
                using (FileStream f = File.OpenRead(PathToConfigFile))
                {
                    Config config = JsonSerializer.Deserialize<Config>(f);
                    InitializeWithCopy(config);
                }
            }
        }

        /// <summary>
        /// Задать стандартные параметры работы
        /// </summary>
        private void DefaultInitialize()
        {
            ColorIndexForField = 3;
            ColorIndexForViewport = 4;
            BorderValueLayout = 300;
            DownScale = "1:1";
            DefaultPlotter = "Нет";
            StartPointModel = new Point2d(0, 0);
        }

        /// <summary>
        /// Задает параметры из переданной копии класса (используется при инициализации после десериализации)
        /// </summary>
        /// <param name="config"></param>
        private void InitializeWithCopy(Config config)
        {
            ColorIndexForField = config.ColorIndexForField;
            ColorIndexForViewport = config.ColorIndexForViewport;
            BorderValueLayout = config.BorderValueLayout;
            DownScale = config.DownScale;
            DefaultPlotter = config.DefaultPlotter;
            StartPointModel = config.StartPointModel;
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
        // Тип рабочего объекта (макет или видовой экран)
        public WorkObject WorkObject { get; set; }
        // Стартовая точка
        public DistribitionOnModel Distribution { get; set; }
        // Состояние полилинии в пространстве модели
        public State StateInModel { get; set; }
        // Контур полилинии (макета или видового экрана) в пространстве модели
        public ObjectId ContourPolyline {  get; set; }
        // Цвет контура полилинии   
        public int ColorIndex { get; set; }

        public Element(string Name)
        {
            this.ID = new Identificator();
            this.Name = Name;
            this.config = Config.GetConfig();
        }

        /// <summary>
        /// Создать полилинию в пространстве модели (макета или видового экрана)
        /// </summary>
        public void Draw()
        {
            ContourPolyline = DrawRectangle(Distribution.StartPoint, DownScaleSize, ColorIndex);
            SetLayer(ContourPolyline, Name);
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
    public class FieldTest : Element
    {
        // Плоттер
        public string Plotter { get; set; }
        // Формат макета
        public string Format { get; set; }
        // Видовые экраны
        public List<ViewportTest> Viewports { get; set; }

        public FieldTest(string Name, string Plotter, string Format) : base(Name)
        {
            this.Plotter = Plotter;
            this.Format = Format;
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
    public class ViewportTest : Element
    {
        // Конфигурационные параметры 
        private Config config = Config.GetConfig();

        // Аннотационный масштаб видового экрана
        public string AnnotationScale { get; set; }
        // Список объектов на пространстве модели, которые будут показаны на видовом экране
        public ObjectIdCollection ObjectIDs { get; set; }

        public ViewportTest(string Name, string AnnotationScale, ObjectIdCollection ObjectIDs) : base(Name)
        {
            this.AnnotationScale = AnnotationScale;
            this.ObjectIDs = ObjectIDs;
            Distribution = ViewportDistributionOnModel.GetInstance();
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
        public Point2d StartPoint { get; set; }
        public Config Config { get; set; }
    }

    /// <summary>
    /// Распределение видовых экранов на пространстве модели в пределах полилинии одного макета
    /// </summary>
    public class ViewportDistributionOnModel : DistribitionOnModel
    {

        public static ViewportDistributionOnModel instance; 
        public static ViewportDistributionOnModel GetInstance()
        {
            if (instance == null)
            {
                instance = new ViewportDistributionOnModel();
            }
            return instance;
        }
        private ViewportDistributionOnModel()
        {

        }

    }

    /// <summary>
    /// Распределение макетов на пространстве модели
    /// </summary>
    public class FieldDistributionOnModel : DistribitionOnModel
    {
        public static FieldDistributionOnModel instance;
        public static FieldDistributionOnModel GetInstance()
        {
            if (instance == null)
            {
                instance = new FieldDistributionOnModel();
            }
            return instance;
        }
        private FieldDistributionOnModel()
        {

        }
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
        // Параметры размеров
        public static string DownScale { get; set; } = "1:1";

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
            DownScaleSizeLayout = ApplyScaleToSizeObjectsInModel(OriginalSizeLayout, FieldList.DownScale);
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
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithScaling, FieldList.DownScale);
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
            SizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(SizeObjectsWithScaling, FieldList.DownScale);
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
