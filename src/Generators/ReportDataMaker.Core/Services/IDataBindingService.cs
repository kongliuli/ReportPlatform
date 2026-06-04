using Xinglin.ReportEditor.Contracts.Models.Template;

namespace ReportDataMaker.Services;

public interface IDataBindingService
{
    void ApplyData(TemplateDefinition template, Dictionary<string, object> data);
    Dictionary<string, object> ExtractData(TemplateDefinition template);
}
