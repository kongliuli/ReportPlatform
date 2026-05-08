using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Adapters;

/// <summary>适配器配置基类</summary>
public abstract class AdapterConfigBase
{
    private string? _adapterId;

    /// <summary>适配器唯一标识</summary>
    public string AdapterId
    {
        get => _adapterId ??= Guid.NewGuid().ToString("N");
        set => _adapterId = value;
    }

    /// <summary>适配器类型</summary>
    public AdapterType Type { get; set; }

    /// <summary>适配器显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;
}
