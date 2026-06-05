using Xinglin.ReportEditor.Contracts.Models.Adapters;
using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Adapter.Excel.Services;

/// <summary>Excel适配器插件</summary>
public class ExcelAdapterPlugin : IAdapterPlugin
{
    public string AdapterType => "excel";
    public string DisplayName => "Excel 适配器";

    public IDataAdapter CreateService(TemplateDefinition template)
    {
        return new TemplateFlattenService();
    }
}
