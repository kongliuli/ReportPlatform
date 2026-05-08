using System.Collections.Generic;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Models;

public class DbAdapterConfig : AdapterConfigBase
{
    public string Provider { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<DbMappingEntry> Mappings { get; set; } = new();
}

public class DbMappingEntry
{
    public string DataPath { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public string Column { get; set; } = string.Empty;
    public string? Where { get; set; }
}
