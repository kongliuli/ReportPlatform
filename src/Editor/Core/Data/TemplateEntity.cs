using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xinglin.ReportEditor.Core.Data;

[Table("Templates")]
public class TemplateEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    [Column(TypeName = "TEXT")]
    public string ContentJson { get; set; } = string.Empty;

    [StringLength(50)]
    public string? HospitalId { get; set; }

    public bool IsDefault { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public virtual ICollection<TemplateVersionEntity> Versions { get; set; } = new List<TemplateVersionEntity>();
}
