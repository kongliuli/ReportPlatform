using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Web.Services;

/// <summary>适配器注册表，管理所有已注册的适配器插件</summary>
public class AdapterRegistry
{
    private readonly Dictionary<string, IAdapterPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>注册适配器插件</summary>
    public void Register(IAdapterPlugin plugin)
    {
        _plugins[plugin.AdapterType] = plugin;
    }

    /// <summary>根据适配器类型获取插件</summary>
    public IAdapterPlugin? GetByType(string adapterType)
    {
        return _plugins.GetValueOrDefault(adapterType);
    }

    /// <summary>获取所有已注册的适配器插件</summary>
    public IReadOnlyList<IAdapterPlugin> GetAllPlugins() => _plugins.Values.ToList().AsReadOnly();
}
