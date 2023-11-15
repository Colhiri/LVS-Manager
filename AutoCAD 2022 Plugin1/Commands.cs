using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

[assembly: CommandClass(typeof(AutoCAD_2022_Plugin1.Commands))]

namespace AutoCAD_2022_Plugin1
{
    /*
     * Необходимо:
     * Создание и удаление листов
     * Копирование листов
     * Создание листов с предварительным оформлением
     * Запуск в реал тайме
     * Анализ области выделения (вытаскивание всех ВЫБРАННЫХ объектов в области ИЗ базы данных)
     * Сохранение объектов, их ID, наименования базы данных объекта
     * Помечать выделенную область командой вверху над первым сверху выделенным объектом (слоем или чем-то другим), после допустим команды LoadObjectsInTemp
     * Возможность масштабирования объектов на чертеже
     * Возможность рисовать прямоугольники
     * 
     * 
     * Команда LoadObjects - загружает выделенные объекты в базу данных присваивая им главное ID
     * Команда RemoveObjects - удаляет слои по главному ID
     * Команда PlaceObjectsInField - размещает объекты из temp database в прямоугольнике заданного размера
     * 
     * ID - должно быть автоматическое от 1 до бесконечности )))
     * 
     * Команды если работать со своей базой данных (Remove - по ID, Switch - по двум ID, Load - по одному ID)
     * 
     * Важные параметры:
     * ObjectID.ObjectClass - главный класс объекта
     * ObjectID.ObjectClass.AppName - имя главного класса?
     * ObjectID.ObjectClass.DxfName - Имя как в автокаде
     * ObjectID.ObjectClass.Name - имя в Api
     * 
     * 
     * Подумал о том, что можно создать класс Field - это поле объектов, которое обладает методами перемасштабирования, перемещения, 
     * скачком по layouts, удаления, и тд. 
     * 
     * 
    */

    public class Commands
    {

        private static List<ObjectId> CreateObjectID = new List<ObjectId>();

        /// <summary>
        /// Выбирает объекты после команды SHOWSELECTED в консоли Autocad
        /// </summary>
        [CommandMethod("ShowSelected")]
        public static void ShowSelected()
        {

            Document doc = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptSelectionResult result = ed.GetSelection();

            ObjectId[] idS;

            if (result.Status == PromptStatus.OK)
            {

                idS = result.Value.GetObjectIds();

                foreach (ObjectId id in idS)
                {
                    ed.WriteMessage($"Select ID: {id}");
                }
            }

            else
            {
                ed.WriteMessage($"No selected objects.");
                return;
            }
        }


        /// <summary>
        /// Просмотр существующих листов
        /// </summary>
        [CommandMethod("ShowLayouts")]
        public void ShowLayouts()
        {
            Document doc = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                DBDictionary layoutsID = trans.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

                ObjectId[] obkIDs;

                int count = 0;
                foreach (var layout in layoutsID)
                {
                    ed.WriteMessage($"Layout -- Key: {layout.Key} \n");
                }

                trans.Dispose();
            }
        }

        /// <summary>
        /// Удалить ЖЕСТКО ЗАКОДИРОВАННЫЙ ЛИСТ
        /// </summary>
        [CommandMethod("DeleteLayout")]
        public void DeleteLayout()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;

            using (Transaction AcTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                string nameToDelete = "Лист2";

                DBDictionary dBDictionaryEntries = AcTrans.GetObject(AcDatabase.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

                if (dBDictionaryEntries.Contains(nameToDelete))
                {
                    LayoutManager.Current.DeleteLayout(nameToDelete);

                    AcTrans.Dispose();
                }
                else
                {
                    AcTrans.Dispose();
                    return;
                }

                AcTrans.Dispose(); 
            }
        }


        [CommandMethod("CreateLayout")]
        public void CreateLayout()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            AcEditor.WriteMessage($"{layoutManager.LayoutCount}");

            ObjectId id = layoutManager.CreateLayout("LIST");

            CreateObjectID.Add(id);

        }

        [CommandMethod("ShowCreateID")]
        public void ShowCreated()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;

            foreach (ObjectId id in CreateObjectID)
            {
                AcEditor.WriteMessage($"{id}\n");
            }
        }


        /// <summary>
        /// Удаляет выбранные объекты из чертежа при помощи выделения до использования команды
        /// </summary>
        [CommandMethod("RemoveSelected", CommandFlags.UsePickSet)]
        public static void Remove()
        {
            Document doc = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            PromptSelectionResult result = ed.SelectImplied();

            ObjectId[] objectsID = new ObjectId[0];

            if (result.Status == PromptStatus.OK)
            {
                objectsID = ed.SelectImplied().Value.GetObjectIds();
            }
            else
            {
                ed.WriteMessage($"Bad status in selected implied -- {result.Status}. Try Selection Again.");
                return;
            }

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                Entity objToRemove;

                foreach (ObjectId id in objectsID)
                {
                    objToRemove = acTrans.GetObject(id, OpenMode.ForWrite) as Entity;

                    try
                    {
                        objToRemove.Erase();

                        ed.WriteMessage($"Layer with ID {objectsID[0]} was erased.");

                        acTrans.Commit();

                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ed.WriteMessage($"Some error: {ex}.");
                    }
                }
                
                acTrans.Dispose();
            }

        }

    }
}
