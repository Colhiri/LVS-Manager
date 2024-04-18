using System;
using static AutoCAD_2022_Plugin1.Working_functions;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using AutoCAD_2022_Plugin1;
using Field = AutoCAD_2022_Plugin1.Field;
using AutoCAD_2022_Plugin1.ViewModels;
using AutoCAD_2022_Plugin1.Views;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using AutoCAD_2022_Plugin1.ViewModels.ManageLV;
using System.Linq;
using AutoCAD_2022_Plugin1.LogicServices;

[assembly: CommandClass(typeof(LightProgram.MainCommandToStart))]

namespace LightProgram
{
    public class MainCommandToStart
    {
        /*
         * Конфиг:
         * Плоттер
         * Цвет поля
         * Цвет видового экрана
         * ??? Стандартный формат
         * ??? Опциональный масштаб (Он будет рассчитываться исходя из размера поля по высоте или ширине)
         * Глобальный параметр с указанием того, что делать с объектами которые не вписываются на лист
         * ??? DownScale
         * 
         * Сделать:
         * Функции апдейта полей и видовых экранов
         * Форма для взаимодействия с этими функциями.
         * Основная суть этой формы запустить команду Вызова формы - 
         * а) выбрать объекты (сразу или постфактум)
         * б) откроется менеджмент - поменять все как нужно
         * в) перерисовать все исходя из новых параметров --- НУЖЕН КЛАСС ПЕРЕОТРИСОВКИ И ПЕРЕРАСПРЕДЕЛЕНИЯ ЗНАЧЕНИЙ
         */

        /// <summary>
        /// 
        /// </summary>
        [CommandMethod("COLHIRU", CommandFlags.UsePickSet)]
        public static void COLHIRU()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            ObjectContextManager OCM = AcDatabase.ObjectContextManager;

            // Подгружаем конфигурацию параметров
            Config config = Config.GetConfig();

            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK)
            {
                Application.ShowAlertDialog("Объекты не выбраны! Заново!");
                return;
            }

            // инициализация формы из клиентского кода 
            CreateLayoutVM tempData = new CreateLayoutVM();
            tempData.PlotterName = config.DefaultPlotter;
            CreateLayoutView window = new CreateLayoutView(tempData);
            window.DataContext = tempData;

            if (Application.ShowModalWindow(window) != true) return;
            
            ObjectIdCollection objectsIDs = new ObjectIdCollection(select.Value.GetObjectIds());

            // Получаем данные из формы WPF
            string resultNameLayout = tempData.Name;
            string resultPlotter = tempData.PlotterName;
            string resultLayoutFormat = tempData.LayoutFormat;
            string resultScale = tempData.AnnotationScaleObjectsVP;
            string NameViewport = tempData.NameViewport;

            // Добавлем новую филду
            FL.Fields.Add(new Field(resultNameLayout, resultPlotter, resultLayoutFormat));
            Field field = FL.Fields.Where(x => x.Name == resultNameLayout).First();
            if (field == null) throw new ArgumentNullException();
            field.Viewports.Add(new ViewportInField(NameViewport, resultScale, objectsIDs, field));
        }

        [CommandMethod("Managing", CommandFlags.UsePickSet)]
        public static void ManagingLayVP()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            ObjectContextManager OCM = AcDatabase.ObjectContextManager;

            // Получаем выбранные объекты
            PromptEntityResult select = AcEditor.GetEntity("Выберите полилинию макета или видового экрана");
            if (select.Status != PromptStatus.OK)
            {
                Application.ShowAlertDialog("Объекты не выбраны. Заново.");
                return;
            }
            ObjectId objectID = select.ObjectId;
            // Получаем параметры выбранных объектов

            // Подумай над тем как лучше реализовать свойства массивов в публичном или приватном модификаторе.
            // Так ли важно их скрывать, если они все равно все изменяются

            string NameLayoutObjects = null;
            string PlotterNameObjects = null;
            string LayoutFormatObjects = null;
            string ViewportName = null;
            string AnnotationScaleObjects = null;
            WorkObject TypeWorkObject = WorkObject.None;

            foreach (Field field in FL.Fields)
            {
                if (field.ContourPolyline == objectID)
                {
                    NameLayoutObjects = field.Name;
                    PlotterNameObjects = field.Plotter;
                    LayoutFormatObjects = field.Format;
                    TypeWorkObject = WorkObject.Field;
                    break;
                }
                else
                {
                    foreach (ViewportInField vp in field.Viewports)
                    {
                        if (vp.ContourPolyline == objectID)
                        {
                            NameLayoutObjects = field.Name;
                            PlotterNameObjects = field.Plotter;
                            LayoutFormatObjects = field.Format;
                            ViewportName = vp.ID.ToString();
                            AnnotationScaleObjects = vp.AnnotationScale;
                            TypeWorkObject = WorkObject.Viewport;
                            break;
                        }
                    }
                }
            }

            // Проверяем что можно работать с выбранным объектом
            if (TypeWorkObject == WorkObject.None)
            {
                Application.ShowAlertDialog("Выбран неправильный объект. Заново.");
                return;
            }

            /// инициализация формы из клиентского кода 
            ManageLayoutViewportVM manageData = new ManageLayoutViewportVM();
            manageData.FieldName = NameLayoutObjects;
            manageData.LayoutFormat = LayoutFormatObjects;
            manageData.PlotterName = PlotterNameObjects;
            if (ViewportName != null)
            {
                manageData.ViewportId = ViewportName;
                manageData.AnnotationScaleObjectsVP = AnnotationScaleObjects;
            }
            ManageLayoutViewportView window = new ManageLayoutViewportView(manageData);
            window.DataContext = manageData;
            
            if (Application.ShowModalWindow(window) != true) return;
        }


        /// <summary>
        /// Запускает создание листов и видовых экранов в соответствии с их параметрами в массиве Fields / Viewports
        /// </summary>
        [CommandMethod("CreateFV")]
        public void CreateFV()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            ObjectContextManager OCM = AcDatabase.ObjectContextManager;

            // Цикл по массиву макетов 
            foreach (Field field in FL.Fields)
            {
                // Создать макет по параметрам
                CreateLayout(nameLayout: field.Name, canonicalScale: field.Format, deviceName: field.Plotter);

                // (вложенный цикл по массиву видовых экранов)
                foreach (ViewportInField vp in field.Viewports)
                {
                    Vector3d vector = new Vector3d(1, 0, 0);
                    // Создать на макете видовые экраны
                    ObjectId vpID = CreateViewport(widthObjectsModel: vp.DownScaleSize.Width, heightObjectsModel: vp.DownScaleSize.Height, layoutName: field.Name,
                                   centerPoint: CheckCenterModel(vp.ObjectIDs), orientation: vector, scaleVP: vp.AnnotationScale);

                    // Обозначить объекты на видовых экранах
                    MoveSelectToVP(vp.ObjectIDs, vpID);
                }
            }
        }
    }
}
