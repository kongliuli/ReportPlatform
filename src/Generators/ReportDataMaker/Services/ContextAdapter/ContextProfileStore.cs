using System.Text.Json;
using ReportDataMaker.Infrastructure;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextProfileStore
{
    private readonly SqliteDatabaseService _db;

    public ContextProfileStore(SqliteDatabaseService db)
    {
        _db = db;
        FileLogger.Instance.WriteLine("[LOG] ===== ContextProfileStore 构造（SQLite） =====");
    }

    public ContextAdapterConfig Load(string profileName)
    {
        FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: profileName='{profileName}'");
        try
        {
            var json = _db.LoadContextProfile(profileName);
            if (json == null)
            {
                FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: 未找到配置文件 '{profileName}'，返回默认");
                return new ContextAdapterConfig();
            }
            var config = JsonSerializer.Deserialize<ContextAdapterConfig>(json);
            if (config != null)
            {
                FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: 找到配置文件 '{profileName}'");
                return config;
            }
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Load: 反序列化失败 '{profileName}'，返回默认");
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
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            _db.SaveContextProfile(config.ProfileName, json);
            FileLogger.Instance.WriteLine("[LOG] ProfileStore.Save: 完成");
        }
        catch (Exception ex)
        {
            FileLogger.Instance.WriteLine($"[LOG] ProfileStore.Save 异常: {ex.GetType().Name} - {ex.Message}");
        }
    }

    public List<string> GetProfileNames()
    {
        FileLogger.Instance.WriteLine("[LOG] ProfileStore.GetProfileNames");
        try
        {
            var names = _db.GetContextProfileNames();
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
        try
        {
            _db.DeleteContextProfile(profileName);
        }
        catch { }
    }
}
