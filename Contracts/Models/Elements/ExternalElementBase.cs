using Xinglin.ReportEditor.Contracts.Enums;

namespace Xinglin.ReportEditor.Contracts.Models.Elements;

public abstract class ExternalElementBase : ElementBase
{
    public string? Label { get; set; }
    
    public string? DataPath { get; set; }
    
    public bool IsDataBound => !string.IsNullOrEmpty(DataPath);
    
    public bool IsRequired { get; set; }
    
    public ElementGroup Group { get; set; } = ElementGroup.Fixed;
    
    public string? AdapterId { get; set; }
}
