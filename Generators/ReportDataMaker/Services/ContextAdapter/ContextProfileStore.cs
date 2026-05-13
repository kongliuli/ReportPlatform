using System.IO;
using System.Text.Json;
using ReportDataMaker.Infrastructure;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextProfileStore
{
    private readonly string _configPath;

    public ContextProfileStore()
    {
        FileLogger.Instance.WriteLine("[LOG] ===== ContextProfileStore 构造 =====");
        string appDataPath;
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore: AppData={appData}");
            appDataPath = Path.Combine(appData, "ReportDataMaker");
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore: 创建目录 {appDataPath}");
            Directory.CreateDirectory(appDataPath);
            FileLogger.Instance.WriteLine("[LOG] ProfileStore: 目录创建成功");
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore 创建目录失败: {ex.Message}，回退到临时目录");
            appDataPath = Path.Combine(Path.GetTempPath(), "ReportDataMaker");
            Directory.CreateDirectory(appDataPath);
        }
        _configPath = Path.Combine(appDataPath, "context-profiles.json");
        FileLogger.Instance.WriteLine($"[LOG] ProfileStore: 配置文件路径={_configPath}, 文件存在={File.Exists(_configPath)}");
        FileLogger.Instance.WriteLine("[LOG] ===== ContextProfileStore 构造完成 =====");
    }

    public ContextAdapterConfig Load(string profileName)
    {
        FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: profileName='{profileName}' configPath='{_configPath}'");
        if (!File.Exists(_configPath))
        {
            FileLogger.Instance.WriteLine("[LOG] ProfileStore.Load: 文件不存在，返回默认配置");
            return new ContextAdapterConfig();
        }
        try
        {
            var json = File.ReadAllText(_configPath);
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: 读取到 {json.Length} 字节");
            var profiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json);
            if (profiles != null && profiles.TryGetValue(profileName, out var config))
            {
                FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: 找到配置文件 '{profileName}'");
                return config;
            }
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: 未找到配置文件 '{profileName}'，返回默认");
            return new ContextAdapterConfig();
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load 异常: {ex.GetType().Name} - {ex.Message}");
            return new ContextAdapterConfig();
        }
    }

    public void Save(ContextAdapterConfig config)
    {
        FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Save: profileName='{config.ProfileName}'");
        Dictionary<string, ContextAdapterConfig> allProfiles;
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                allProfiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json)
                    ?? new Dictionary<string, ContextAdapterConfig>();
                FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Save: 读取到 {allProfiles.Count} 个已有配置");
            }
            catch (Exception ex)
            {
                FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Save 读取异常: {ex.Message}");
                allProfiles = new Dictionary<string, ContextAdapterConfig>();
            }
        }
        else { allProfiles = new Dictionary<string, ContextAdapterConfig>(); }

        allProfiles[config.ProfileName] = config;
        var outputJson = JsonSerializer.Serialize(allProfiles, new JsonSerializerOptions { WriteIndented = true });
        FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Save: 写入 {outputJson.Length} 字节");
        File.WriteAllText(_configPath, outputJson);
        FileLogger.Instance.WriteLine("[LOG] ProfileStore.Save: 完成");
    }

    public List<string> GetProfileNames()
    {
        FileLogger.Instance.WriteLine("[LOG] ProfileStore.GetProfileNames");
        if (!File.Exists(_configPath))
        {
            FileLogger.Instance.WriteLine("[LOG] ProfileStore.GetProfileNames: 文件不存在");
            return new List<string>();
        }
        try
        {
            var json = File.ReadAllText(_configPath);
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.GetProfileNames: 读取到 {json.Length} 字节");
            var profiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json);
            var names = profiles?.Keys.ToList() ?? new List<string>();
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.GetProfileNames: 返回 {names.Count} 个: [{string.Join(", ", names)}]");
            return names;
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.GetProfileNames 异常: {ex.GetType().Name} - {ex.Message}");
            return new List<string>();
        }
    }

    public void Delete(string profileName)
    {
        if (!File.Exists(_configPath))
            return;
        try
        {
            var json = File.ReadAllText(_configPath);
            var allProfiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json)
                ?? new Dictionary<string, ContextAdapterConfig>();
            allProfiles.Remove(profileName);
            var outputJson = JsonSerializer.Serialize(allProfiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, outputJson);
        }
        catch { }
    }
}
