using Newtonsoft.Json;
using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.Models;

public abstract class ReportExternalElementBase : ExternalElementBase
{
    public bool IsVisible { get; set; } = true;
    public string BackgroundColor { get; set; } = string.Empty;
    public string BorderColor { get; set; } = string.Empty;
    public double BorderWidth { get; set; }
    public string BorderStyle { get; set; } = string.Empty;
    public double CornerRadius { get; set; }
    public double Opacity { get; set; } = 1;
    public string Shadow { get; set; } = string.Empty;
    public string FontFamily { get; set; } = string.Empty;
    public double FontSize { get; set; }
    public string FontWeight { get; set; } = string.Empty;
    public string FontStyle { get; set; } = string.Empty;
    public string ForegroundColor { get; set; } = "#000000";
    public string TextAlignment { get; set; } = string.Empty;
    public double LabelWidth { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string FormatString { get; set; } = string.Empty;
    public string? ElementType { get; set; }
}

public class ExternalTextElement : ReportExternalElementBase
{
    public string Text { get; set; } = string.Empty;
    public string RichText { get; set; } = string.Empty;
    public bool IsRichText { get; set; }
    public string TextDecoration { get; set; } = string.Empty;
    public string StyleRef { get; set; } = string.Empty;
}

public class ExternalLineElement : ReportExternalElementBase
{
    public string LineColor { get; set; } = "#000000";
    public double LineWidth { get; set; } = 1;
    public string LineStyle { get; set; } = string.Empty;
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }
}

public class ExternalDropdownElement : ReportExternalElementBase
{
    public string Value { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
}

public class ExternalNumberElement : ReportExternalElementBase
{
    public double Value { get; set; }
    public string Format { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class ExternalDateElement : ReportExternalElementBase
{
    public string Value { get; set; } = string.Empty;
    public string Format { get; set; } = "yyyy-MM-dd";
    public string MinDate { get; set; } = string.Empty;
    public string MaxDate { get; set; } = string.Empty;
}

public class ExternalTableElement : ReportExternalElementBase
{
    public int Rows { get; set; } = 3;
    public int Columns { get; set; } = 4;
    public List<List<string>> CellData { get; set; } = new();
    public double CellPadding { get; set; }
    public double TableBorder { get; set; } = 1;
    public bool HasHeader { get; set; } = true;
}

public class ExternalImageElement : ReportExternalElementBase
{
    public string Src { get; set; } = string.Empty;

    [JsonProperty("stretch")]
    public string Fit { get; set; } = string.Empty;

    public bool MaintainAspectRatio { get; set; }

    public string AltText { get; set; } = string.Empty;
}

public class ExternalShapeElement : ReportExternalElementBase
{
    public string ShapeType { get; set; } = string.Empty;
    public string FillColor { get; set; } = string.Empty;
    public string StrokeColor { get; set; } = string.Empty;
    public double StrokeWidth { get; set; }
}

public class ExternalDividerElement : ReportExternalElementBase
{
    public double Thickness { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
}

public class ExternalCheckboxElement : ReportExternalElementBase
{
    public bool Checked { get; set; }
    public string CheckColor { get; set; } = string.Empty;
}

public class ExternalRadioElement : ReportExternalElementBase
{
    public string GroupName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Checked { get; set; }
}

public class ExternalSignatureElement : ReportExternalElementBase
{
    public string Placeholder { get; set; } = string.Empty;

    [JsonProperty("strokeColor")]
    public string LineColor { get; set; } = string.Empty;

    [JsonProperty("strokeWidth")]
    public double LineWidth { get; set; }
}

public class ExternalBarcodeElement : ReportExternalElementBase
{
    public string Value { get; set; } = string.Empty;

    [JsonProperty("barcodeFormat")]
    public string Format { get; set; } = string.Empty;

    public bool ShowText { get; set; }
    public string LineColor { get; set; } = string.Empty;
}

public class ExternalQrCodeElement : ReportExternalElementBase
{
    public string Value { get; set; } = string.Empty;

    [JsonProperty("errorLevel")]
    public string ErrorCorrectionLevel { get; set; } = string.Empty;

    public double Margin { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class ExternalChartElement : ReportExternalElementBase
{
    public string ChartType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public bool ShowLegend { get; set; }
    public bool ShowGrid { get; set; }
}

public class ExternalContainerElement : ReportExternalElementBase
{
    public List<ReportExternalElementBase> Children { get; set; } = new();
    public string Layout { get; set; } = string.Empty;
    public double Padding { get; set; }
    public bool ClipContent { get; set; }
}

public class ExternalRepeatElement : ReportExternalElementBase
{
    public string DataSource { get; set; } = string.Empty;
    public string ItemTemplate { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public double Gap { get; set; }
}

public class ExternalHeaderElement : ReportExternalElementBase
{
    public List<ReportExternalElementBase> Children { get; set; } = new();
    public bool ShowOnFirstPage { get; set; }
    public bool ShowOnAllPages { get; set; }
}

public class ExternalFooterElement : ReportExternalElementBase
{
    public List<ReportExternalElementBase> Children { get; set; } = new();
    public bool ShowOnLastPage { get; set; }
    public bool ShowOnAllPages { get; set; }
}

public class ExternalPageNumberElement : ReportExternalElementBase
{
    public string Format { get; set; } = string.Empty;

    [JsonProperty("startFrom")]
    public int StartPage { get; set; }
}

public class ExternalWatermarkElement : ReportExternalElementBase
{
    public string Text { get; set; } = string.Empty;
    public double Angle { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool Repeat { get; set; }
}

public class ExternalIconElement : ReportExternalElementBase
{
    public string IconName { get; set; } = string.Empty;
    public string IconSet { get; set; } = string.Empty;

    [JsonProperty("iconColor")]
    public string Color { get; set; } = string.Empty;

    [JsonProperty("iconSize")]
    public double Size { get; set; }
}

public class ExternalHyperlinkElement : ReportExternalElementBase
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool OpenInNewTab { get; set; }
}
