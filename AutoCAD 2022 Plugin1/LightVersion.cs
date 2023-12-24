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
         * 
         * 
         * 
         * 
         */

        //"ISO_full_bleed_A5_(148.00_x_210.00_MM)"
        //"ISO_full_bleed_A4_(210.00_x_297.00_MM)"
        //"ISO_full_bleed_A3_(297.00_x_420.00_MM)"
        //"ISO_full_bleed_A2_(420.00_x_594.00_MM)"
        //"ISO_full_bleed_A1_(594.00_x_841.00_MM)"

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
            {
                throw new System.Exception("Invalid argument for create list.");
            }

            if (deviceName == "")
            {
                CreateLayout(nameLayout, canonicalScale);
            }
            else
            {
                CreateLayout(nameLayout, canonicalScale, deviceName);
            }
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
            // Сейчас стоит заглушка
            FieldList FL = new FieldList(new Point2d(0, 0));

            /// Получить аннотационные масштабы
            // ObjectContextCollection occ = OCM.GetContextCollection("ACDB_ANNOTATIONSCALES");
            // List<AnnotationScale> annoScales = occ.Cast<AnnotationScale>().ToList();
            // using (var writer = new StreamWriter("Scales.csv"))
            // using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            // {
            //     List<WrapInfoScale> lst = annoScales.Select(x => new WrapInfoScale(x.Name, x.Scale)).ToList();
            //     csv.WriteRecords(lst);
            // }

            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK) return;
            ObjectIdCollection objectIds = new ObjectIdCollection(select.Value.GetObjectIds());

            // Получаем имя листа на который хотим поместить объекты
            PromptStringOptions promptNameList = new PromptStringOptions("Enter name layout where you decide move objects: ");
            string resultNameList = AcEditor.GetString(promptNameList).StringResult;
            if (resultNameList == null)
                throw new System.Exception("Empty input.");
            if (!layManager.LayoutExists(resultNameList))
                throw new System.Exception($"{resultNameList} not exists in layouts list.");

            PromptStringOptions promptScaleObjects = new PromptStringOptions("Enter scale objects: Example \"1:4\"");
            string resultScale = AcEditor.GetString(promptScaleObjects).StringResult;
            if (resultScale == null)
                throw new System.Exception("Empty input.");
            // string resultScale = "1:4"; // Это имитация получения канонического масштаба от пользователя
            ///         public static LocationDraw StartLocationDrawing { get; private set; } = LocationDraw.TopLeft;

            // Добавлем новую филду
            Field field = FL.AddField(resultNameList) as Field;
            if (field == null) throw new ArgumentNullException();
            ViewportInField viewport = field.AddViewport(resultScale, objectIds);

            if (field.StateInModel == State.NoExist) 
            {
                field.Draw();
            }

            if (viewport.StateInModel == State.NoExist)
            {
                viewport.Draw();
            }



            /// Получаем масштаб будущего видового экрана Требуется создание WPF формы для передачи масштаба объектов
            // PromptStringOptions promptScaleObjects = new PromptStringOptions("Enter scale objects: ");
            // string resultScale = AcEditor.GetString(promptScaleObjects).StringResult;
            // if (resultScale == null)
            //     throw new System.Exception("Empty input.");

            /// Получаем стартовую точку для макета. А также пересчитываем размер выбранных в модели объектов
            // Point2d startPoint = GetStartPointDraw(objectIds);
            // Size SizeModel = CheckModelSize(objectIds);
            // Size SizeLayout = CheckSizeLayout(resultNameList);
            // Size newSizeModel = ApplyScaleToSizeObjectsInModel(SizeModel, resultScale);

            /// Проверка вхождения выбранных объектов на макет.
            // bool checking = CheckSizeViewportOnSizeLayout(SizeLayout, newSizeModel);
            // if (!checking) throw new System.Exception("Scale selected objects is too big for choicing layout!");
            
            /// Рисуем все сразу
            //CheckingResultDraw(resultNameList, SizeLayout, newSizeModel, startPoint);

        }

    }
}
