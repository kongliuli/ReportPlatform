using Microsoft.Extensions.DependencyInjection;

namespace ReportDataMaker.Infrastructure;

/// <summary>服务定位器，提供全局服务访问入口</summary>
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>获取已注册的服务提供者实例</summary>
    public static IServiceProvider Services => _serviceProvider 
        ?? throw new InvalidOperationException("ServiceLocator not initialized");

    /// <summary>使用服务集合初始化服务定位器</summary>
    /// <param name="services">服务集合</param>
    public static void Initialize(IServiceCollection services)
    {
        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>获取指定类型的服务实例</summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例</returns>
    public static T GetService<T>() where T : class
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>获取指定类型的服务实例，未注册时返回null</summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例或null</returns>
    public static T? GetServiceOrDefault<T>() where T : class
    {
        return Services.GetService<T>();
    }
}
