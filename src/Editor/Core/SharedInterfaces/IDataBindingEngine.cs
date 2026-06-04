using Xinglin.ReportEditor.Contracts.Models.Template;

namespace Xinglin.ReportEditor.Core.SharedInterfaces;

public interface IDataBindingEngine
{
    /// <summary>Apply data binding to a template definition with dictionary data</summary>
    TemplateDefinition ApplyDataBinding(TemplateDefinition template, Dictionary<string, object> sampleData);

    /// <summary>Apply data binding from JSON strings</summary>
    TemplateDefinition ApplyDataBinding(string templateJson, string sampleDataJson);

    /// <summary>Apply data binding from JSON template with dictionary data</summary>
    TemplateDefinition ApplyDataBinding(string templateJson, Dictionary<string, object> sampleData);
}
