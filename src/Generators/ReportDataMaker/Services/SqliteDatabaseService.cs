using System.IO;
using Microsoft.Data.Sqlite;

namespace ReportDataMaker.Services;

public sealed class SqliteDatabaseService : IDisposable
{
    private readonly string _dbPath;
    private readonly SqliteConnection _connection;
    private bool _initialized;

    public SqliteDatabaseService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReportDataMaker");
        Directory.CreateDirectory(appDataPath);
        _dbPath = Path.Combine(appDataPath, "config.db");
        _connection = new SqliteConnection($"Data Source={_dbPath}");
        _connection.Open();
        EnableWalMode();
    }

    private void EnableWalMode()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL";
        cmd.ExecuteNonQuery();
    }

    public void EnsureInitialized()
    {
        if (_initialized) return;
        CreateTables();
        MigrateFromJson();
        _initialized = true;
    }

    private void CreateTables()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS adapter_configs (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                template_name TEXT NOT NULL,
                config_json TEXT NOT NULL,
                updated_at TEXT NOT NULL DEFAULT (datetime('now')),
                UNIQUE(template_name)
            );
            CREATE TABLE IF NOT EXISTS context_profiles (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                profile_name TEXT NOT NULL UNIQUE,
                config_json TEXT NOT NULL,
                updated_at TEXT NOT NULL DEFAULT (datetime('now'))
            );";
        cmd.ExecuteNonQuery();
    }

    private void MigrateFromJson()
    {
        var appDataPath = Path.GetDirectoryName(_dbPath)!;

        var adaptersJsonPath = Path.Combine(appDataPath, "adapters.json");
        if (File.Exists(adaptersJsonPath))
        {
            try
            {
                var json = File.ReadAllText(adaptersJsonPath);
                var configs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json);
                if (configs != null)
                {
                    using var checkCmd = _connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM adapter_configs";
                    var count = Convert.ToInt64(checkCmd.ExecuteScalar());
                    if (count == 0)
                    {
                        foreach (var kvp in configs)
                        {
                            using var insertCmd = _connection.CreateCommand();
                            insertCmd.CommandText = "INSERT OR IGNORE INTO adapter_configs (template_name, config_json) VALUES (@name, @json)";
                            insertCmd.Parameters.AddWithValue("@name", kvp.Key);
                            insertCmd.Parameters.AddWithValue("@json", System.Text.Json.JsonSerializer.Serialize(kvp.Value));
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SqliteDatabaseService] 适配器配置迁移失败: {ex.Message}"); }
        }

        var profilesJsonPath = Path.Combine(appDataPath, "context-profiles.json");
        if (File.Exists(profilesJsonPath))
        {
            try
            {
                var json = File.ReadAllText(profilesJsonPath);
                var profiles = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (profiles != null)
                {
                    using var checkCmd = _connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM context_profiles";
                    var count = Convert.ToInt64(checkCmd.ExecuteScalar());
                    if (count == 0)
                    {
                        foreach (var kvp in profiles)
                        {
                            using var insertCmd = _connection.CreateCommand();
                            insertCmd.CommandText = "INSERT OR IGNORE INTO context_profiles (profile_name, config_json) VALUES (@name, @json)";
                            insertCmd.Parameters.AddWithValue("@name", kvp.Key);
                            insertCmd.Parameters.AddWithValue("@json", kvp.Value.ToString());
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[SqliteDatabaseService] ContextProfiles迁移失败: {ex.Message}"); }
        }
    }

    public string? LoadAdapterConfigs(string templateName)
    {
        EnsureInitialized();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT config_json FROM adapter_configs WHERE template_name = @name";
        cmd.Parameters.AddWithValue("@name", templateName);
        return cmd.ExecuteScalar() as string;
    }

    public void SaveAdapterConfigs(string templateName, string configJson)
    {
        EnsureInitialized();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO adapter_configs (template_name, config_json, updated_at) 
            VALUES (@name, @json, datetime('now'))
            ON CONFLICT(template_name) DO UPDATE SET config_json = @json, updated_at = datetime('now')";
        cmd.Parameters.AddWithValue("@name", templateName);
        cmd.Parameters.AddWithValue("@json", configJson);
        cmd.ExecuteNonQuery();
    }

    public string? LoadContextProfile(string profileName)
    {
        EnsureInitialized();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT config_json FROM context_profiles WHERE profile_name = @name";
        cmd.Parameters.AddWithValue("@name", profileName);
        return cmd.ExecuteScalar() as string;
    }

    public void SaveContextProfile(string profileName, string configJson)
    {
        EnsureInitialized();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO context_profiles (profile_name, config_json, updated_at) 
            VALUES (@name, @json, datetime('now'))
            ON CONFLICT(profile_name) DO UPDATE SET config_json = @json, updated_at = datetime('now')";
        cmd.Parameters.AddWithValue("@name", profileName);
        cmd.Parameters.AddWithValue("@json", configJson);
        cmd.ExecuteNonQuery();
    }

    public List<string> GetContextProfileNames()
    {
        EnsureInitialized();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT profile_name FROM context_profiles ORDER BY profile_name";
        var names = new List<string>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            names.Add(reader.GetString(0));
        return names;
    }

    public void DeleteContextProfile(string profileName)
    {
        EnsureInitialized();
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM context_profiles WHERE profile_name = @name";
        cmd.Parameters.AddWithValue("@name", profileName);
        cmd.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
