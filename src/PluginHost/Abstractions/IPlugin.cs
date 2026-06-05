namespace Xinglin.ReportEditor.PluginHost.Abstractions;

/// <summary>插件接口，所有插件必须实现</summary>
public interface IPlugin : IDisposable
{
    /// <summary>插件唯一标识</summary>
    string PluginId { get; }

    /// <summary>插件显示名称</summary>
    string DisplayName { get; }

    /// <summary>插件版本</summary>
    Version Version { get; }

    /// <summary>插件类型（adapter/export/render）</summary>
    string PluginType { get; }

    /// <summary>初始化插件</summary>
    Task InitializeAsync(IPluginContext context);

    /// <summary>插件是否已初始化</summary>
    bool IsInitialized { get; }
}
