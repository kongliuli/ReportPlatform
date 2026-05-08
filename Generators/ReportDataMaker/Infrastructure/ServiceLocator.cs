using Microsoft.Extensions.DependencyInjection;

namespace ReportDataMaker.Infrastructure;

public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    public static IServiceProvider Services => _serviceProvider 
        ?? throw new InvalidOperationException("ServiceLocator not initialized");

    public static void Initialize(IServiceCollection services)
    {
        _serviceProvider = services.BuildServiceProvider();
    }

    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }

    public static T? GetServiceOrDefault<T>() where T : class
    {
        return Services.GetService<T>();
    }
}
