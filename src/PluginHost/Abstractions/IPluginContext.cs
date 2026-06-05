namespace Xinglin.ReportEditor.PluginHost.Abstractions;

/// <summary>插件上下文，提供插件运行时环境</summary>
public interface IPluginContext
{
    /// <summary>获取配置值</summary>
    string? GetConfiguration(string key);

    /// <summary>获取服务提供者</summary>
    IServiceProvider ServiceProvider { get; }

    /// <summary>记录信息日志</summary>
    void LogInformation(string message);

    /// <summary>记录警告日志</summary>
    void LogWarning(string message);

    /// <summary>记录错误日志</summary>
    void LogError(string message, Exception? exception = null);
}
