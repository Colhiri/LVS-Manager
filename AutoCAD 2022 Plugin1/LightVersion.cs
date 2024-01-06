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
         * 
         * Сделать:
         * Функции апдейта полей и видовых экранов
         * Форма для взаимодействия с этими функциями.
         * Основная суть этой формы запустить команду Вызова формы - 
         * а) выбрать объекты (сразу или постфактум)
         * б) откроется менеджмент - поменять все как нужно
         * в) перерисовать все исходя из новых параметров --- НУЖЕН КЛАСС ПЕРЕОТРИСОВКИ И ПЕРЕРАСПРЕДЕЛЕНИЯ ЗНАЧЕНИЙ
         * 
         * 
         */

        public static LocationDraw StartLocationDrawing { get; private set; } = LocationDraw.TopLeft;

        /// <summary>
        /// (CreateLayoutUser "LAYOUT" "ISO_full_bleed_A4_(210.00_x_297.00_MM)")
        /// (CreateLayoutUser "LAYOUT" "A4")
        /// </summary>
        /// <param name="rbArgs"></param>
        [LispFunction("CreateLayoutUser")]
        public static void CreateLayoutUser(ResultBuffer rbArgs)
        {
            string nameLayout = "";
            string canonicalScale = "";
            string deviceName = "";

            int count = 0;
            foreach (var arg in rbArgs)
            {
                if (arg.TypeCode == (int)LispDataType.Text)
                {
                    switch (count++)
                    {
                        case 0:
                            nameLayout = arg.Value.ToString();
                            break;
                        case 1:
                            canonicalScale = arg.Value.ToString();
                            break;
                        case 2:
                            deviceName = arg.Value.ToString();
                            break;
                    }
                }
            }

            if (nameLayout == "" || canonicalScale == "")
                throw new System.Exception("Invalid argument for create list.");
            if (deviceName == "")
                CreateLayout(nameLayout, canonicalScale);
            else
                CreateLayout(nameLayout, canonicalScale, deviceName);
        }

        /// <summary>
        /// 
        /// </summary>
        [CommandMethod("DeleteField")]
        public static void DeleteField()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            ObjectContextManager OCM = AcDatabase.ObjectContextManager;

            // Получаем имя листа на который хотим поместить объекты
            PromptStringOptions promptNameList = new PromptStringOptions("Enter name layout where you decide move objects: ");
            string resultNameList = AcEditor.GetString(promptNameList).StringResult;
            if (resultNameList == null)
                throw new System.Exception("Empty input.");
            if (!layManager.LayoutExists(resultNameList))
                throw new System.Exception($"{resultNameList} not exists in layouts list.");

            FL.DeleteField(resultNameList);
        }

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

            // Нужно сделать функцию которая выдает точку на основе всех объектов и выбранного перечисления по расположению макетов
            // Сейчас стоит заглушка
            // Сейчас стоит заглушка
            // Сейчас стоит заглушка

            // Создаем форму
            // Нужно сделать подгрузку конфига для того, чтобы сразу было можно настроить глобальные параметры плагина, например, где 
            // располагаются точки, какой начальный принтер.
            // Здесь должен располагаться конфиг, откуда все подгружается
            // Имитация
            FieldList.ColorIndexForField = 3;
            FieldList.ColorIndexForViewport = 4;
            FL.StartPoint = new Point2d(0.0, 0.0);
            string plotterNameFromConfig = "Нет";

            TemporaryDataWPF tempData = new TemporaryDataWPF();
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

            if (field.StateInModel == State.NoExist) 
                field.Draw();

            if (viewport.StateInModel == State.NoExist)
                viewport.Draw();
        }


        // Исправь ошибку с крашем автокада, когда ничего не выбирается в имени макета
        // Исправь ошибку с крашем автокада, когда ничего не выбирается в имени макета
        // Исправь ошибку с крашем автокада, когда ничего не выбирается в имени макета
        // Исправь ошибку с крашем автокада, когда ничего не выбирается в имени макета


        /// <summary>
        /// Тест работы второго окна и его функций
        /// </summary>
        [CommandMethod("zoomtest", CommandFlags.UsePickSet)]
        public static void zoomtest()
        {
            Document acDoc = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database acDatabase = acDoc.Database;
            Editor acEditor = acDoc.Editor;

            PromptSelectionResult select = acEditor.SelectImplied();
            if (select.Status != PromptStatus.OK) return;
            ObjectIdCollection objectsIDs = new ObjectIdCollection(select.Value.GetObjectIds());

            ZoomToObjects(objectsIDs);
        }
    }
}
