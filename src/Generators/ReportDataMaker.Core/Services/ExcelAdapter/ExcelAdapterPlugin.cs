using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.ExcelAdapter;

/// <summary>Excel适配器插件</summary>
public class ExcelAdapterPlugin : IAdapterPlugin
{
    public string AdapterType => "excel";
    public string DisplayName => "Excel 适配器";

    public object CreateService(TemplateDefinition template)
    {
        return new TemplateFlattenService();
    }
}
