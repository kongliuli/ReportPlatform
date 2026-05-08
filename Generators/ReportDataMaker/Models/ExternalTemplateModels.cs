using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReportDataMaker.Models
{
    public class ExternalTemplateDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public string HospitalId { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public string Orientation { get; set; }
        public double MarginLeft { get; set; }
        public double MarginRight { get; set; }
        public double MarginTop { get; set; }
        public double MarginBottom { get; set; }
        public string BackgroundColor { get; set; }
        public double GlobalFontSize { get; set; }
        public bool EnableGlobalFontSize { get; set; }
        public List<ExternalElementBase> Elements { get; set; }
        public List<DataBindingDefinition> DataBindings { get; set; }
    }

    public class ExternalElementBase
    {
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
        public double Rotation { get; set; }
        public int ZIndex { get; set; }
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public double BorderWidth { get; set; }
        public string BorderStyle { get; set; }
        public double CornerRadius { get; set; }
        public double Opacity { get; set; }
        public string Shadow { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string FontWeight { get; set; }
        public string FontStyle { get; set; }
        public string ForegroundColor { get; set; }
        public string TextAlignment { get; set; }
        public string Label { get; set; }
        public double LabelWidth { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public List<string> Options { get; set; }
        public string DataPath { get; set; }
        public string FormatString { get; set; }
        public bool IsDataBound { get; set; }

        [JsonProperty("$type")]
        public string ElementType { get; set; }

        public ElementGroup Group { get; set; }
    }

    public class ExternalTextElement : ExternalElementBase
    {
        public string Text { get; set; }
        public string RichText { get; set; }
        public bool IsRichText { get; set; }
        public string TextDecoration { get; set; }
        public string StyleRef { get; set; }
    }

    public class ExternalLineElement : ExternalElementBase
    {
        public string LineColor { get; set; }
        public double LineWidth { get; set; }
        public string LineStyle { get; set; }
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
    }

    public class ExternalDropdownElement : ExternalElementBase
    {
        public string Value { get; set; }
        public string Placeholder { get; set; }
    }

    public class ExternalNumberElement : ExternalElementBase
    {
        public double Value { get; set; }
        public string Format { get; set; }
        public int DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }
    }

    public class ExternalDateElement : ExternalElementBase
    {
        public string Value { get; set; }
        public string Format { get; set; }
        public string MinDate { get; set; }
        public string MaxDate { get; set; }
    }

    public class ExternalTableElement : ExternalElementBase
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<List<string>> CellData { get; set; }
        public double CellPadding { get; set; }
        public double TableBorder { get; set; }
        public bool HasHeader { get; set; }
    }
}
