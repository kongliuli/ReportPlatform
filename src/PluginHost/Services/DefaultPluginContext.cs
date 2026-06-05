using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xinglin.ReportEditor.PluginHost.Abstractions;

namespace Xinglin.ReportEditor.PluginHost.Services;

/// <summary>默认插件上下文实现</summary>
public class DefaultPluginContext : IPluginContext
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DefaultPluginContext(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DefaultPluginContext> logger)
    {
        ServiceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public IServiceProvider ServiceProvider { get; }

    public string? GetConfiguration(string key) => _configuration[key];

    public void LogInformation(string message) => _logger.LogInformation("{Message}", message);
    public void LogWarning(string message) => _logger.LogWarning("{Message}", message);
    public void LogError(string message, Exception? exception = null)
    {
        if (exception != null)
            _logger.LogError(exception, "{Message}", message);
        else
            _logger.LogError("{Message}", message);
    }
}
