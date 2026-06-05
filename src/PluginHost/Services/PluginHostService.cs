using System.Reflection;
using Xinglin.ReportEditor.PluginHost.Abstractions;
using Xinglin.ReportEditor.PluginHost.Models;

namespace Xinglin.ReportEditor.PluginHost.Services;

/// <summary>插件宿主服务，管理插件的加载、初始化和卸载</summary>
public class PluginHostService : IDisposable
{
    private readonly Dictionary<string, IPlugin> _plugins = new();
    private readonly Dictionary<string, PluginDescriptor> _descriptors = new();
    private readonly IPluginContext _context;
    private bool _disposed;

    public PluginHostService(IPluginContext context)
    {
        _context = context;
    }

    /// <summary>已加载的插件数量</summary>
    public int PluginCount => _plugins.Count;

    /// <summary>获取所有已加载插件的描述符</summary>
    public IReadOnlyList<PluginDescriptor> GetAllDescriptors() => _descriptors.Values.ToList().AsReadOnly();

    /// <summary>获取指定类型的插件</summary>
    public IEnumerable<IPlugin> GetPluginsByType(string pluginType) =>
        _plugins.Values.Where(p => _descriptors.TryGetValue(p.PluginId, out var d) && d.PluginType == pluginType);

    /// <summary>注册插件实例</summary>
    public PluginLoadResult RegisterPlugin(IPlugin plugin)
    {
        if (_plugins.ContainsKey(plugin.PluginId))
            return PluginLoadResult.Failure(plugin.PluginId, $"插件 {plugin.PluginId} 已注册");

        try
        {
            var descriptor = new PluginDescriptor
            {
                PluginId = plugin.PluginId,
                DisplayName = plugin.DisplayName,
                Version = plugin.Version,
                PluginType = plugin.PluginType,
                LoadedAt = DateTime.UtcNow
            };

            _plugins[plugin.PluginId] = plugin;
            _descriptors[plugin.PluginId] = descriptor;

            _context.LogInformation($"插件 {plugin.DisplayName} ({plugin.PluginId}) 已注册");
            return PluginLoadResult.Success(plugin.PluginId);
        }
        catch (Exception ex)
        {
            return PluginLoadResult.Failure(plugin.PluginId, $"注册失败: {ex.Message}", ex);
        }
    }

    /// <summary>初始化指定插件</summary>
    public async Task<PluginLoadResult> InitializePluginAsync(string pluginId)
    {
        if (!_plugins.TryGetValue(pluginId, out var plugin))
            return PluginLoadResult.Failure(pluginId, $"插件 {pluginId} 未找到");

        if (plugin.IsInitialized)
            return PluginLoadResult.Success(pluginId);

        try
        {
            await plugin.InitializeAsync(_context);
            _context.LogInformation($"插件 {plugin.DisplayName} ({plugin.PluginId}) 已初始化");
            return PluginLoadResult.Success(pluginId);
        }
        catch (Exception ex)
        {
            _context.LogError($"插件 {plugin.PluginId} 初始化失败", ex);
            return PluginLoadResult.Failure(pluginId, $"初始化失败: {ex.Message}", ex);
        }
    }

    /// <summary>初始化所有已注册插件</summary>
    public async Task InitializeAllAsync()
    {
        foreach (var pluginId in _plugins.Keys.ToList())
            await InitializePluginAsync(pluginId);
    }

    /// <summary>卸载指定插件</summary>
    public bool UnloadPlugin(string pluginId)
    {
        if (!_plugins.TryGetValue(pluginId, out var plugin))
            return false;

        try
        {
            plugin.Dispose();
            _plugins.Remove(pluginId);
            _descriptors.Remove(pluginId);
            _context.LogInformation($"插件 {pluginId} 已卸载");
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>从程序集发现并注册插件</summary>
    public IEnumerable<PluginLoadResult> DiscoverPlugins(Assembly assembly)
    {
        var pluginType = typeof(IPlugin);
        var results = new List<PluginLoadResult>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface) continue;
            if (!pluginType.IsAssignableFrom(type)) continue;

            try
            {
                var plugin = (IPlugin)Activator.CreateInstance(type)!;
                var result = RegisterPlugin(plugin);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(PluginLoadResult.Failure(type.Name, $"实例化失败: {ex.Message}", ex));
            }
        }

        return results;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var plugin in _plugins.Values)
            plugin.Dispose();
        _plugins.Clear();
        _descriptors.Clear();
    }
}
