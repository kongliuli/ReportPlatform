using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace ReportDataMaker.Models;

public class ExternalTemplateDefinition
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = "1";
    public string? HospitalId { get; set; }
    public double PageWidth { get; set; } = 210;
    public double PageHeight { get; set; } = 297;
    public string Orientation { get; set; } = "Portrait";
    public double MarginLeft { get; set; } = 20;
    public double MarginRight { get; set; } = 20;
    public double MarginTop { get; set; } = 20;
    public double MarginBottom { get; set; } = 20;
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public double GlobalFontSize { get; set; }
    public bool EnableGlobalFontSize { get; set; }
    public List<ExternalElementBase> Elements { get; set; } = new();
    public List<LegacyDataBindingDefinition> DataBindings { get; set; } = new();
    public string? FilePath { get; set; }
}

public class LegacyDataBindingDefinition
{
    public string? Id { get; set; }
    public string ElementId { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public string BindingType { get; set; } = "Text";
    public string? FormatString { get; set; }
    public string? DefaultValue { get; set; }
}

public class ExternalElementBase
{
    private string? _id;
    
    public string Id
    {
        get => _id ??= Guid.NewGuid().ToString("N");
        set => _id = value;
    }
    
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsVisible { get; set; } = true;
    public double Rotation { get; set; }
    public int ZIndex { get; set; }
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
    public string Label { get; set; } = string.Empty;
    public double LabelWidth { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = new();
    public string DataPath { get; set; } = string.Empty;
    public string FormatString { get; set; } = string.Empty;
    public bool IsDataBound => !string.IsNullOrEmpty(DataPath);
    public string? ElementType { get; set; }
    public Xinglin.ReportEditor.Contracts.Enums.ElementGroup Group { get; set; }
    public string? AdapterId { get; set; }
}

public class ExternalTextElement : ExternalElementBase
{
    public string Text { get; set; } = string.Empty;
    public string RichText { get; set; } = string.Empty;
    public bool IsRichText { get; set; }
    public string TextDecoration { get; set; } = string.Empty;
    public string StyleRef { get; set; } = string.Empty;
}

public class ExternalLineElement : ExternalElementBase
{
    public string LineColor { get; set; } = "#000000";
    public double LineWidth { get; set; } = 1;
    public string LineStyle { get; set; } = string.Empty;
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }
}

public class ExternalDropdownElement : ExternalElementBase
{
    public string Value { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
}

public class ExternalNumberElement : ExternalElementBase
{
    public double Value { get; set; }
    public string Format { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class ExternalDateElement : ExternalElementBase
{
    public string Value { get; set; } = string.Empty;
    public string Format { get; set; } = "yyyy-MM-dd";
    public string MinDate { get; set; } = string.Empty;
    public string MaxDate { get; set; } = string.Empty;
}

public class ExternalTableElement : ExternalElementBase
{
    public int Rows { get; set; } = 3;
    public int Columns { get; set; } = 4;
    public List<List<string>> CellData { get; set; } = new();
    public double CellPadding { get; set; }
    public double TableBorder { get; set; } = 1;
    public bool HasHeader { get; set; } = true;
}
