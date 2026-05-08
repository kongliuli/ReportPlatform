using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public interface IDataBindingService
{
    void ApplyData(ExternalTemplateDefinition template, Dictionary<string, object> data);
    Dictionary<string, object> ExtractData(ExternalTemplateDefinition template);
}
