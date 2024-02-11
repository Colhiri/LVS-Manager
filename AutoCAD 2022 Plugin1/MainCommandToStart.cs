﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Linq;
using static AutoCAD_2022_Plugin1.CadUtilityLib;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using AutoCAD_2022_Plugin1;
using Field = AutoCAD_2022_Plugin1.Field;
using AutoCAD_2022_Plugin1.ViewModels;
using AutoCAD_2022_Plugin1.Views;
using AutoCAD_2022_Plugin1.Views.ManageViews;
using AutoCAD_2022_Plugin1.ViewModels.ManageVM;
using AutoCAD_2022_Plugin1.Services;

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

            /// Особая инициализация формы из клиентского кода 
            /// (сначала окно, потом передаем в VM параметр формы, потом грузим форму контекстом)
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

            try
            {
                Field field = FL.GetField(objectID);
                NameLayoutObjects = field.NameLayout;
                PlotterNameObjects = field.PlotterName;
                LayoutFormatObjects = field.LayoutFormat;
                TypeWorkObject = WorkObject.Layout;
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);

                try
                {
                    Field field = FL.GetNames().Select(x => FL.GetField(x)).Where(x => x.CheckViewport(objectID)).First();
                    NameLayoutObjects = field.NameLayout;
                    PlotterNameObjects = field.PlotterName;
                    LayoutFormatObjects = field.LayoutFormat;
                    AnnotationScaleObjects = field.GetViewport(objectID).AnnotationScaleViewport;
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
