namespace ReportDataMaker.Models
{
    public class ElementBase
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        public string BorderColor { get; set; }
        public double BorderWidth { get; set; }
        public string BorderStyle { get; set; }
        public double CornerRadius { get; set; }
        public double Opacity { get; set; }
        public string ShadowColor { get; set; }
        public double ShadowDepth { get; set; }
        public string HorizontalAlignment { get; set; }
        public string VerticalAlignment { get; set; }
    }
}