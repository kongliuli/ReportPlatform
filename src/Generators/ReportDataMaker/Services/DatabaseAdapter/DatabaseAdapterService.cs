using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.DatabaseAdapter;

public class DatabaseAdapterService
{
    public string TestConnection(string connectionString)
    {
        return "连接测试功能待实现";
    }

    public Dictionary<string, object> ImportData(TemplateDefinition template, string connectionString, string query)
    {
        return new Dictionary<string, object>();
    }
}
