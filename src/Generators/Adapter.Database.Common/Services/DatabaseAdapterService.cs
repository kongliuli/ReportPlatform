using Xinglin.ReportEditor.Contracts.Enums;
using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Adapter.Database.Common.Services;

public class DatabaseAdapterService : IDataAdapter
{
    public string AdapterId => "database-default";
    public string AdapterName => "数据库适配器";
    public AdapterType Type => AdapterType.Database;
    public IReadOnlyList<string> TargetDataPaths => Array.Empty<string>();

    public Task<AdapterResult> ReadDataAsync() => throw new NotImplementedException();
    public Task<AdapterResult> ReadBatchDataAsync() => throw new NotImplementedException();
    public Task<ValidationResult> ValidateConfigAsync() => Task.FromResult(ValidationResult.Success);

    public string TestConnection(string connectionString)
    {
        return "连接测试功能待实现";
    }

    public Dictionary<string, object> ImportData(TemplateDefinition template, string connectionString, string query)
    {
        return new Dictionary<string, object>();
    }
}
