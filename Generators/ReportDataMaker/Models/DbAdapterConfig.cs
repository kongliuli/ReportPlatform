using System.Collections.Generic;

namespace ReportDataMaker.Models;

public class DbAdapterConfig
{
    public string AdapterId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}
