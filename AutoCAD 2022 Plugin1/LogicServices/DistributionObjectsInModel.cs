using Autodesk.AutoCAD.Geometry;

namespace AutoCAD_2022_Plugin1
{
    public class DistributionObjectsInModel
    {
        public Point2d StartPoint { get; set; }
        private Point2d PointDrawing { get; set; }
        public Point2d ToPoint2d() => StartPoint;

        public DistributionObjectsInModel(Point2d StartPoint)
        {
            this.StartPoint = StartPoint;
        }
    }
}
