using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services.ContextAdapter;

public class ContextAdapterFactory
{
    public ContextAdapterService Create(TemplateDefinition template)
    {
        return new ContextAdapterService();
    }
}
