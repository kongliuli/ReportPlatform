using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextAdapterConfig : AdapterConfigBase
{
    public string ProfileName { get; set; } = "default";
    public Dictionary<string, string> StaticValues { get; set; } = new();
    public List<DynamicContextRule> DynamicRules { get; set; } = new();
}
