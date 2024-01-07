using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using static AutoCAD_2022_Plugin1.Working_functions;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using AutoCAD_2022_Plugin1;
using Field = AutoCAD_2022_Plugin1.Field;

[assembly: CommandClass(typeof(LightProgram.LightVersion))]

namespace LightProgram
{
    public class LightVersion
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

            LayoutData tempData = new LayoutData();
            tempData.PlotterName = plotterNameFromConfig;
            ParametersLayout window = new ParametersLayout(tempData);
            if (Application.ShowModalWindow(window) != true) return;

            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK) return;
            ObjectIdCollection objectIds = new ObjectIdCollection(select.Value.GetObjectIds());

            string resultNameLayout = tempData.Name;
            string resultPlotter = tempData.PlotterName;
            string resultLayoutFormat = tempData.LayoutFormat;
            string resultScale = tempData.AnnotationScaleObjectsVP;

            // Добавлем новую филду
            Field field = FL.AddField(resultNameLayout, resultLayoutFormat, resultPlotter) as Field;
            if (field == null) throw new ArgumentNullException();
            ViewportInField viewport = field.AddViewport(resultScale, objectIds);
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
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK)
            {
                Application.ShowAlertDialog("Выберите объекты");
                select = AcEditor.GetSelection();

            }
            ObjectIdCollection objectIDs = new ObjectIdCollection(select.Value.GetObjectIds());

            // Создаем форму
            ManageData manageData = new ManageData();




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
