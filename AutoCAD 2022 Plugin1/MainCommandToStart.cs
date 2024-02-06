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
            FieldList.ColorIndexForField = 3;
            FieldList.ColorIndexForViewport = 4;
            FL.StartPoint = new Point2d(0.0, 0.0);
            string plotterNameFromConfig = "Нет";

            /// инициализация формы из клиентского кода 
            CreateLayoutVM tempData = new CreateLayoutVM();
            tempData.PlotterName = plotterNameFromConfig;
            CreateLayoutView window = new CreateLayoutView(tempData);
            window.DataContext = tempData;

            if (Application.ShowModalWindow(window) != true) return;

            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK)
            {
                Application.ShowAlertDialog("Объекты не выбраны! Заново!");
                // select = AcEditor.GetSelection();
            }
            ObjectIdCollection objectsIDs = new ObjectIdCollection(select.Value.GetObjectIds());

            string resultNameLayout = tempData.Name;
            string resultPlotter = tempData.PlotterName;
            string resultLayoutFormat = tempData.LayoutFormat;
            string resultScale = tempData.AnnotationScaleObjectsVP;

            // Добавлем новую филду
            Field field = FL.AddField(resultNameLayout, resultLayoutFormat, resultPlotter) as Field;
            if (field == null) throw new ArgumentNullException();
            ViewportInField viewport = field.AddViewport(resultScale, objectsIDs);
            //viewport.ChangeStartPoint(new Point2d(0, 0));

            if (field.StateInModel == State.NoExist) 
                field.Draw();

            if (viewport.StateInModel == State.NoExist)
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

            foreach (string NameField in FL.GetNames())
            {
                Field field = FL.GetField(NameField);
                if (field.ContourField == objectID)
                {
                    NameLayoutObjects = field.NameLayout;
                    PlotterNameObjects = field.PlotterName;
                    LayoutFormatObjects = field.LayoutFormat;
                    TypeWorkObject = WorkObject.Field;
                    break;
                }
                else
                {
                    foreach (Identificator id in field.ViewportIdentificators())
                    {
                        ViewportInField vp = field.GetViewport(id);

                        if (vp.ContourObjects == objectID)
                        {
                            NameLayoutObjects = field.NameLayout;
                            PlotterNameObjects = field.PlotterName;
                            LayoutFormatObjects = field.LayoutFormat;
                            AnnotationScaleObjects = vp.AnnotationScaleViewport;
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
            manageData.Name = NameLayoutObjects;
            manageData.LayoutFormat = LayoutFormatObjects;
            manageData.PlotterName = PlotterNameObjects;
            manageData.AnnotationScaleObjectsVP = AnnotationScaleObjects;

            ManageLayoutViewportView window = new ManageLayoutViewportView(manageData);
            window.DataContext = manageData;
            
            if (Application.ShowModalWindow(window) != true) return;

        }

        /// <summary>
        /// Тест работы второго окна и его функций
        /// </summary>
        [CommandMethod("zoomtest", CommandFlags.UsePickSet)]
        public static void zoomtest()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            ObjectContextManager OCM = AcDatabase.ObjectContextManager;

            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK)
            {
                Application.ShowAlertDialog("Выберите объекты");
                select = AcEditor.GetSelection();
            }
            ObjectIdCollection objectsIDs = new ObjectIdCollection(select.Value.GetObjectIds());

            AcEditor.WriteMessage("Objects selected");


            // ZoomToObjects(objectsIDs);
        }
    }
}
