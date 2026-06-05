using Microsoft.Extensions.DependencyInjection;
using Xinglin.ReportEditor.PluginHost.Abstractions;
using Xinglin.ReportEditor.PluginHost.Services;

namespace Xinglin.ReportEditor.PluginHost.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPluginHost(this IServiceCollection services)
    {
        services.AddSingleton<IPluginContext, DefaultPluginContext>();
        services.AddSingleton<PluginHostService>();
        return services;
    }
}
