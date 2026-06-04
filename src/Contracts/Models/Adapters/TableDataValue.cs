namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

public class TableDataValue
{
    public string? TableElementId { get; set; }
    public List<List<string>> Rows { get; set; } = new();
}
