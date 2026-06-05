namespace Xinglin.ReportEditor.PluginHost.Models;

/// <summary>插件描述符，包含插件的元数据信息</summary>
public class PluginDescriptor
{
    public string PluginId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public Version Version { get; init; } = new(1, 0);
    public string PluginType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? AssemblyPath { get; init; }
    public bool IsEnabled { get; set; } = true;
    public DateTime? LoadedAt { get; set; }
}
