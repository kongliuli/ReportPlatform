namespace Xinglin.WebReportEditor.Contracts.DTOs;

public class TemplateVersionDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string? ChangeDescription { get; set; }
    public DateTime CreateTime { get; set; }
    public string? CreatedBy { get; set; }
}
