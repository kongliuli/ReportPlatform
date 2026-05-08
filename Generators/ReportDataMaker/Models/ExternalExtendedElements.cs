using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReportDataMaker.Models
{
    public class ExternalImageElement : ExternalElementBase
    {
        public string Src { get; set; }
        [JsonProperty("stretch")]
        public string Fit { get; set; }
        public bool MaintainAspectRatio { get; set; }
        public string AltText { get; set; }
    }

    public class ExternalShapeElement : ExternalElementBase
    {
        public string ShapeType { get; set; }
        public string FillColor { get; set; }
        public string StrokeColor { get; set; }
        public double StrokeWidth { get; set; }
    }

    public class ExternalDividerElement : ExternalElementBase
    {
        public double Thickness { get; set; }
        public string Color { get; set; }
        public string Style { get; set; }
    }

    public class ExternalCheckboxElement : ExternalElementBase
    {
        public bool Checked { get; set; }
        public string CheckColor { get; set; }
    }

    public class ExternalRadioElement : ExternalElementBase
    {
        public string GroupName { get; set; }
        public string Value { get; set; }
        public bool Checked { get; set; }
    }

    public class ExternalSignatureElement : ExternalElementBase
    {
        public string Placeholder { get; set; }
        [JsonProperty("strokeColor")]
        public string LineColor { get; set; }
        [JsonProperty("strokeWidth")]
        public double LineWidth { get; set; }
    }

    public class ExternalBarcodeElement : ExternalElementBase
    {
        public string Value { get; set; }
        [JsonProperty("barcodeFormat")]
        public string Format { get; set; }
        public bool ShowText { get; set; }
        public string LineColor { get; set; }
    }

    public class ExternalQrCodeElement : ExternalElementBase
    {
        public string Value { get; set; }
        [JsonProperty("errorLevel")]
        public string ErrorCorrectionLevel { get; set; }
        public double Margin { get; set; }
        public string Color { get; set; }
    }

    public class ExternalChartElement : ExternalElementBase
    {
        public string ChartType { get; set; }
        public string Title { get; set; }
        public string DataSource { get; set; }
        public bool ShowLegend { get; set; }
        public bool ShowGrid { get; set; }
    }

    public class ExternalContainerElement : ExternalElementBase
    {
        public List<ExternalElementBase> Children { get; set; }
        public string Layout { get; set; }
        public double Padding { get; set; }
        public bool ClipContent { get; set; }
    }

    public class ExternalRepeatElement : ExternalElementBase
    {
        public string DataSource { get; set; }
        public string ItemTemplate { get; set; }
        public string Direction { get; set; }
        public double Gap { get; set; }
    }

    public class ExternalHeaderElement : ExternalElementBase
    {
        public List<ExternalElementBase> Children { get; set; }
        public bool ShowOnFirstPage { get; set; }
        public bool ShowOnAllPages { get; set; }
    }

    public class ExternalFooterElement : ExternalElementBase
    {
        public List<ExternalElementBase> Children { get; set; }
        public bool ShowOnLastPage { get; set; }
        public bool ShowOnAllPages { get; set; }
    }

    public class ExternalPageNumberElement : ExternalElementBase
    {
        public string Format { get; set; }
        [JsonProperty("startFrom")]
        public int StartPage { get; set; }
    }

    public class ExternalWatermarkElement : ExternalElementBase
    {
        public string Text { get; set; }
        public double Angle { get; set; }
        public string Color { get; set; }
        public bool Repeat { get; set; }
    }

    public class ExternalIconElement : ExternalElementBase
    {
        public string IconName { get; set; }
        public string IconSet { get; set; }
        [JsonProperty("iconColor")]
        public string Color { get; set; }
        [JsonProperty("iconSize")]
        public double Size { get; set; }
    }

    public class ExternalHyperlinkElement : ExternalElementBase
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public bool OpenInNewTab { get; set; }
    }
}
