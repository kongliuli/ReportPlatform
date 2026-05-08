using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

public class DataBindingService : IDataBindingService
{
    public void ApplyData(ExternalTemplateDefinition template, Dictionary<string, object> data)
    {
        if (template.Elements == null) return;
        foreach (var element in template.Elements)
        {
            if (!string.IsNullOrEmpty(element.DataPath) && data.TryGetValue(element.DataPath, out var value))
            {
                element.DefaultValue = value?.ToString() ?? string.Empty;
            }
        }
    }

    public Dictionary<string, object> ExtractData(ExternalTemplateDefinition template)
    {
        var data = new Dictionary<string, object>();
        if (template.Elements == null) return data;
        foreach (var element in template.Elements)
        {
            if (!string.IsNullOrEmpty(element.DataPath))
            {
                data[element.DataPath] = element.DefaultValue ?? string.Empty;
            }
        }
        return data;
    }
}
