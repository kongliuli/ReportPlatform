using Xinglin.ReportEditor.Contracts.Models.Elements;

namespace Xinglin.ReportEditor.Contracts.Models.Template;

/// <summary>
/// 模板定义
/// </summary>
public class TemplateDefinition
{
    public string? Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int Version { get; set; } = 1;
    
    public string Type { get; set; } = string.Empty;
    
    public string? HospitalId { get; set; }
    
    public PageSettings PageSettings { get; set; } = new();
    
    public List<DataBindingDefinition> DataBindings { get; set; } = new();
    
    public List<ExternalElementBase> Elements { get; set; } = new();
    
    public bool EnableGlobalFontSize { get; set; }
    
    public double? GlobalFontSize { get; set; }
}
