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
}
