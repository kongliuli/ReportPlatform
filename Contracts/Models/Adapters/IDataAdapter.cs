using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

public interface IDataAdapter
{
    string AdapterId { get; }
    
    string AdapterName { get; }
    
    AdapterType Type { get; }
    
    IReadOnlyList<string> TargetDataPaths { get; }
    
    Task<AdapterResult> ReadDataAsync();
    
    Task<AdapterResult> ReadBatchDataAsync();
    
    Task<ValidationResult> ValidateConfigAsync();
}
