using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

using static AutoCAD_2022_Plugin1.Working_functions;



[assembly: CommandClass(typeof(AutoCAD_2022_Plugin1.CommandsTest))]

namespace AutoCAD_2022_Plugin1
{


    public class CommandsTest : Commands
    {
        /*
         * Ты остановился на масштабировании видового экрана. Научился переносить в реальном масштабе с модели на видовой экран
         * выбранные объекты.
         * 
         * Теперь нужно создать функцию, которая возвращает размеры листа
         * 
         * Потом функцию проверяющую вписывается ли текущий выбранный масштаб в выбранный масштаб листа
         * 
         * 
         */



        /// <summary>
        /// Шаблон для методов
        /// </summary>
        [CommandMethod("TEST", CommandFlags.UsePickSet)]
        public void Shablon()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) return;
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            // if (select.Status != PromptStatus.OK) return;
            //ObjectIdCollection objectIds = new ObjectIdCollection(select.Value.GetObjectIds());

            //
            //(double, double) SizeModel = CheckModelSize(objectIds);
            //Point3d CenterModel = CheckCenterModel(objectIds);
            //
            //ObjectId vpID = CreateViewport(width: SizeModel.Item1, height: SizeModel.Item2, layoutName: "Лист2",
            //centerPoint: CenterModel, orientation: new Vector3d(0, 0, 1));

            //
            //MoveSelectToVP(objectIds, vpID);

            // 
            CheckSizeLayout("Лист2");

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                acTrans.Commit();
            }
        }

    }
}
