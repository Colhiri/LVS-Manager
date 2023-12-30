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
using System.Linq;
using System.Xml.Linq;
using Autodesk.AutoCAD.GraphicsSystem;

[assembly: CommandClass(typeof(LightProgram.LightVersion))]

namespace LightProgram
{
    public class LightVersion
    {
        /*
         * Должна быть функция внешнего кода, обрабатывающая ввод пользователя через LispFunction
         * 
         * Функции:
         * Создать лист с названием и выбранным размером
         * 
         * Проанализировать размеры выделенных объектов в модели
         * 
         * Проанализировать вхождение выделенных объектов на макет с возвратом bool с выбранным масштабом объектов
         * 
         * bool = true, построить над выделенными объектами прямоугольную область 1 / 10 от размера страницы и поместить туда прямоугольник размером 
         * с выделенные объекты на выбранном масштабе
         * 
         * bool = false, кинуть ошибку прямо в автокад (это как бы предупреждение, что все не очень хорошо)
         * 
         * 
         * Анализ области печати на листе и вывод области печати в рамку вхождения объектов на макет
         * 
         * 
         * Task 06 12 2023
         * 
         * Необходимо назначать слои, таким образом можно будет отделять "объекты" на листах
         * 
         * При назначении нового листа необходимо создавать новую область листа над объектами, которые относятся к этому листу
         * Это проще всего сделать событием -- проверка существования листа
         * 
         * Должна быть обертка класса в виде Field ()
         */

        //"ISO_full_bleed_A5_(148.00_x_210.00_MM)"
        //"ISO_full_bleed_A4_(210.00_x_297.00_MM)"
        //"ISO_full_bleed_A3_(297.00_x_420.00_MM)"
        //"ISO_full_bleed_A2_(420.00_x_594.00_MM)"
        //"ISO_full_bleed_A1_(594.00_x_841.00_MM)"

        public static FieldList FL = new FieldList(new Point2d(0.0, 0.0));

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

        [CommandMethod("TESTING", CommandFlags.UsePickSet)]
        public static void GetSizeLayoutWithoutLayout()
        {
            GetSizePaper("A4");
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
            TemporaryDataWPF tempData = new TemporaryDataWPF();
            tempData.PlotterName = "Нет";
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

            /*
            // Получаем имя листа на который будем перемещать объекты
            PromptStringOptions promptNameList = new PromptStringOptions("Enter new name layout: ");
            string resultNameList = AcEditor.GetString(promptNameList).StringResult;
            if (resultNameList == null)
                throw new System.Exception("Empty input.");
            // Получаем плоттер для листа
            PromptStringOptions promptPlotter= new PromptStringOptions("Enter plotter: ");
            string resultPlotter = AcEditor.GetString(promptPlotter).StringResult;
            if (resultPlotter == null)
                throw new System.Exception("Empty imput.");
            // Получаем формат страницы
            PromptStringOptions promptPaperFormat = new PromptStringOptions("Enter layout format: ");
            string resultPaperFormat = AcEditor.GetString(promptPaperFormat).StringResult;
            if (resultPaperFormat == null)
                throw new System.Exception("Empty imput.");
            // Получаем масштаб объектов в будущем видовом экране
            PromptStringOptions promptScaleObjects = new PromptStringOptions("Enter scale objects: Example \"1:4\"");
            string resultScale = AcEditor.GetString(promptScaleObjects).StringResult;
            if (resultScale == null)
                throw new System.Exception("Empty input.");
            */


            // Добавлем новую филду
            Field field = FL.AddField(resultNameLayout, resultLayoutFormat, resultPlotter) as Field;
            if (field == null) throw new ArgumentNullException();
            ViewportInField viewport = field.AddViewport(resultScale, objectIds);

            if (field.StateInModel == State.NoExist) 
                field.Draw();

            if (viewport.StateInModel == State.NoExist)
                viewport.Draw();
        }
    }
}
