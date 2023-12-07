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

        //"ISO_full_bleed_A5_(148.00_x_210.00_MM)"
        //"ISO_full_bleed_A4_(210.00_x_297.00_MM)"
        //"ISO_full_bleed_A3_(297.00_x_420.00_MM)"
        //"ISO_full_bleed_A2_(420.00_x_594.00_MM)"
        //"ISO_full_bleed_A1_(594.00_x_841.00_MM)"



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

            GetAllCanonicalScales();


            // Получаем выбранные объекты
            PromptSelectionResult select = AcEditor.SelectImplied();
            if (select.Status != PromptStatus.OK) return;
            ObjectIdCollection objectIds = new ObjectIdCollection(select.Value.GetObjectIds());


            //
            Size SizeModel = CheckModelSize(objectIds);
            Size SizeLayout = CheckSizeLayout("Лист2");


            bool checking = CheckSizeViewportOnSizeLayout("Лист2", SizeModel);
            if (!checking)
            {
                throw new System.Exception("Scale selected objects is too big for choicing layout!");
            }

            Point3d CenterModel = CheckCenterModel(objectIds);

            //
            ObjectId vpid = CreateViewport(widthObjectsModel: SizeModel.Width, heightObjectsModel: SizeModel.Height, layoutName: "Лист2",
            centerPoint: CenterModel, orientation: new Vector3d(0, 0, 1), StandardScaleType.Scale1To2);



            //
            //MoveSelectToVP(objectIds, vpID);

            // 

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                acTrans.Commit();
            }
        }

    }
}
