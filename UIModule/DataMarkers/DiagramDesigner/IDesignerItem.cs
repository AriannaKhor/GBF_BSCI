namespace UIModule.DataMarkers.DiagramDesigner
{
    public interface IDesignerItem
    {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double Offset { get; set; }
        double MinWidth { get; set; }
        double MinHeight { get; set; }
        bool IsSelected { get; set; }
        bool IsSimple { get; set; }
        bool IsBlinking { get; set; }

    }
}
