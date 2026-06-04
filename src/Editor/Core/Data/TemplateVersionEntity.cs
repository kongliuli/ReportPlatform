using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xinglin.ReportEditor.Core.Data;

[Table("TemplateVersions")]
public class TemplateVersionEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid TemplateId { get; set; }

    public int VersionNumber { get; set; }

    [Column(TypeName = "TEXT")]
    public string ContentJson { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ChangeDescription { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public virtual TemplateEntity Template { get; set; } = null!;
}
