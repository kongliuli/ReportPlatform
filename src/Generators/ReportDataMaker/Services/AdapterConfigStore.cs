using System.Text.Json;
using System.Text.Json.Nodes;
using Xinglin.ReportEditor.Contracts.Models.Adapters;

namespace ReportDataMaker.Services;

public class AdapterConfigStore
{
    private readonly SqliteDatabaseService _db;

    public AdapterConfigStore(SqliteDatabaseService db)
    {
        _db = db;
    }

    public List<AdapterConfigBase> Load(string templateName)
    {
        try
        {
            var json = _db.LoadAdapterConfigs(templateName);
            if (json == null) return new List<AdapterConfigBase>();
            DecryptConnectionStrings(ref json);
            var configs = JsonSerializer.Deserialize<List<AdapterConfigBase>>(json);
            return configs ?? new List<AdapterConfigBase>();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[AdapterConfigStore] 加载配置失败: {ex.Message}"); return new List<AdapterConfigBase>(); }
    }

    public void Save(string templateName, List<AdapterConfigBase> configs)
    {
        var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
        EncryptConnectionStrings(ref json);
        _db.SaveAdapterConfigs(templateName, json);
    }

    private static void EncryptConnectionStrings(ref string json)
    {
        try
        {
            var node = JsonNode.Parse(json);
            if (node == null) return;
            ProcessConnectionStrings(node, encrypt: true);
            json = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[AdapterConfigStore] 加密连接串失败: {ex.Message}"); }
    }

    private static void DecryptConnectionStrings(ref string json)
    {
        try
        {
            var node = JsonNode.Parse(json);
            if (node == null) return;
            ProcessConnectionStrings(node, encrypt: false);
            json = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[AdapterConfigStore] 解密连接串失败: {ex.Message}"); }
    }

    private static void ProcessConnectionStrings(JsonNode node, bool encrypt)
    {
        if (node is JsonObject obj)
        {
            if (obj.ContainsKey("ConnectionString"))
            {
                var val = obj["ConnectionString"]?.GetValue<string>();
                if (!string.IsNullOrEmpty(val))
                {
                    obj["ConnectionString"] = encrypt
                        ? ConfigProtector.Protect(val)
                        : ConfigProtector.Unprotect(val);
                }
            }
            foreach (var prop in obj.ToList())
                if (prop.Value != null) ProcessConnectionStrings(prop.Value, encrypt);
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
                if (item != null) ProcessConnectionStrings(item, encrypt);
        }
    }
}
