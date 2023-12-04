using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Internal;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Runtime;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.InteropServices;
using static Autodesk.AutoCAD.Windows.Window;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCAD_2022_Plugin1
{
    public static class Working_functions
    {
        /// <summary>
        /// Создает видовой экран по заданным параметрам
        /// Creating viewport in set layout in set point
        /// 
        /// CreateViewport(width: dWidth, height: dHeight, layoutName: "Лист2",
        /// centerPoint: acPt3d, orientation: acVec3dCol[nCnt++]);
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="layoutName"></param>
        /// <param name="centerPoint"></param>
        /// <param name="orientation">Направление viewport 001 стандартный к модели</param>
        /// <returns>
        /// Creating viewport ID
        /// </returns>
        public static ObjectId CreateViewport(double widthObjectsModel, double heightObjectsModel, string layoutName, 
                                              Point3d centerPoint, Vector3d orientation, StandardScaleType scaleVP)
        {
            //ObjectId layoutID

            ObjectId viewportID;

            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;
            ObjectContextManager ocm = AcDatabase.ObjectContextManager;

            layoutManager.CurrentLayout = layoutName;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                Viewport viewport = new Viewport
                {
                    Width = widthObjectsModel,
                    Height = heightObjectsModel,
                    CenterPoint = centerPoint,
                };

                // Add new DBObject in Database
                // Set ObjectId Creating ViewPort
                viewportID = acBlkTblRec.AppendEntity(viewport);
                acTrans.AddNewlyCreatedDBObject(viewport, true);

                // Activate this parameters vork only drop DBObject in acDB (PS vp.ON only)
                viewport.ViewDirection = orientation;
                viewport.On = true;

                

                viewport.StandardScale = StandardScaleType.Scale1To1;

                acTrans.Commit();
                
            }

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                Viewport vp = acTrans.GetObject(viewportID, OpenMode.ForWrite) as Viewport;

                AnnotationScale dbAnnoScale = AcDatabase.Cannoscale;
                // AnnotationScale vpAnnScale = vp.AnnotationScale;
                // double AnnScale = vpAnnScale.Scale;

                // vp.AnnotationScale.Name = "1:4";
                double cstScaleVP = vp.CustomScale;



                vp.StandardScale = scaleVP;
                cstScaleVP = vp.CustomScale;



                acTrans.Commit();
            }

            return viewportID;
        }

        /// <summary>
        /// Проверяет вписывается ли размеры видового экрана в выбранный макет.
        /// </summary>
        /// <param name="nameLayout"></param>
        /// <param name="viewportSize"></param>
        /// <param name="borderLayout"></param>
        /// <returns>
        /// Layout size + border layout  >= Viewport size
        /// </returns>
        public static bool CheckSizeViewportOnSizeLayout(string nameLayout, Point2d viewportSize, double borderLayout = 5)
        {
            double height, width;
            (double, double) size = CheckSizeLayout(nameLayout);
            size.ToTuple().Deconstruct(out height, out width);
            Point2d layoutSize = new Point2d(height, width);

            if (layoutSize.X - layoutSize.X * (borderLayout / 100) < viewportSize.X ||
                layoutSize.Y - layoutSize.Y * (borderLayout / 100) < viewportSize.Y)
                return false;
            return true;
        }
        public static bool CheckSizeViewportOnSizeLayout(Point2d layoutSize, Point2d viewportSize, double borderLayout = 5)
        {

            if (layoutSize.X - layoutSize.X * (borderLayout / 100) < viewportSize.X ||
                layoutSize.Y - layoutSize.Y * (borderLayout / 100) < viewportSize.Y)
                return false;
            return true;
        }


        /// <summary>
        /// Позволяет узнать размеры выделенной области по углам объектов
        /// </summary>
        /// <returns>
        /// Return Width, Height
        /// </returns>
        public static (double, double) CheckModelSize(ObjectIdCollection ids)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            double ModelWidth;
            double ModelHeight;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                // Получаем геометрию выбранных объектов
                Extents3d ext = new Extents3d();
                foreach (ObjectId id in ids)
                {
                    ext.AddExtents((acTrans.GetObject(id, OpenMode.ForRead) as Entity).GeometricExtents);
                }

                ModelWidth = ext.MaxPoint.X - ext.MinPoint.X;
                ModelHeight = ext.MaxPoint.Y - ext.MinPoint.Y;

                acTrans.Abort();
            }

            return (ModelWidth, ModelHeight);
        }


        /// <summary>
        /// Позволяет центральную точку в выбранных объектах на модели
        /// </summary>
        /// <returns>
        /// Center Point
        /// </returns>
        public static Point3d CheckCenterModel(ObjectIdCollection ids)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            Point3d center;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                // Получаем геометрию выбранных объектов
                Extents3d ext = new Extents3d();
                foreach (ObjectId id in ids)
                {
                    ext.AddExtents((acTrans.GetObject(id, OpenMode.ForRead) as Entity).GeometricExtents);
                }

                center = new Point3d((ext.MaxPoint.X + ext.MinPoint.X) * 0.5,
                                     (ext.MaxPoint.Y + ext.MinPoint.Y) * 0.5,
                                     (ext.MaxPoint.Z + ext.MinPoint.Z) * 0.5);

                acTrans.Abort();
            }
            return center;
        }

        /// <summary>
        /// Переносит выбранные объекты на выбранный видовой экран
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="viewportID"></param>
        /// <exception cref="System.Exception"></exception>
        public static void MoveSelectToVP(ObjectIdCollection ids, ObjectId viewportID)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            double modelHeight;
            double modelWidth;
            double mScrRatio;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                Extents3d ext = new Extents3d();
                foreach (ObjectId id in ids)
                {
                    var ent = acTrans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent != null)
                    {
                        ext.AddExtents(ent.GeometricExtents);
                    }
                }

                Viewport vp = (Viewport)acTrans.GetObject(viewportID, OpenMode.ForRead);
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
                vp.ViewHeight = mHeight * 1.0; // 1.01
                // set the view center
                vp.ViewCenter = mCentPt;
                vp.Visible = true;
                vp.On = true;
                vp.UpdateDisplay();

                acTrans.Commit();
            }
        }


        /// <summary>
        /// Возвращает размер макета
        /// </summary>
        /// <param name="nameLayout"></param>
        /// <returns>
        /// Height, Weidth
        /// </returns>
        /// <exception cref="System.Exception"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static (double, double) CheckSizeLayout(string nameLayout)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            double layHeight;
            double layWidth;

            ObjectId layID = layManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;
                
                // Обновляем девайс печати для листа, потому что он не знает изначально что у него за форматы печати есть.
                PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;
                try
                {
                    pltValidator.SetPlotConfigurationName(layWrite, layWrite.PlotConfigurationName, null);
                }
                catch
                {
                    pltValidator.SetPlotConfigurationName(layWrite, "Default Windows System Printer.pc3", null);
                }

                layHeight = layWrite.PlotPaperSize.Y;

                layWidth = layWrite.PlotPaperSize.X;

                acTrans.Commit();
            }

            return (layHeight, layWidth);
        }

        /// <summary>
        /// Получаем все канонические масштабы в открытом чертеже
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllCanonicalScales(string deviceName = "Default Windows System Printer.pc3")
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;
            PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;

            string[] device = pltValidator.GetPlotDeviceList().Cast<string>().ToArray();
            if (!device.Contains(deviceName))
            {
                throw new System.Exception("Not found your device in device list in Autocad.");
            }

            string[] scales;

            using (PlotSettings pltSet = new PlotSettings(true))
            {
                pltValidator.SetPlotConfigurationName(pltSet, deviceName, null);
                scales = pltValidator.GetCanonicalMediaNameList(pltSet).Cast<string>().ToArray();
            }

            return scales;
        }


        ////// Функция, которая применяет стандартный масштаб к видовому экрану
        ////// 
        ////// 
        ////// 



        ////// Функция, которая применяет пользовательский масштаб к видовому экрану
        ////// 
        ////// 
        ////// 


        ////// Функция, которая проверяет границы на макете (их наличие и если да то сколько)
        ////// 
        ////// Нужна чтобы понимать как показывать прямоугольник над выделенной областью
        ////// а также чтобы правильно рассчитывать границы макета

        ////// Нужно будет перерабатывать функцию Создания и проверки видового экрана, так как эти функции не учитывают границы отхождения видового экрана
        ////// 
        ////// 
        ////// 
        ///


        public class WrapInfoScale
        {
            /// <summary>
            /// Класс обертки для получения аннотационного масштаба и его параметров
            /// </summary>
            public static int Id { get; set; } = 1;
            public string Name { get; set; }
            public double CustomScale { get; set; }
            public int FirstNumberScale {  get; set; }
            public int SecondNumberScale { get; set; }
            public StandardScaleType standardScale { get; set; }

            public WrapInfoScale(string Name, double CustomScale) 
            {
                this.Name = Name;
                int[] scales = Name.Split(':').Select(x => int.Parse(x)).ToArray();
                this.CustomScale = CustomScale; // scales[0] / scales[1];
                Id++;
                this.FirstNumberScale = scales[0];
                this.SecondNumberScale = scales[1];

                foreach (string std in Enum.GetNames(typeof(StandardScaleType)))
                {
                }
            }
        }




        /// <summary>
        /// Применение выбранного мастшаба на выбранные объекты в модели 
        /// </summary>
        public static Point2d ApplyScaleToSizeObjectsInModel(Point2d sizeObjects, string annotationScaleName)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            string nameLayout = layManager.CurrentLayout;

            ObjectId layID = layManager.GetLayoutId(nameLayout);

            double newWidth;
            double newHeight;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

                BlockTable acBlkTbl = acTrans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                Viewport viewport = new Viewport
                {
                    Width = sizeObjects.X,
                    Height = sizeObjects.Y,
                    CenterPoint = new Point3d(0, 0, 0),
                };

                ObjectContextManager ocm = AcDatabase.ObjectContextManager;
                ObjectContextCollection occ = ocm.GetContextCollection("ACDB_ANNOTATIONSCALES");


                List<ObjectContext> annoScales = new List<ObjectContext>();

                foreach (var oc in occ)
                {
                    annoScales.Add(oc);
                }


                // Add new DBObject in Database
                // Set ObjectId Creating ViewPort
                ObjectId viewportID = acBlkTblRec.AppendEntity(viewport);
                acTrans.AddNewlyCreatedDBObject(viewport, true);

                Viewport vp = acTrans.GetObject(viewportID, OpenMode.ForWrite) as Viewport;

                // Activate this parameters vork only drop DBObject in acDB (PS vp.ON only)
                List<AnnotationScale> annotation = occ.Cast<AnnotationScale>().ToList();

                AnnotationScale rightScale = annotation[0];

                foreach (AnnotationScale annoSC in annotation)
                {
                    if (annoSC.Name == annotationScaleName)
                    {
                        rightScale = annoSC;
                    }
                }

                vp.On = true;
                vp.AnnotationScale = rightScale;
                vp.CustomScale = rightScale.Scale;

                // vp.StandardScale = scaleObjects;
                // double cstScaleVP = vp.CustomScale;

                newWidth = vp.Width * rightScale.Scale;
                newHeight = vp.Height * rightScale.Scale;

                acTrans.Abort();
            }

            return new Point2d(newWidth, newHeight);
        }

        /// <summary>
        /// Если результат сравнения размеров пройден успешно, то рисуем прямоугольник, в котором будет "будущий" видовой экран
        /// </summary>
        public static void CheckingResultDraw(Point2d sizeLayout, Point2d sizeObjects, ObjectIdCollection objectIds)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            double heightObjects = sizeObjects.Y;
            double weightObjects = sizeObjects.X;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                Extents3d ext = new Extents3d();

                foreach (ObjectId id in objectIds)
                {
                    var GetObject = acTrans.GetObject(id, OpenMode.ForRead) as Entity;

                    if (GetObject != null)
                    {
                        ext.AddExtents(GetObject.GeometricExtents);
                    }
                }

                double startPointX = (ext.MaxPoint.X + ext.MinPoint.X) * 0.5;
                double startPointY = ext.MaxPoint.Y * 1.1;
                Point2d newSizeLayout = ApplyScaleToSizeObjectsInModel(sizeLayout, "1:4");
                startPointX = startPointX - newSizeLayout.X * 0.5;
                Point2d startPoint = new Point2d(startPointX, startPointY);
                Point2d secondPoint = new Point2d(startPointX, startPointY+newSizeLayout.Y);
                Point2d thirdPoint = new Point2d(startPointX + newSizeLayout.X, startPointY + newSizeLayout.Y);
                Point2d fouthPoint = new Point2d(startPointX + newSizeLayout.X, startPointY);
                Point2d fifthPoint = new Point2d(startPointX, startPointY);


                BlockTable blkTbl = acTrans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord records = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;


                /// Макет
                using (Polyline acPoly = new Polyline())
                {
                    acPoly.AddVertexAt(0, startPoint, 0, 0, 0);
                    acPoly.AddVertexAt(1, secondPoint, 0, 0, 0);
                    acPoly.AddVertexAt(2, thirdPoint, 0, 0, 0);
                    acPoly.AddVertexAt(3, fouthPoint, 0, 0, 0);
                    acPoly.AddVertexAt(4, fifthPoint, 0, 0, 0);


                    records.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);

                }

                Point2d startPointObj = new Point2d(startPointX, startPointY);
                Point2d secondPointObj = new Point2d(startPointX, startPointY + heightObjects);
                Point2d thirdPointObj = new Point2d(startPointX + weightObjects, startPointY + heightObjects);
                Point2d fouthPointObj = new Point2d(startPointX + weightObjects, startPointY);
                Point2d fifthPointObj = new Point2d(startPointX, startPointY);

                /// Объект
                using (Polyline acPoly = new Polyline())
                {
                    acPoly.AddVertexAt(0, startPointObj, 0, 0, 0);
                    acPoly.AddVertexAt(1, secondPointObj, 0, 0, 0);
                    acPoly.AddVertexAt(2, thirdPointObj, 0, 0, 0);
                    acPoly.AddVertexAt(3, fouthPointObj, 0, 0, 0);
                    acPoly.AddVertexAt(4, fifthPointObj, 0, 0, 0);

                    acPoly.Closed = true;


                    records.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);

                }

                acTrans.Commit();   
            }

        }




        /// <summary>
        /// Меняет масштаб листа на заданный стандартный масштаб
        /// </summary>
        /// <param name="nameLayout"></param>
        /// <param name="scale"></param>
        /// <exception cref="System.Exception"></exception>
        public static void SetSizeLayout(string nameLayout, StdScaleType scale)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            ObjectId layID = layManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

                PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;

                pltValidator.SetStdScaleType(layWrite, scale);

                acTrans.Commit();
            }
        }

        /// <summary>
        /// Меняет масштаб листа на заданный канонический масштаб печати
        /// </summary>
        /// <param name="nameLayout"></param>
        /// <param name="canonicalScale"></param>
        /// <exception cref="System.Exception"></exception>
        public static void SetSizeLayout(string nameLayout, string canonicalScale)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            ObjectId layID = layManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

                // Получаем значение девайса печати для макета
                string deviceName = layWrite.PlotConfigurationName;

                if (!layManager.LayoutExists(nameLayout))
                    throw new System.Exception($"Layout with name {nameLayout} already exists.");
                if (!GetAllCanonicalScales(deviceName).Contains(canonicalScale))
                    throw new System.Exception($"Canonical scale is wrong.");

                PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;

                pltValidator.SetPlotConfigurationName(layWrite, deviceName, null);
                pltValidator.SetCanonicalMediaName(layWrite, canonicalScale);

                acTrans.Commit();
            }
        }

        /// <summary>
        /// TEST TEST TEST
        /// </summary>
        /// <param name="nameLayout"></param>
        /// <exception cref="System.Exception"></exception>
        public static void TESTING(string nameLayout)
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            ObjectId layID = layManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

                PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;

                string check1 = layWrite.PlotConfigurationName;




                pltValidator.SetPlotConfigurationName(layWrite, "DWG To PDF.pc3", null);
                pltValidator.GetLocaleMediaName(layWrite, "ISO_full_bleed_A4_(210.00_x_297.00_MM)");
                // pltValidator.SetPlotConfigurationName(layWrite, deviceName, null);
                // pltValidator.SetCanonicalMediaName(layWrite, canonicalScale);

                acTrans.Commit();
            }
        }

        /// <summary>
        /// Создать макет с указанным именем и масштабом
        /// </summary>
        public static ObjectId CreateLayout(string nameLayout, string canonicalScale, string deviceName = "Default Windows System Printer.pc3")
        {
            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layManager = LayoutManager.Current;

            if (layManager.LayoutExists(nameLayout))
            {
                throw new System.Exception($"Layout with name {nameLayout} already exists.");
            }
            if (!GetAllCanonicalScales().Contains(canonicalScale))
            {
                throw new System.Exception($"Canonical scale is wrong.");
            }

            ObjectId id = layManager.CreateLayout(nameLayout);

            // Меняем масштаб листа на заданный масштаб выбранного девайса
            SetSizeLayout(nameLayout, canonicalScale);

            AcEditor.WriteMessage($"{nameLayout} created with scale {canonicalScale}");

            return id;

        }

    }
}
