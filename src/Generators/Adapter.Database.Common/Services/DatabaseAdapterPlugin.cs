using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Adapter.Database.Common.Services;

/// <summary>数据库适配器插件</summary>
public class DatabaseAdapterPlugin : IAdapterPlugin
{
    public string AdapterType => "database";
    public string DisplayName => "数据库适配器";

    public IDataAdapter CreateService(TemplateDefinition template)
    {
        return new DatabaseAdapterService();
    }
}
