namespace Xinglin.WebReportEditor.Contracts.Requests;

public class TemplateFilterRequest
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? HospitalId { get; set; }
    public bool? IsPublished { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
