using System.IO;
using System.Text.Json;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextProfileStore
{
    private readonly string _configPath;

    public ContextProfileStore()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReportDataMaker");
        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "context-profiles.json");
    }

    public ContextAdapterConfig Load(string profileName)
    {
        if (!File.Exists(_configPath))
            return new ContextAdapterConfig();
        try
        {
            var json = File.ReadAllText(_configPath);
            var profiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json);
            if (profiles != null && profiles.TryGetValue(profileName, out var config))
                return config;
            return new ContextAdapterConfig();
        }
        catch { return new ContextAdapterConfig(); }
    }

    public void Save(ContextAdapterConfig config)
    {
        Dictionary<string, ContextAdapterConfig> allProfiles;
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                allProfiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json)
                    ?? new Dictionary<string, ContextAdapterConfig>();
            }
            catch { allProfiles = new Dictionary<string, ContextAdapterConfig>(); }
        }
        else { allProfiles = new Dictionary<string, ContextAdapterConfig>(); }

        allProfiles[config.ProfileName] = config;
        var outputJson = JsonSerializer.Serialize(allProfiles, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, outputJson);
    }

    public List<string> GetProfileNames()
    {
        if (!File.Exists(_configPath))
            return new List<string>();
        try
        {
            var json = File.ReadAllText(_configPath);
            var profiles = JsonSerializer.Deserialize<Dictionary<string, ContextAdapterConfig>>(json);
            return profiles?.Keys.ToList() ?? new List<string>();
        }
        catch { return new List<string>(); }
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
