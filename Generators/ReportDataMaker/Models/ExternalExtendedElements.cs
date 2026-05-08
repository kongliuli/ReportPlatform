using Newtonsoft.Json;

namespace ReportDataMaker.Models;

public class ExternalImageElement : ExternalElementBase
{
    public string Src { get; set; } = string.Empty;
    
    [JsonProperty("stretch")]
    public string Fit { get; set; } = string.Empty;
    
    public bool MaintainAspectRatio { get; set; }
    
    public string AltText { get; set; } = string.Empty;
}

public class ExternalShapeElement : ExternalElementBase
{
    public string ShapeType { get; set; } = string.Empty;
    public string FillColor { get; set; } = string.Empty;
    public string StrokeColor { get; set; } = string.Empty;
    public double StrokeWidth { get; set; }
}

public class ExternalDividerElement : ExternalElementBase
{
    public double Thickness { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
}

public class ExternalCheckboxElement : ExternalElementBase
{
    public bool Checked { get; set; }
    public string CheckColor { get; set; } = string.Empty;
}

public class ExternalRadioElement : ExternalElementBase
{
    public string GroupName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool Checked { get; set; }
}

public class ExternalSignatureElement : ExternalElementBase
{
    public string Placeholder { get; set; } = string.Empty;
    
    [JsonProperty("strokeColor")]
    public string LineColor { get; set; } = string.Empty;
    
    [JsonProperty("strokeWidth")]
    public double LineWidth { get; set; }
}

public class ExternalBarcodeElement : ExternalElementBase
{
    public string Value { get; set; } = string.Empty;
    
    [JsonProperty("barcodeFormat")]
    public string Format { get; set; } = string.Empty;
    
    public bool ShowText { get; set; }
    public string LineColor { get; set; } = string.Empty;
}

public class ExternalQrCodeElement : ExternalElementBase
{
    public string Value { get; set; } = string.Empty;
    
    [JsonProperty("errorLevel")]
    public string ErrorCorrectionLevel { get; set; } = string.Empty;
    
    public double Margin { get; set; }
    public string Color { get; set; } = string.Empty;
}

public class ExternalChartElement : ExternalElementBase
{
    public string ChartType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public bool ShowLegend { get; set; }
    public bool ShowGrid { get; set; }
}

public class ExternalContainerElement : ExternalElementBase
{
    public List<ExternalElementBase> Children { get; set; } = new();
    public string Layout { get; set; } = string.Empty;
    public double Padding { get; set; }
    public bool ClipContent { get; set; }
}

public class ExternalRepeatElement : ExternalElementBase
{
    public string DataSource { get; set; } = string.Empty;
    public string ItemTemplate { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public double Gap { get; set; }
}

public class ExternalHeaderElement : ExternalElementBase
{
    public List<ExternalElementBase> Children { get; set; } = new();
    public bool ShowOnFirstPage { get; set; }
    public bool ShowOnAllPages { get; set; }
}

public class ExternalFooterElement : ExternalElementBase
{
    public List<ExternalElementBase> Children { get; set; } = new();
    public bool ShowOnLastPage { get; set; }
    public bool ShowOnAllPages { get; set; }
}

public class ExternalPageNumberElement : ExternalElementBase
{
    public string Format { get; set; } = string.Empty;
    
    [JsonProperty("startFrom")]
    public int StartPage { get; set; }
}

public class ExternalWatermarkElement : ExternalElementBase
{
    public string Text { get; set; } = string.Empty;
    public double Angle { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool Repeat { get; set; }
}

public class ExternalIconElement : ExternalElementBase
{
    public string IconName { get; set; } = string.Empty;
    public string IconSet { get; set; } = string.Empty;
    
    [JsonProperty("iconColor")]
    public string Color { get; set; } = string.Empty;
    
    [JsonProperty("iconSize")]
    public double Size { get; set; }
}

public class ExternalHyperlinkElement : ExternalElementBase
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool OpenInNewTab { get; set; }
}
