using System.ComponentModel.DataAnnotations;

namespace Xinglin.WebReportEditor.Contracts.Requests;

public class UpdateTemplateRequest
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    [MaxLength(5_000_000)]
    public string? ContentJson { get; set; }

    public bool? IsPublished { get; set; }

    [StringLength(500)]
    public string? ChangeDescription { get; set; }
}
