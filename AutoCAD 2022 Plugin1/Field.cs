using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using static AutoCAD_2022_Plugin1.Working_functions;


namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// При обновлении области из автокада у нас просто рисуются прямоугольники в рандомных местах
    /// Это должен решать нахождения в базе автокада полилиний, которые были созданы DrawRectangle
    /// 
    /// А также нужно создать область рисования макета, в которой как раз и будут находиться 
    /// все объекты этого макета
    /// 
    /// Ну или проосто делать все через команды удаления
    /// 
    /// 
    /// Создай отдельные команды для взаимодействия и обновления Полей, макетов, видовых экранов
    /// Вынести команды удаления, обновления в отдельные автокадовские команды
    /// 
    /// </summary>


    public enum State
    {
        Exist,
        NoExist
    }


    /// <summary>
    /// Пока это главный класс
    /// </summary>
    public class FieldList
    {
        private List<Field> Fields { get; set; } = new List<Field>();
        public string CurrentLayout { get; private set; }
        internal static FieldList Current => new FieldList();

        public object AddField(string nameLayout)
        {
            if (Fields.Count == 0 || !Fields.Select(x => x.NameLayout).Contains(nameLayout))
            {
                Fields.Add(new Field(nameLayout));
            }
            return UpdateField(nameLayout);
        }

        public void DeleteField(string nameLayout)
        {
            Fields.Remove(Fields[Fields.Select(x => x.NameLayout).ToList().IndexOf(nameLayout)]);
        }
        private Field UpdateField(string nameLayout)
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
        public static string DownScale { get; set; } = "1:1";
        public static int IdMove { get; private set; } = 0;
        public int Id { get; private set; }
        public string NameLayout { get; set; }
        public Size OriginalSizeLayout { get; set; }
        public Size DownScaleSizeLayout { get; set; }
        public int CountViewport => Viewports.Count;
        public State stateInModel { get; set; } = State.NoExist;

        public Point2d StartPoint { get; private set; }
        private List<ViewportInField> Viewports { get; set; } = new List<ViewportInField>();

        /// Возможные поля
        public Extents2d Margins { get; set; }
        public Point2d StartMarginsPoint { get; set; }
        public Size SizeMargins { get; set; }
        // public string CanonicalPaperSize { get; set; }


        public Field(string NameLayout)
        {
            this.NameLayout = NameLayout;
            Id = IdMove++;
            // Получаем оригинальный масштаб
            OriginalSizeLayout = CheckSizeLayout(NameLayout);
            // Применяем уменьшающий коэффициент
            DownScaleSizeLayout = ApplyScaleToSizeObjectsInModel(OriginalSizeLayout, DownScale);
        }

        /// <summary>
        /// Добавление видового экрана, а также перерасчет некоторых важных параметров для Field
        /// </summary>
        /// <param name="AnnotationScaleViewport"></param>
        /// <param name="ObjectsId"></param>
        public ViewportInField AddViewport(string AnnotationScaleViewport, ObjectIdCollection ObjectsId)
        {
            
            // Добавляем стартовую точку
            if (stateInModel == State.NoExist)
            {
                StartPoint = GetStartPointDraw(ObjectsId);
                StartPoint = new Point2d(StartPoint.X - DownScaleSizeLayout.Width * 0.5, StartPoint.Y);

                // Добавляем область печати на макете, стартовую точку и прямоугольный размер 
                Margins = GetMargins(NameLayout);
                StartMarginsPoint = new Point2d(StartPoint.X + Margins.MaxPoint.Y, StartPoint.Y + Margins.MaxPoint.X);
                SizeMargins = new Size(DownScaleSizeLayout.Width - Margins.MaxPoint.Y * 2, DownScaleSizeLayout.Height - Margins.MaxPoint.X * 2);
            }

            // Добавляем параметры видового экрана
            var viewport = new ViewportInField(AnnotationScaleViewport, ObjectsId, StartMarginsPoint);
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

        /// <summary>
        /// СДЕЛАЙ ВОЗВРАЩЕНИЕ OBJECTID СОЗДАННОЙ ПОЛИЛИНИИ
        /// СДЕЛАЙ ВОЗВРАЩЕНИЕ OBJECTID СОЗДАННОЙ ПОЛИЛИНИИ
        /// СДЕЛАЙ ВОЗВРАЩЕНИЕ OBJECTID СОЗДАННОЙ ПОЛИЛИНИИ
        /// </summary>
        /// <returns></returns>
        public object Draw()
        {
            // Рисуем макет
            DrawRectangle(StartPoint, DownScaleSizeLayout);
            // Рисуем границу
            DrawRectangle(StartMarginsPoint, SizeMargins);
            stateInModel = State.Exist;
            return null;
        }
    }

    /// <summary>
    /// Класс содержащий в себе информацию об отдельном видовом экране на макете
    /// </summary>
    public class ViewportInField
    {
        public static int IdMove { get; private set; } = 0;
        public int Id { get; private set; }
        public State stateInModel { get; set; } = State.NoExist;
        public string AnnotationScaleViewport { get; set; }
        public double CustomScaleViewport { get; set; }
        public Size sizeObjectsWithoutScale { get; private set; }
        public Size sizeObjectsWithScaling { get; private set; }
        public ObjectIdCollection ObjectsIDs { get; private set; }
        public Point2d StartDrawPointVP {  get; private set; }

        public ViewportInField(string AnnotationScaleViewport, ObjectIdCollection ObjectsId, Point2d StartDrawPointVP)
        {
            this.AnnotationScaleViewport = AnnotationScaleViewport;
            this.ObjectsIDs = ObjectsId;
            this.StartDrawPointVP = StartDrawPointVP;
            this.Id = IdMove++;

            sizeObjectsWithoutScale = CheckModelSize(ObjectsId);
            sizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(sizeObjectsWithoutScale, AnnotationScaleViewport);
            sizeObjectsWithScaling = ApplyScaleToSizeObjectsInModel(sizeObjectsWithScaling, Field.DownScale);
        }

        /// <summary>
        /// СДЕЛАЙ ВОЗВРАЩЕНИЕ OBJECTID СОЗДАННОЙ ПОЛИЛИНИИ
        /// СДЕЛАЙ ВОЗВРАЩЕНИЕ OBJECTID СОЗДАННОЙ ПОЛИЛИНИИ
        /// СДЕЛАЙ ВОЗВРАЩЕНИЕ OBJECTID СОЗДАННОЙ ПОЛИЛИНИИ
        /// </summary>
        /// <returns></returns>
        public object Draw()
        {
            // Рисуем контур объектов
            DrawRectangle(StartDrawPointVP, sizeObjectsWithScaling);
            stateInModel = State.Exist;
            return null;
        }
    }
}
