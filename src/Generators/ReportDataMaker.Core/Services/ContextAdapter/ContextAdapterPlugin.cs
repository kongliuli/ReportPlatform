using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.ContextAdapter;

/// <summary>上下文适配器插件</summary>
public class ContextAdapterPlugin : IAdapterPlugin
{
    public string AdapterType => "context";
    public string DisplayName => "上下文适配器";

    public IDataAdapter CreateService(TemplateDefinition template)
    {
        return new ContextAdapterService();
    }
}
