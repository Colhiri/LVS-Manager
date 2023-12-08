using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Просто удобные размеры объектов в прямоугольном формате
    /// </summary>
    public struct Size
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public Size(double Width, double Height)
        {
            this.Width = Width;
            this.Height = Height;
        }
    }

    /// <summary>
    /// Класс обертки для получения аннотационного масштаба и его параметров
    /// </summary>
    public class WrapInfoScale
    {
        public static int Id { get; set; } = 1;
        public string Name { get; set; }
        public double CustomScale { get; set; }
        public int FirstNumberScale { get; set; }
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

    public static class Working_functions
    {
        private static Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
        private static Database AcDatabase = AcDocument.Database;
        private static Editor AcEditor = AcDocument.Editor;
        private static LayoutManager layoutManager = LayoutManager.Current;
        private static ObjectContextManager OCM = AcDatabase.ObjectContextManager;
        private static PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;
        
        // TESTING
        // TESTING
        // TESTING
        // TESTING
        public static FieldList FL = FieldList.Current;



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
            if (AcDocument is null) throw new System.Exception("No active document!");

            ObjectId viewportID;

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
        public static bool CheckSizeViewportOnSizeLayout(string nameLayout, Size viewportSize, double borderLayout = 5)
        {
            Size layoutSize = CheckSizeLayout(nameLayout);

            if (layoutSize.Width - layoutSize.Width * (borderLayout / 100) < viewportSize.Width ||
                layoutSize.Height - layoutSize.Height * (borderLayout / 100) < viewportSize.Height)
                return false;
            return true;
        }
        public static bool CheckSizeViewportOnSizeLayout(Size layoutSize, Size viewportSize, double borderLayout = 5)
        {

            if (layoutSize.Width - layoutSize.Width * (borderLayout / 100) < viewportSize.Width ||
                layoutSize.Height - layoutSize.Height * (borderLayout / 100) < viewportSize.Height)
                return false;
            return true;
        }


        /// <summary>
        /// Позволяет узнать размеры выделенной области по углам объектов
        /// </summary>
        /// <returns>
        /// Return Width, Height
        /// </returns>
        public static Size CheckModelSize(ObjectIdCollection ids)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

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

            return new Size(ModelWidth, ModelHeight);
        }


        /// <summary>
        /// Позволяет центральную точку в выбранных объектах на модели
        /// </summary>
        /// <returns>
        /// Center Point
        /// </returns>
        public static Point3d CheckCenterModel(ObjectIdCollection ids)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

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
            if (AcDocument is null) throw new System.Exception("No active document!");

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
        public static Size CheckSizeLayout(string nameLayout)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            double layHeight;
            double layWidth;

            ObjectId layID = layoutManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;


                Extents2d ext = layWrite.PlotPaperMargins;

                // Обновляем девайс печати для листа, потому что он не знает изначально что у него за форматы печати есть.
                try
                {
                    pltValidator.SetPlotConfigurationName(layWrite, layWrite.PlotConfigurationName, layWrite.CanonicalMediaName);
                }
                catch
                {
                    pltValidator.SetPlotConfigurationName(layWrite, "Нет", "ISO_A4_(210.00_x_297.00_MM)");
                }

                // pltValidator.SetPlotWindowArea(layWrite, new Extents2d(new Point2d()));
                Extents2d ext2 = layWrite.PlotPaperMargins;

                layHeight = layWrite.PlotPaperSize.Y;

                layWidth = layWrite.PlotPaperSize.X;

                acTrans.Commit();
            }

            return new Size(layHeight, layWidth);
        }

        /// <summary>
        /// Получаем все канонические масштабы в открытом чертеже
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllCanonicalScales(string deviceName = "Нет")
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

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


        /// <summary>
        /// Применение выбранного мастшаба на выбранные объекты в модели 
        /// </summary>
        public static Size ApplyScaleToSizeObjectsInModel(Size sizeObjects, string annotationScaleName)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            string nameLayout = layoutManager.CurrentLayout;

            ObjectId layID = layoutManager.GetLayoutId(nameLayout);

            double newWidth;
            double newHeight;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

                BlockTable acBlkTbl = acTrans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                Viewport viewport = new Viewport
                {
                    Width = sizeObjects.Width,
                    Height = sizeObjects.Height,
                    CenterPoint = new Point3d(0, 0, 0),
                };

                ObjectContextCollection occ = OCM.GetContextCollection("ACDB_ANNOTATIONSCALES");

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

            return new Size(newWidth, newHeight);
        }

        /// <summary>
        /// Дает точку в которой будет нарисовано вхождение объектов на макет
        /// </summary>
        /// <param name="objectIds"></param>
        /// <returns></returns>
        public static Point2d GetStartPointDraw(ObjectIdCollection objectIds)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

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

                acTrans.Abort();

                return new Point2d(startPointX, startPointY);
            }
        }



        /// <summary>
        /// Дает размеры поля печати на листе
        /// </summary>
        /// <param name="objectIds"></param>
        /// <returns></returns>
        public static Extents2d GetMargins(string nameLayout)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            ObjectId layoutID = layoutManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                Layout layout = acTrans.GetObject(layoutID, OpenMode.ForRead) as Layout;

                Extents2d margins = layout.PlotPaperMargins;

                acTrans.Abort();

                return margins;
            }
        }

        /// <summary>
        /// Строим прямоугольники в модели
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="size"></param>
        public static void DrawRectangle(Point2d startPoint, Size size)
        {
            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                BlockTable blkTbl = acTrans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord records = acTrans.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                using (Polyline acPoly = new Polyline())
                {
                    acPoly.AddVertexAt(0, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(startPoint.X, startPoint.Y + size.Height), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(startPoint.X + size.Width, startPoint.Y + size.Height), 0, 0, 0);
                    acPoly.AddVertexAt(3, new Point2d(startPoint.X + size.Width, startPoint.Y), 0, 0, 0);
                    acPoly.AddVertexAt(4, new Point2d(startPoint.X, startPoint.Y), 0, 0, 0);

                    ObjectId polylineId = records.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                }
                acTrans.Commit();
            }
        }


        /// <summary>
        /// Если результат сравнения размеров пройден успешно, то рисуем прямоугольник, в котором будет "будущий" видовой экран
        /// </summary>
        public static void CheckingResultDraw(string nameLayout, Size sizeLayout, Size sizeObjects, Point2d startPointMain, string canon1icalUnscale = "1:4")
        {
            // A4 210 297
            // Letter 216 279
            // Применяем уменьшающий масштаб листа и объектов на листе к модели для рисования 
            Size newSizeLayout = ApplyScaleToSizeObjectsInModel(sizeLayout, canon1icalUnscale);
            Size newSizeObjects = ApplyScaleToSizeObjectsInModel(sizeObjects, canon1icalUnscale);

            Point2d newStartPoint = new Point2d(startPointMain.X - newSizeLayout.Width * 0.5, startPointMain.Y);
            
            // Получаем прямоугольник границ
            Extents2d margins = GetMargins(nameLayout);
            Point2d marginStartPoint = new Point2d(newStartPoint.X + margins.MaxPoint.Y, newStartPoint.Y + margins.MaxPoint.X);
            Size sizeLayoutMargins = new Size(sizeLayout.Width - margins.MaxPoint.Y * 2, sizeLayout.Height - margins.MaxPoint.X * 2);

            // Рисуем макет
            DrawRectangle(newStartPoint, sizeLayout);
            // Рисуем границу
            DrawRectangle(marginStartPoint, sizeLayoutMargins);
            // Рисуем контур объектов
            DrawRectangle(marginStartPoint, newSizeObjects);
        }


        /// <summary>
        /// Меняет масштаб листа на заданный стандартный масштаб
        /// </summary>
        /// <param name="nameLayout"></param>
        /// <param name="scale"></param>
        /// <exception cref="System.Exception"></exception>
        public static void SetSizeLayout(string nameLayout, StdScaleType scale)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            ObjectId layID = layoutManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

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
            if (AcDocument is null) throw new System.Exception("No active document!");

            ObjectId layID = layoutManager.GetLayoutId(nameLayout);

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                var layWrite = acTrans.GetObject(layID, OpenMode.ForWrite) as Layout;

                // Получаем значение девайса печати для макета
                string deviceName = layWrite.PlotConfigurationName;

                if (!layoutManager.LayoutExists(nameLayout))
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
        /// Создать макет с указанным именем и масштабом
        /// </summary>
        public static ObjectId CreateLayout(string nameLayout, string canonicalScale, string deviceName = "Нет")
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            if (layoutManager.LayoutExists(nameLayout))
                throw new System.Exception($"Layout with name {nameLayout} already exists.");

            if (!GetAllCanonicalScales().Contains(canonicalScale))
                throw new System.Exception($"Canonical scale is wrong.");

            ObjectId id = layoutManager.CreateLayout(nameLayout);

            // Меняем масштаб листа на заданный масштаб выбранного девайса
            SetSizeLayout(nameLayout, canonicalScale);

            AcEditor.WriteMessage($"{nameLayout} created with scale {canonicalScale}");

            return id;
        }
    }
}
