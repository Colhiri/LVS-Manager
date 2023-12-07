using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using static Autodesk.AutoCAD.Windows.Window;
using static AutoCAD_2022_Plugin1.Working_functions;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using CsvHelper;
using System.Globalization;
using System.IO;
using AutoCAD_2022_Plugin1;

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
        [CommandMethod("SelectCheckingUser", CommandFlags.UsePickSet)]
        public static void SelectCheckingUser()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            ObjectContextManager OCM = AcDatabase.ObjectContextManager;

            /// Получить аннотационные масштабы
            ObjectContextCollection occ = OCM.GetContextCollection("ACDB_ANNOTATIONSCALES");

            Size SizeLayoutTest = CheckSizeLayout("Лист1");


            List<AnnotationScale> annoScales = occ.Cast<AnnotationScale>().ToList();

            using (var writer = new StreamWriter("Scales.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                List<WrapInfoScale> lst = annoScales.Select(x => new WrapInfoScale(x.Name, x.Scale)).ToList();

                csv.WriteRecords(lst);
            }

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

            // Получаем масштаб будущего видового экрана
            // Получаем масштаб будущего видового экрана
            // Получаем масштаб будущего видового экрана
            // Требуется создание WPF формы для передачи масштаба объектов

            // PromptStringOptions promptScaleObjects = new PromptStringOptions("Enter scale objects: ");
            // string resultScale = AcEditor.GetString(promptScaleObjects).StringResult;
            // if (resultScale == null)
            //     throw new System.Exception("Empty input.");
            string resultScale = "1:4"; // Это имитация получения канонического масштаба от пользователя

            Point2d startPoint = GetStartPointDraw(objectIds);

            Size SizeModel = CheckModelSize(objectIds);
            Size SizeLayout = CheckSizeLayout(resultNameList);
            Size newSizeModel = ApplyScaleToSizeObjectsInModel(SizeModel, resultScale);

            bool checking = CheckSizeViewportOnSizeLayout(SizeLayout, newSizeModel);
            if (!checking)
            {
                throw new System.Exception("Scale selected objects is too big for choicing layout!");
            }
            CheckingResultDraw(resultNameList, SizeLayout, newSizeModel, startPoint);

        }

    }
}
