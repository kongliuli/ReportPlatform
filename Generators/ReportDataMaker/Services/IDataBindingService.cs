using ReportDataMaker.Models;

namespace ReportDataMaker.Services;

/// <summary>数据绑定服务接口，定义模板数据应用和提取的契约</summary>
public interface IDataBindingService
{
    /// <summary>将数据应用到模板元素上</summary>
    /// <param name="template">外部模板定义</param>
    /// <param name="data">数据字典</param>
    void ApplyData(ExternalTemplateDefinition template, Dictionary<string, object> data);
    /// <summary>从模板元素中提取数据</summary>
    /// <param name="template">外部模板定义</param>
    /// <returns>数据字典</returns>
    Dictionary<string, object> ExtractData(ExternalTemplateDefinition template);
}
