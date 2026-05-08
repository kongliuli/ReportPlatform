namespace ReportDataMaker.Models
{
    public class TextElement : ElementBase
    {
        public string Text { get; set; }
        public string RichText { get; set; }
        public bool IsRichText { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string FontWeight { get; set; }
        public string FontStyle { get; set; }
        public string ForegroundColor { get; set; }
        public string BackgroundColor { get; set; }
        public string TextAlignment { get; set; }
        public string VerticalAlignment { get; set; }
        public string TextDecoration { get; set; }
        public string DataBindingPath { get; set; }
        public string FormatString { get; set; }
    }
}