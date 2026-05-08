using System.Collections.Generic;

namespace ReportDataMaker.Models;

public class DataBindingDefinition
{
    public string? Id { get; set; }
    public string ElementId { get; set; } = string.Empty;
    public string DataPath { get; set; } = string.Empty;
    public string BindingType { get; set; } = "Text";
    public string? FormatString { get; set; }
    public string? DefaultValue { get; set; }
}
