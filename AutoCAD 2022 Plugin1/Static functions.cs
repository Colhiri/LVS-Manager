using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using AcCoreAp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace AutoCAD_2022_Plugin1
{
    public static class Working_functions
    {
        private static Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
        private static Database AcDatabase = AcDocument.Database;
        private static Editor AcEditor = AcDocument.Editor;
        private static LayoutManager layoutManager = LayoutManager.Current;
        private static ObjectContextManager OCM = AcDatabase.ObjectContextManager;
        private static PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;
        private static LayerStateManager layerManager = new LayerStateManager(AcDatabase);
        public static FieldList FL = new FieldList();

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
        /// Усекает часть Z и возвращает Point2d
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point2d Point3dTo2d(Point3d point)
        {
            return new Point2d(point.X, point.Y);
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
        /// Возвращает размеры бумаги исходя из заданного стандартного макета в модели
        /// </summary>
        /// <param name="formatPaper"></param>
        /// <param name="devicePlotter"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static Size GetSizePaper(string formatPaper, string devicePlotter = "Нет")
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            string[] devices = pltValidator.GetPlotDeviceList().Cast<string>().ToArray();
            if (!devices.Contains(devicePlotter)) throw new System.Exception("Not found your device in device list in Autocad.");

            string[] scales;

            ObjectId ModelSpaceId;

            double width = 0.0;
            double height = 0.0;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                DBDictionary layoutDict = acTrans.GetObject(AcDatabase.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

                foreach (DBDictionaryEntry entry in layoutDict)
                {
                    Layout layout = acTrans.GetObject(entry.Value, OpenMode.ForWrite) as Layout;

                    if (layout.ModelType)
                    {
                        pltValidator.SetPlotConfigurationName(layout, devicePlotter, null);
                        pltValidator.SetCanonicalMediaName(layout, formatPaper);

                        width = layout.PlotPaperSize.X;
                        height = layout.PlotPaperSize.Y;
                        
                        break;
                    }
                }
            }
            return new Size(width, height);
        }

        /// <summary>
        /// Предоставляет список всех плоттеров в текущей сессии AutoCad
        /// </summary>
        public static string[] GetPlotters()
        {
            if (AcDocument is null) throw new System.Exception("No active document!");
            return pltValidator.GetPlotDeviceList().Cast<string>().ToArray();
        }

        /// <summary>
        /// Проверяет находится ли данный плоттер в листе всех плоттеров
        /// </summary>
        /// <param name="PlotterName"></param>
        /// <returns></returns>
        public static bool CheckPlotter(string PlotterName)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            string[] plotterNames = pltValidator.GetPlotDeviceList().Cast<string>().ToArray();
            return plotterNames.Contains(PlotterName);
        }

        /// <summary>
        /// Проверяет верный ли формат листа и есть ли он в AutoCad
        /// </summary>
        /// <param name="PageFormat"></param>
        /// <returns></returns>
        public static bool CheckPageFormat(string PageFormat, string PlotterName)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            string[] formats;

            using (PlotSettings plt = new PlotSettings(true))
            {
                formats = pltValidator.GetCanonicalMediaNameList(plt).Cast<string>().ToArray();
            }

            return formats.Contains(PageFormat);
        }


        /// <summary>
        /// Получаем все канонические масштабы в открытом чертеже
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllCanonicalScales(string deviceName)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            if (!CheckPlotter(deviceName)) throw new System.Exception("Not found your device in device list in Autocad.");

            string[] scales;

            using (PlotSettings pltSet = new PlotSettings(true))
            {
                pltValidator.SetPlotConfigurationName(pltSet, deviceName, null);
                scales = pltValidator.GetCanonicalMediaNameList(pltSet).Cast<string>().ToArray();
            }

            return scales;
        }

        /// <summary>
        /// Возвращает все аннотационные масштабы принятые в AutoCad
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllAnnotationScales()
        {
            ObjectContextCollection occ = OCM.GetContextCollection("ACDB_ANNOTATIONSCALES");
            string[] annoScales = occ.Cast<AnnotationScale>().Select(x => x.Name).ToArray();
            return annoScales;
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

        /// <summary>
        /// Добавить новый аннотационный масштаб
        /// </summary>
        /// <param name="annoScale"></param>
        /// <exception cref="System.Exception"></exception>
        public static void AddNewAnnotationScale(string annoScale)
        {
            int[] parts = annoScale.Split(':').Select(x => int.Parse(x)).ToArray();

            ObjectContextCollection occ = OCM.GetContextCollection("ACDB_ANNOTATIONSCALES");
            if (occ == null) throw new System.Exception("Object Context Collection is null. Some errors with standart annotation scales.");

            AnnotationScale newAnnoScale = new AnnotationScale()
            {
                Name = annoScale,
                PaperUnits = parts[0],
                DrawingUnits = parts[1],
            };

            occ.AddContext(newAnnoScale);
        }


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

                // Add new DBObject in Database
                // Set ObjectId Creating ViewPort
                ObjectId viewportID = acBlkTblRec.AppendEntity(viewport);
                acTrans.AddNewlyCreatedDBObject(viewport, true);

                Viewport vp = acTrans.GetObject(viewportID, OpenMode.ForWrite) as Viewport;

                // Activate this parameters vork only drop DBObject in acDB (PS vp.ON only)
                List<AnnotationScale> annotation = occ.Cast<AnnotationScale>().ToList();

                AnnotationScale rightScale = annotation[0];

                try
                {
                    rightScale = annotation.Where(x => x.Name == annotationScaleName).First();
                }
                catch
                {
                    AddNewAnnotationScale(annotationScaleName);
                    annotation = OCM.GetContextCollection("ACDB_ANNOTATIONSCALES").Cast<AnnotationScale>().ToList();
                    rightScale = annotation.Where(x => x.Name == annotationScaleName).First();
                }

                vp.On = true;
                vp.AnnotationScale = rightScale;
                vp.CustomScale = rightScale.Scale;

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
        /// <param name="NameLayer">Имя слоя, которое равно имени макета</param>
        /// <returns></returns>
        public static ObjectId DrawRectangle(Point2d startPoint, Size size, int IndexColor)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            ObjectId polylineId;

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

                    polylineId = records.AppendEntity(acPoly);
                    acPoly.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.ByAci, (short)IndexColor);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                }
                acTrans.Commit();

                BlockTableRecord record = acTrans.GetObject(polylineId, OpenMode.ForRead) as BlockTableRecord;
            }
            return polylineId;
        }

        /// <summary>
        /// Создать слой
        /// </summary>
        /// <param name="NameLayer"></param>
        public static void CreateLayer(string NameLayer)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                LayerTable layColl = acTrans.GetObject(AcDatabase.LayerTableId, OpenMode.ForRead) as LayerTable;

                if (layColl.Has(NameLayer)) return;

                using (LayerTableRecord layTableRec = new LayerTableRecord())
                {
                    layTableRec.Name = NameLayer;

                    layColl.UpgradeOpen();

                    layColl.Add(layTableRec);
                    acTrans.AddNewlyCreatedDBObject(layTableRec, true);
                }
                acTrans.Commit();
            }
        }

        /// <summary>
        /// Установить имя слоя и создать, если такого слоя не существует
        /// </summary>
        /// <param name="objID"></param>
        /// <param name="NameLayer"></param>
        /// <exception cref="System.Exception"></exception>
        public static void SetLayer(ObjectId objID, string NameLayer)
        {
            if (AcDocument is null) throw new System.Exception("No active document!");

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                if (!layerManager.HasLayerState(NameLayer)) CreateLayer(NameLayer);

                Entity objectRecord = acTrans.GetObject(objID, OpenMode.ForWrite) as Entity;

                objectRecord.Layer = NameLayer;

                acTrans.Commit();
            }
        }


        /// <summary>
        /// Изменить масштаб листа на заданный стандартный масштаб
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

            if (!GetAllCanonicalScales(deviceName).Contains(canonicalScale))
                throw new System.Exception($"Canonical scale is wrong.");

            ObjectId id = layoutManager.CreateLayout(nameLayout);

            // Меняем масштаб листа на заданный масштаб выбранного девайса
            SetSizeLayout(nameLayout, canonicalScale);

            AcEditor.WriteMessage($"{nameLayout} created with scale {canonicalScale}");

            return id;
        }

        /// <summary>
        /// Наводится объекты в пространстве модели
        /// </summary>
        /// <param name="objectsIDs"></param>
        public static void ZoomToObjects(ObjectIdCollection objectsIDs)
        {
            using (ViewTableRecord view = AcEditor.GetCurrentView())
            {
                Size sizeObjects = CheckModelSize(objectsIDs);

                view.Width = sizeObjects.Width;
                view.Height = sizeObjects.Height;
                view.CenterPoint = Point3dTo2d(CheckCenterModel(objectsIDs));

                AcEditor.SetCurrentView(view);
            }

        }
    }
}
