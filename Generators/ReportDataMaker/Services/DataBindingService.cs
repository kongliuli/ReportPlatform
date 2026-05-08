using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

/// <summary>数据绑定服务实现，提供模板数据的应用和提取功能</summary>
public class DataBindingService : IDataBindingService
{
    /// <summary>将数据应用到模板元素上</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="data">数据字典</param>
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

    /// <summary>从模板元素中提取数据</summary>
    /// <param name="template">外部模板定义</param>
    /// <returns>数据字典</returns>
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
