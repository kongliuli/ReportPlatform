using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>
/// 适配器配置基类
/// </summary>
public abstract class AdapterConfigBase
{
    private string? _adapterId;
    
    public string AdapterId
    {
        get => _adapterId ??= Guid.NewGuid().ToString("N");
        set => _adapterId = value;
    }
    
    public AdapterType Type { get; set; }
    
    public string DisplayName { get; set; } = string.Empty;
    
    public Dictionary<string, string> FieldMappings { get; set; } = new();
    
    public bool IsEnabled { get; set; } = true;
}
