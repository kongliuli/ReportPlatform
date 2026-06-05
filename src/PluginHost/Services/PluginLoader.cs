using System.Reflection;
using System.Runtime.Loader;
using Xinglin.ReportEditor.PluginHost.Abstractions;
using Xinglin.ReportEditor.PluginHost.Models;

namespace Xinglin.ReportEditor.PluginHost.Services;

/// <summary>插件 DLL 加载器，支持从文件系统加载和卸载插件程序集</summary>
public class PluginLoader : IDisposable
{
    private readonly List<PluginLoadContext> _loadContexts = new();
    private readonly string _pluginDirectory;
    private readonly IPluginContext _context;
    private readonly FileSystemWatcher? _watcher;
    private bool _disposed;

    public PluginLoader(string pluginDirectory, IPluginContext context)
    {
        _pluginDirectory = pluginDirectory;
        _context = context;

        if (!Directory.Exists(_pluginDirectory))
            Directory.CreateDirectory(_pluginDirectory);

        // 监视插件目录变化
        _watcher = new FileSystemWatcher(_pluginDirectory, "*.dll")
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true
        };
        _watcher.Created += OnPluginFileChanged;
        _watcher.Changed += OnPluginFileChanged;
        _watcher.Deleted += OnPluginFileChanged;
        _watcher.Renamed += OnPluginFileRenamed;
    }

    /// <summary>插件目录变更事件</summary>
    public event EventHandler<PluginDirectoryChangedEventArgs>? PluginDirectoryChanged;

    /// <summary>从插件目录加载所有插件</summary>
    public IEnumerable<PluginLoadResult> LoadAllPlugins(PluginHostService hostService)
    {
        var results = new List<PluginLoadResult>();

        foreach (var dllPath in Directory.GetFiles(_pluginDirectory, "*.dll", SearchOption.AllDirectories))
        {
            var result = LoadPlugin(dllPath, hostService);
            results.Add(result);
        }

        return results;
    }

    /// <summary>从指定 DLL 路径加载插件</summary>
    public PluginLoadResult LoadPlugin(string dllPath, PluginHostService hostService)
    {
        if (!File.Exists(dllPath))
            return PluginLoadResult.Failure(Path.GetFileName(dllPath), $"文件不存在: {dllPath}");

        try
        {
            var loadContext = new PluginLoadContext(dllPath);
            _loadContexts.Add(loadContext);

            var assembly = loadContext.LoadFromAssemblyPath(dllPath);
            var results = hostService.DiscoverPlugins(assembly);

            var failedResult = results.FirstOrDefault(r => !r.IsSuccess);
            if (failedResult != null)
                return failedResult;

            var pluginId = results.FirstOrDefault(r => r.IsSuccess)?.PluginId ?? Path.GetFileName(dllPath);
            loadContext.PluginId = pluginId;
            _context.LogInformation($"从 {Path.GetFileName(dllPath)} 加载插件: {pluginId}");
            return PluginLoadResult.Success(pluginId);
        }
        catch (Exception ex)
        {
            _context.LogError($"加载插件 DLL 失败: {dllPath}", ex);
            return PluginLoadResult.Failure(Path.GetFileName(dllPath), $"加载失败: {ex.Message}", ex);
        }
    }

    /// <summary>卸载指定插件及其程序集</summary>
    public bool UnloadPlugin(string pluginId, PluginHostService hostService)
    {
        // 找到包含该插件的加载上下文
        var context = _loadContexts.FirstOrDefault(c => c.PluginId == pluginId);
        if (context == null) return false;

        hostService.UnloadPlugin(pluginId);
        context.Unload();
        _loadContexts.Remove(context);
        _context.LogInformation($"插件 {pluginId} 及其程序集已卸载");
        return true;
    }

    private void OnPluginFileChanged(object sender, FileSystemEventArgs e)
    {
        _context.LogInformation($"插件目录变更: {e.ChangeType} {e.Name}");
        PluginDirectoryChanged?.Invoke(this, new PluginDirectoryChangedEventArgs(e.ChangeType, e.FullPath));
    }

    private void OnPluginFileRenamed(object sender, RenamedEventArgs e)
    {
        _context.LogInformation($"插件文件重命名: {e.OldName} -> {e.Name}");
        PluginDirectoryChanged?.Invoke(this, new PluginDirectoryChangedEventArgs(WatcherChangeTypes.Renamed, e.FullPath));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _watcher?.Dispose();
        foreach (var context in _loadContexts)
            context.Unload();
        _loadContexts.Clear();
    }
}

/// <summary>插件加载上下文，支持程序集卸载</summary>
internal class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    public string PluginId { get; set; } = string.Empty;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // 先尝试从依赖解析器加载
        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
            return LoadFromAssemblyPath(assemblyPath);

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
            return LoadUnmanagedDllFromPath(libraryPath);

        return IntPtr.Zero;
    }
}

/// <summary>插件目录变更事件参数</summary>
public class PluginDirectoryChangedEventArgs : EventArgs
{
    public WatcherChangeTypes ChangeType { get; }
    public string Path { get; }

    public PluginDirectoryChangedEventArgs(WatcherChangeTypes changeType, string path)
    {
        ChangeType = changeType;
        Path = path;
    }
}
