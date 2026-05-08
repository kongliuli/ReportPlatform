using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xinglin.WebReportEditor.Core.Data;

[Table("RefreshTokens")]
public class RefreshTokenEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public Guid UserId { get; set; }

    public DateTime ExpiryTime { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public virtual UserEntity User { get; set; } = null!;
}
