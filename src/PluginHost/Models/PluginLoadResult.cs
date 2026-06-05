namespace Xinglin.ReportEditor.PluginHost.Models;

/// <summary>插件加载结果</summary>
public class PluginLoadResult
{
    public bool IsSuccess { get; init; }
    public string? PluginId { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? Exception { get; init; }

    public static PluginLoadResult Success(string pluginId) => new() { IsSuccess = true, PluginId = pluginId };
    public static PluginLoadResult Failure(string pluginId, string error, Exception? ex = null) => new() { IsSuccess = false, PluginId = pluginId, ErrorMessage = error, Exception = ex };
}
