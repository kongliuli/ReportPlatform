namespace Xinglin.WebReportEditor.Contracts.DTOs;

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Version { get; set; }
    public string? HospitalId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public string? CreatedBy { get; set; }
}
