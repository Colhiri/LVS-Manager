using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Linq;
using static AutoCAD_2022_Plugin1.CadUtilityLib;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using AutoCAD_2022_Plugin1;
using AutoCAD_2022_Plugin1.ViewModels;
using AutoCAD_2022_Plugin1.Views;
using AutoCAD_2022_Plugin1.Views.ManageViews;
using AutoCAD_2022_Plugin1.ViewModels.ManageVM;

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
        public static LocationDraw StartLocationDrawing { get; private set; } = LocationDraw.TopLeft;
        public static FieldList FL = FieldList.GetInstance();

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

            // Создаем форму
            // Нужно сделать подгрузку конфига для того, чтобы сразу было можно настроить глобальные параметры плагина, например, где 
            // располагаются точки, какой начальный принтер.
            // Здесь должен располагаться конфиг, откуда все подгружается
            // Имитация
            Point2d StartPoint = new Point2d(0, 0);
            int ColorIndexLayout = 3;
            string plotterNameFromConfig = "Нет";
            int ColorIndexViewport = 4;
            string DownScale = "1:1";
            FieldList FL = FieldList.GetInstance(StartPoint, ColorIndexLayout, ColorIndexViewport, DownScale);

            /// инициализация формы из клиентского кода 
            CreateLayoutVM tempData = new CreateLayoutVM();
            tempData.PlotterName = plotterNameFromConfig;
            CreateLayoutView window = new CreateLayoutView(tempData);

            if (Application.ShowModalWindow(window) != true) return;

            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK)
            {
                Application.ShowAlertDialog("Объекты не выбраны! Заново!");
                return;
            }
            ObjectIdCollection objectsIDs = new ObjectIdCollection(select.Value.GetObjectIds());

            string resultNameLayout = tempData.Name;
            string resultPlotter = tempData.PlotterName;
            string resultLayoutFormat = tempData.LayoutFormat;
            string resultScale = tempData.AnnotationScaleObjectsVP;
            string resultNameViewport = tempData.ViewportName;

            // Добавлем новую филду
            LayoutModel layout = FL.AddLayout(resultNameLayout, resultLayoutFormat, resultPlotter);
            if (layout == null) throw new ArgumentNullException();
            ViewportModel viewport = layout.AddViewport(resultNameViewport, resultScale, objectsIDs);
            //viewport.ChangeStartPoint(new Point2d(0, 0));

            if (layout.StateType == State.NoExist) 
                layout.Draw();

            if (viewport.StateType == State.NoExist)
                viewport.Draw();
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
            string AnnotationScaleObjects = null;
            WorkObject TypeWorkObject = WorkObject.None;

            try
            {
                LayoutModel field = FL.Fields.Where(x => x.ContourObject == objectID).First();
                NameLayoutObjects = field.Name;
                PlotterNameObjects = field.Plotter;
                LayoutFormatObjects = field.Format;
                TypeWorkObject = WorkObject.Layout;
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);

                try
                {
                    LayoutModel field = FL.Fields.Where(x => x.Viewports.Select(vp => vp.ContourObject).Contains(objectID)).First();
                    NameLayoutObjects = field.Name;
                    PlotterNameObjects = field.Plotter;
                    LayoutFormatObjects = field.Format;
                    AnnotationScaleObjects = field.Viewports.Where(x => x.ContourObject == objectID).First().Scale;
                    TypeWorkObject = WorkObject.Viewport;
                }
                catch (System.Exception ex2)
                {
                    Application.ShowAlertDialog($"Выбран неправильный объект. Заново. \n{ex2.Message}");
                    return;
                }
            }

            // Проверяем что можно работать с выбранным объектом
            if (TypeWorkObject == WorkObject.None)
            {
                Application.ShowAlertDialog("Выбран неправильный объект. Заново.");
                return;
            }

            /// Особая инициализация формы из клиентского кода 
            /// (сначала окно, потом передаем в VM параметр формы, потом грузим форму контекстом)
            MainManageWindow window = new MainManageWindow();
            MainManageVM manageData = new MainManageVM(window);
            window.DataContext = manageData;
            //manageData.Name = NameLayoutObjects;
            //manageData.LayoutFormat = LayoutFormatObjects;
            //manageData.PlotterName = PlotterNameObjects;
            //manageData.AnnotationScaleObjectsVP = AnnotationScaleObjects;
            if (Application.ShowModalWindow(window) != true) return;




        }

            ParametersLVS parameters = new ParametersLVS()
            {
                NameLayout = NameLayoutObjects,
                LayoutFormat = LayoutFormatObjects,
                PlotterName = PlotterNameObjects,
                AnnotationScaleObjectsVP = AnnotationScaleObjects,
            };
            MainManageVM manageData = new MainManageVM(parameters);
            MainManageWindow window = new MainManageWindow(manageData);
            if (Application.ShowModalWindow(window) != true) return;

        }
    }
}
