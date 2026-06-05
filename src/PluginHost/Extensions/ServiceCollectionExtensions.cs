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

    public static IServiceCollection AddPluginHost(this IServiceCollection services, string pluginDirectory)
    {
        services.AddSingleton<IPluginContext, DefaultPluginContext>();
        services.AddSingleton<PluginHostService>(sp =>
        {
            var context = sp.GetRequiredService<IPluginContext>();
            var hostService = new PluginHostService(context);
            hostService.EnableHotLoading(pluginDirectory);
            return hostService;
        });
        return services;
    }
}
