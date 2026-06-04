using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.DatabaseAdapter;

/// <summary>数据库适配器插件</summary>
public class DatabaseAdapterPlugin : IAdapterPlugin
{
    public string AdapterType => "database";
    public string DisplayName => "数据库适配器";

    public object CreateService(TemplateDefinition template)
    {
        return new DatabaseAdapterService();
    }
}
