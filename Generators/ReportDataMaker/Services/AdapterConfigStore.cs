using System.IO;
using System.Text.Json;
using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services;

/// <summary>适配器配置存储，负责适配器配置的持久化读写</summary>
public class AdapterConfigStore
{
    private readonly string _configPath;

    /// <summary>初始化适配器配置存储，使用默认应用数据路径</summary>
    public AdapterConfigStore()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReportDataMaker");
        Directory.CreateDirectory(appDataPath);
        _configPath = Path.Combine(appDataPath, "adapters.json");
    }

    /// <summary>加载指定模板的适配器配置列表</summary>
    /// <param name="templateName">模板名称</param>
    /// <returns>适配器配置列表</returns>
    public List<AdapterConfigBase> Load(string templateName)
    {
        if (!File.Exists(_configPath))
            return new List<AdapterConfigBase>();
        try
        {
            var json = File.ReadAllText(_configPath);
            var configs = JsonSerializer.Deserialize<Dictionary<string, List<AdapterConfigBase>>>(json);
            return configs?.GetValueOrDefault(templateName, new List<AdapterConfigBase>()) 
                ?? new List<AdapterConfigBase>();
        }
        catch { return new List<AdapterConfigBase>(); }
    }

    /// <summary>保存指定模板的适配器配置列表</summary>
    /// <param name="templateName">模板名称</param>
    /// <param name="configs">适配器配置列表</param>
    public void Save(string templateName, List<AdapterConfigBase> configs)
    {
        Dictionary<string, List<AdapterConfigBase>> allConfigs;
        if (File.Exists(_configPath))
        {
            try
            {
                var json = File.ReadAllText(_configPath);
                allConfigs = JsonSerializer.Deserialize<Dictionary<string, List<AdapterConfigBase>>>(json) 
                    ?? new Dictionary<string, List<AdapterConfigBase>>();
            }
            catch { allConfigs = new Dictionary<string, List<AdapterConfigBase>>(); }
        }
        else { allConfigs = new Dictionary<string, List<AdapterConfigBase>>(); }

        allConfigs[templateName] = configs;
        var outputJson = JsonSerializer.Serialize(allConfigs, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configPath, outputJson);
    }
}
