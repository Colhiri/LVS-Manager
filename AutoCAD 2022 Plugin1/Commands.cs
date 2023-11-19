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
     * Сначала создается лист с параметрами А4 например
     * Выбирается глобальный масштаб видовый экранов на макете ИЛИ выбирается отдельно при выделении объектов
     * 
     * При выделении объектов им можно назначить масштаб видового экрана, размер видового экрана
     * 
     * Что если получать значения размеров объектов по крайним точкам и собственно таким образом получить объект прямоугольника, который будет являться истинным размеров выделенной области на макете
     * 
     * 
     * 
    */
    public class Commands
    {

        /// <summary>
        /// Шаблон для методов
        /// </summary>
        [CommandMethod("Shablon")]
        private void Shablon()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) return;
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {


                acTrans.Commit();

                acTrans.Dispose();
            }
        }



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
        /// Перемещение объектов НА ВИДОВОЙ ЭКРАН
        /// </summary>
        [CommandMethod("MoveToLayout", CommandFlags.UsePickSet)]
        public void MoveToLayout()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) return;
            
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;


            ObjectIdCollection objectIDs;
            PromptSelectionResult selectResult = AcEditor.SelectImplied();
            if (selectResult.Status != PromptStatus.OK) return;
            
            objectIDs = new ObjectIdCollection(selectResult.Value.GetObjectIds());

            test(ref objectIDs);


            //string nameLayout = "Лист2";
            //ObjectId layoutID = layoutManager.GetLayoutId(nameLayout);

            //using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            //{
            //    // Получаем коробку с выбранными объектами из МОДЕЛИ
            //    Extents3d ext = new Extents3d();
            //    foreach (ObjectId id in objectIDs)
            //    {
            //        var entity = acTrans.GetObject(id, OpenMode.ForRead) as Entity;
            //        if (entity != null)
            //        {
            //            ext.AddExtents(entity.GeometricExtents);
            //        }
            //    }

            //    // Получаем список видовых экранов на выбранном ЛИСТЕ
            //    double modelScrRatio;
            //    Layout layout = acTrans.GetObject(layoutID, OpenMode.ForRead) as Layout;
            //    ObjectIdCollection viewColl = layout.GetViewports();

            //    for (int i = 1; i < viewColl.Count; i++)
            //    {
            //        Viewport vp = acTrans.GetObject(viewColl[i], OpenMode.ForRead) as Viewport;

            //        modelScrRatio = vp.Width / vp.Height;
            //        // Преобразование матрицы DCS to WCS
            //        Matrix3d matWCS2DCS;
            //        // Преобразование плоскости в мировые координаты
            //        matWCS2DCS = Matrix3d.PlaneToWorld(vp.ViewDirection);
            //        // Возвращение вектора смещения
            //        matWCS2DCS = Matrix3d.Displacement(vp.ViewTarget - Point3d.Origin) * matWCS2DCS;
            //        // Возвращение матрицы поворота на угол вокруг оси с вектором оси через точку центра
            //        matWCS2DCS = Matrix3d.Rotation(-vp.TwistAngle, vp.ViewDirection, vp.ViewTarget) * matWCS2DCS;
            //        // Возвращение обратной матрицы
            //        matWCS2DCS.Inverse();
            //        ext.TransformBy(matWCS2DCS);

            //        double modelWidth = ext.MaxPoint.X - ext.MinPoint.X;
            //        double modelHeight = ext.MaxPoint.Y - ext.MinPoint.Y;

            //        Point2d modelCenter = new Point2d((ext.MaxPoint.X + ext.MinPoint.X) * 0.5,
            //                                          (ext.MaxPoint.Y + ext.MinPoint.Y) * 0.5);

            //        vp.UpgradeOpen();

            //        if (modelWidth > (modelHeight * modelScrRatio))
            //        {
            //            modelHeight = modelWidth / modelScrRatio;
            //        }
            //        vp.ViewHeight = modelHeight * 1.01;
            //        vp.ViewCenter = modelCenter;
            //        vp.On = true;
            //        vp.StandardScale = StandardScaleType.Scale1To50;
            //        vp.UpdateDisplay();

            //        vp.On = false;
            //        vp.StandardScale = StandardScaleType.Scale1To50;
            //        vp.UpdateDisplay();

            //    }

            //    acTrans.Commit();

            //}
        }


        public static void test(ref ObjectIdCollection ids)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                Extents3d ext = new Extents3d();
                foreach (ObjectId id in ids)
                {
                    var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent != null)
                    {
                        ext.AddExtents(ent.GeometricExtents);
                    }
                }

                LayoutManager layman = LayoutManager.Current;

                DBDictionary layoutDic = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

                double mScrRatio;
                // Проходимся по всем листам (модель пропускаем)
                foreach (DBDictionaryEntry entry in layoutDic)
                {
                    Layout layout = tr.GetObject(entry.Value, OpenMode.ForRead) as Layout;

                    if (!layout.ModelType) // Это не модель
                    {
                        layman.CurrentLayout = layout.LayoutName;
                        ObjectIdCollection idsVports = layout.GetViewports();
                        // Проходимся по всем видовым экранам (кроме основного видового экрана листа)
                        for (int i = 1; i < idsVports.Count; i++)
                        {
                            Viewport vp = (Viewport)tr.GetObject(idsVports[i], OpenMode.ForRead);
                            mScrRatio = (vp.Width / vp.Height);
                            // prepare Matrix for DCS to WCS transformation
                            Matrix3d matWCS2DCS;
                            matWCS2DCS = Matrix3d.PlaneToWorld(vp.ViewDirection);
                            matWCS2DCS = Matrix3d.Displacement(vp.ViewTarget - Point3d.Origin) * matWCS2DCS;
                            matWCS2DCS = Matrix3d.Rotation(-vp.TwistAngle, vp.ViewDirection, vp.ViewTarget) * matWCS2DCS;
                            matWCS2DCS = matWCS2DCS.Inverse();
                            ext.TransformBy(matWCS2DCS);
                            // width of the extents in current view
                            double mWidth;
                            mWidth = (ext.MaxPoint.X - ext.MinPoint.X);
                            // height of the extents in current view
                            double mHeight;
                            mHeight = (ext.MaxPoint.Y - ext.MinPoint.Y);
                            // get the view center point
                            Point2d mCentPt = new Point2d(
                              ((ext.MaxPoint.X + ext.MinPoint.X) * 0.5),
                              ((ext.MaxPoint.Y + ext.MinPoint.Y) * 0.5));
                            vp.UpgradeOpen();
                            if (mWidth > (mHeight * mScrRatio)) mHeight = mWidth / mScrRatio;
                            vp.ViewHeight = mHeight * 1.01;
                            // set the view center
                            vp.ViewCenter = mCentPt;
                            vp.Visible = true;
                            vp.On = true;
                            vp.UpdateDisplay();
                            // doc.Editor.SwitchToModelSpace();
                            // Application.SetSystemVariable("CVPORT", vp.Number);
                        }
                    }
                }
                tr.Commit();
            }
        }
    


        [CommandMethod("FourFloatingViewports")]
        public static void FourFloatingViewports()
        {
            // Get the current document and database, and start a transaction
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId,
                                             OpenMode.ForRead) as BlockTable;

                // Open the Block table record Paper space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace],
                                                OpenMode.ForWrite) as BlockTableRecord;

                // Switch to the previous Paper space layout
                Application.SetSystemVariable("TILEMODE", 0);
                acDoc.Editor.SwitchToPaperSpace();

                Point3dCollection acPt3dCol = new Point3dCollection();
                acPt3dCol.Add(new Point3d(0, 0, 0));
                acPt3dCol.Add(new Point3d(50, 50, 0));
                acPt3dCol.Add(new Point3d(100, 50, 0));
                acPt3dCol.Add(new Point3d(100, 50, 0));

                Vector3dCollection acVec3dCol = new Vector3dCollection();
                acVec3dCol.Add(new Vector3d(0, 0, 1));
                acVec3dCol.Add(new Vector3d(0, 0, 1));
                acVec3dCol.Add(new Vector3d(0, 0, 1));
                acVec3dCol.Add(new Vector3d(0, 0, 1));

                double dWidth = 20;
                double dHeight = 20;

                Viewport acVportLast = null;
                int nCnt = 0;

                foreach (Point3d acPt3d in acPt3dCol)
                {
                    using (Viewport acVport = new Viewport())
                    {
                        acVport.CenterPoint = acPt3d;
                        acVport.Width = dWidth;
                        acVport.Height = dHeight;

                        // Add the new object to the block table record and the transaction
                        acBlkTblRec.AppendEntity(acVport);
                        acTrans.AddNewlyCreatedDBObject(acVport, true);

                        // Change the view direction
                        acVport.ViewDirection = acVec3dCol[nCnt];

                        // Enable the viewport
                        acVport.On = true;

                        // Record the last viewport created
                        acVportLast = acVport;

                        // Increment the counter by 1
                        nCnt = nCnt + 1;
                    }
                }

                if (acVportLast != null)
                {
                    // Activate model space in the viewport
                    // acDoc.Editor.SwitchToModelSpace();

                    // Set the new viewport current via an imported ObjectARX function
                    // acedSetCurrentVPort(acVportLast.UnmanagedObject);
                }

                // Save the new objects to the database
                acTrans.Commit();
            }
        }



        /// <summary>
        /// Перемещение объекта на лист БЕЗ ВИДОВОГО ЭКРАНА ПРОСТО ГРАНИЦАМ ПРЯМОУГОЛЬНИКА
        /// </summary>
        [CommandMethod("CloneToLayout", CommandFlags.UsePickSet)]
        public void CloneToLayout()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            Editor ed = doc.Editor;
            Database db = doc.Database;
            PromptSelectionResult rs = ed.SelectImplied();
            if (rs.Status != PromptStatus.OK) return;
            ObjectIdCollection ids = new ObjectIdCollection(rs.Value.GetObjectIds());

            Extents3d ext = new Extents3d();
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                foreach (ObjectId id in ids)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent != null)
                    {
                        ext.AddExtents(ent.GeometricExtents);
                    }
                }

                // Центральная точка габаритов выбранных примитивов.
                Point3d modelCenter = ext.MinPoint + (ext.MaxPoint - ext.MinPoint) * 0.5;
                double modelHeight = ext.MaxPoint.Y - ext.MinPoint.Y;
                double modelWidth = ext.MaxPoint.X - ext.MinPoint.X;

                LayoutManager layman = LayoutManager.Current;

                DBDictionary layoutDic = tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

                double mScale = 1;

                // Проходимся по всем листам (модель пропускаем)
                foreach (DBDictionaryEntry entry in layoutDic)
                {
                    Layout layout = tr.GetObject(entry.Value, OpenMode.ForRead) as Layout;

                    if (layout.LayoutName == "Лист2") // Это не модель
                    {
                        Extents3d extLayout = new Extents3d();
                        BlockTableRecord btrLayout = tr.GetObject(layout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                        // Находим габариты примитивов в листе - будем считать их рамкой листа.
                        foreach (ObjectId id in btrLayout)
                        {
                            Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                            if (ent != null && !(ent is Viewport))
                            {
                                extLayout.AddExtents(ent.GeometricExtents);
                            }
                        }
                        // Центральная точка габаритов выбранных примитивов в листе
                        Point3d layoutCenter = extLayout.MinPoint + (extLayout.MaxPoint - extLayout.MinPoint) * 0.5;
                        double layoutHeight = extLayout.MaxPoint.Y - extLayout.MinPoint.Y;
                        double layoutWidth = extLayout.MaxPoint.X - extLayout.MinPoint.X;

                        // Находим масштабный коэффициент.
                        mScale = layoutWidth / modelWidth;
                        // Если масштабированная высота модели превышает высоту листа,
                        // то в качестве масштаба используем коэффициент на основе высоты
                        if (mScale * modelHeight > layoutHeight)
                            mScale = layoutHeight / modelHeight;

                        // Матрица масштабирования относительно центра в модели 
                        Matrix3d matScale = Matrix3d.Scaling(mScale, modelCenter);
                        // Масштабируем старый центр в модели по этой матрице
                        Point3d modelCenterScaling = modelCenter.TransformBy(matScale);
                        // Находим матрицу переноса точки масштабированного центра в модели в точку
                        // центра листа
                        Matrix3d matDisp = Matrix3d.Displacement(layoutCenter - modelCenterScaling);
                        // Комбинированная матрица
                        Matrix3d matCompose = matScale.PreMultiplyBy(matDisp);

                        IdMapping idmap = new IdMapping();

                        btrLayout.UpgradeOpen();
                        db.DeepCloneObjects(ids, layout.BlockTableRecordId, idmap, false);
                        btrLayout.DowngradeOpen();
                        foreach (IdPair idpair in idmap)
                        {
                            if (idpair.IsPrimary)
                            {
                                Entity ent = tr.GetObject(idpair.Value, OpenMode.ForWrite) as Entity;
                                if (ent != null)
                                {
                                    // Трансформируем по комбинированной матрице
                                    ent.TransformBy(matCompose);
                                }
                            }
                        }
                    }

                }
                tr.Commit();
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


        /// <summary>
        /// Копировать жестко закодированный лист
        /// </summary>
        [CommandMethod("CopyLayout")]
        public void CopyLayout()
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            layoutManager.CopyLayout("Лист2", "Лист5");

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                acTrans.Dispose();
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

            // Добавление в глобальный статический список
            CreateObjectID.Add(id);

        }


        /// <summary>
        /// Проверка работы статического списка в автокаде при загрузке DLL
        /// </summary>
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
