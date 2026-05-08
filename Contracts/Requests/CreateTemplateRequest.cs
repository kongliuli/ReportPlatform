using System.ComponentModel.DataAnnotations;

namespace Xinglin.WebReportEditor.Contracts.Requests;

public class CreateTemplateRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [StringLength(50)]
    public string? HospitalId { get; set; }

    [Required]
    public string ContentJson { get; set; } = string.Empty;

    [StringLength(100)]
    public string? CreatedBy { get; set; }
}
