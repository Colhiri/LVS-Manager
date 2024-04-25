namespace AutoCAD_2022_Plugin1
{
    /// <summary>
    /// Состояние отрисовки видового экрана или поля в пространстве модели
    /// </summary>
    public enum State
    {
        Exist,
        NoExist
    }

    /// <summary>
    /// Где находится стартовая точка отрисовки полей и видовых экранов
    /// </summary>
    public enum LocationDraw
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Custom
    }

    public enum WorkObject
    {
        Layout,
        Viewport,
        None
    }
}
