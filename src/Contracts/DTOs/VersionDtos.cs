namespace Xinglin.WebReportEditor.Contracts.DTOs;

public class TemplateVersionDetailDto : TemplateVersionDto
{
    public string ContentJson { get; set; } = string.Empty;
}

public class VersionDiffResponse
{
    public Guid TemplateId { get; set; }
    public Guid VersionIdA { get; set; }
    public Guid VersionIdB { get; set; }
    public List<ElementDiff> ElementDiffs { get; set; } = new();
    public List<PropertyDiff> PropertyDiffs { get; set; } = new();
}

public class ElementDiff
{
    public string DiffType { get; set; } = string.Empty;
    public string? ElementId { get; set; }
    public string? ElementType { get; set; }
    public string? Description { get; set; }
}

public class PropertyDiff
{
    public string PropertyName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
