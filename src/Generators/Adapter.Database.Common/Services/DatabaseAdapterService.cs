using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Adapter.Database.Common.Services;

public class DatabaseAdapterService : IDataAdapter
{
    private DatabaseAdapterBase? _baseAdapter;

    public string AdapterId => "database-default";
    public string AdapterName => "数据库适配器";
    public AdapterType Type => AdapterType.Database;
    public IReadOnlyList<string> TargetDataPaths => _baseAdapter?.TargetDataPaths ?? Array.Empty<string>();

    public Task<AdapterResult> ReadDataAsync()
    {
        if (_baseAdapter == null)
            return Task.FromResult(new AdapterResult { Success = false, ErrorMessage = "未配置数据库适配器" });
        return _baseAdapter.ReadDataAsync();
    }

    public Task<AdapterResult> ReadBatchDataAsync()
    {
        if (_baseAdapter == null)
            return Task.FromResult(new AdapterResult { Success = false, ErrorMessage = "未配置数据库适配器" });
        return _baseAdapter.ReadBatchDataAsync();
    }

    public Task<ValidationResult> ValidateConfigAsync()
    {
        if (_baseAdapter == null)
            return Task.FromResult(ValidationResult.Success);
        return _baseAdapter.ValidateConfigAsync();
    }

    public void SetBaseAdapter(DatabaseAdapterBase baseAdapter) => _baseAdapter = baseAdapter;

    public string TestConnection(string connectionString)
    {
        return "连接测试功能待实现";
    }

    public Dictionary<string, object> ImportData(TemplateDefinition template, string connectionString, string query)
    {
        return new Dictionary<string, object>();
    }
}
