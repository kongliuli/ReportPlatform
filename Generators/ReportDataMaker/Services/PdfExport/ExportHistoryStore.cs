using Newtonsoft.Json;
using ReportDataMaker.Models;

namespace ReportDataMaker.Services.PdfExport;

public class ExportRecord
{
    public DateTime ExportTime { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int RecordCount { get; set; }
}

public class ExportHistoryStore
{
    private readonly string _historyPath;
    private readonly List<ExportRecord> _records = new();
    private const int MaxRecords = 100;

    public ExportHistoryStore()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReportDataMaker");
        Directory.CreateDirectory(appDataPath);
        _historyPath = Path.Combine(appDataPath, "export-history.json");
        Load();
    }

    public void Add(ExportRecord record)
    {
        _records.Insert(0, record);
        if (_records.Count > MaxRecords)
            _records.RemoveAt(_records.Count - 1);
        Save();
    }

    public IReadOnlyList<ExportRecord> GetAll() => _records.AsReadOnly();

    public void Clear()
    {
        _records.Clear();
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_historyPath)) return;
            var json = File.ReadAllText(_historyPath);
            var records = JsonConvert.DeserializeObject<List<ExportRecord>>(json);
            if (records != null)
                _records.AddRange(records);
        }
        catch
        {
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_records, Formatting.Indented);
            File.WriteAllText(_historyPath, json);
        }
        catch
        {
        }
    }
}
