using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public static ObjectId CreateViewport(double width, double height, string layoutName, Point3d centerPoint, Vector3d orientation)
        {
            //ObjectId layoutID

            ObjectId viewportID;

            Document AcDocument = AcCoreAp.DocumentManager.MdiActiveDocument;
            if (AcDocument is null) throw new System.Exception("No active document!");
            Database AcDatabase = AcDocument.Database;
            Editor AcEditor = AcDocument.Editor;
            LayoutManager layoutManager = LayoutManager.Current;

            layoutManager.CurrentLayout = layoutName;

            using (Transaction acTrans = AcDatabase.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = acTrans.GetObject(AcDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.PaperSpace], OpenMode.ForWrite) as BlockTableRecord;

                Viewport viewport = new Viewport
                {
                    Width = width,
                    Height = height,
                    CenterPoint = centerPoint
                };

                // Add new DBObject in Database
                // Set ObjectId Creating ViewPort
                viewportID = acBlkTblRec.AppendEntity(viewport);
                acTrans.AddNewlyCreatedDBObject(viewport, true);

                // Activate this parameters vork only drop DBObject in acDB (PS vp.ON only)
                viewport.ViewDirection = orientation;
                viewport.On = true;

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
            (double, double) size = CheckSizeLayout(nameLayout);

            double height, width;

            size.ToTuple().Deconstruct(out height, out width);

            Point2d sizeLayout = new Point2d(height, width);

            if (sizeLayout.X - sizeLayout.X * (borderLayout / 100) < viewportSize.X || 
                sizeLayout.Y - sizeLayout.Y * (borderLayout / 100) < viewportSize.Y)
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

                PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;

                List<string> formatsLayout;

                try
                {
                    formatsLayout = pltValidator.GetCanonicalMediaNameList(layWrite).Cast<string>().ToList();
                }
                catch
                {
                    throw new System.Exception("Refresh layout. Check it.");
                }

                if (formatsLayout == null) throw new ArgumentNullException();

                layHeight = layWrite.PlotPaperSize.X;

                layWidth = layWrite.PlotPaperSize.Y;

                acTrans.Commit();
            }

            return (layHeight, layWidth);
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

                PlotSettingsValidator pltValidator = PlotSettingsValidator.Current;
                pltValidator.SetCanonicalMediaName(layWrite, canonicalScale);

                acTrans.Commit();
            }
        }

    }
}
