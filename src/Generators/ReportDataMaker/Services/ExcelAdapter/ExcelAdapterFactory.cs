using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.ExcelAdapter;

public class ExcelAdapterFactory
{
    public TemplateFlattenService Create(TemplateDefinition template)
    {
        return new TemplateFlattenService();
    }
}
