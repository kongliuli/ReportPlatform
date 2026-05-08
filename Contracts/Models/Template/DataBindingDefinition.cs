namespace Xinglin.ReportEditor.Contracts.Models.Template;

public class DataBindingDefinition
{
    public string? Id { get; set; }
    
    public string ElementId { get; set; } = string.Empty;
    
    public string DataPath { get; set; } = string.Empty;
    
    public BindingType BindingType { get; set; } = BindingType.Text;
    
    public string? FormatString { get; set; }
    
    public string? DefaultValue { get; set; }
    
    public string? Transform { get; set; }
}

public enum BindingType
{
    Text,
    Image,
    Visibility,
    Repeat,
    Style
}
