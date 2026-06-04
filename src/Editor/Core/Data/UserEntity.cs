using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Xinglin.ReportEditor.Core.Data;

[Table("Users")]
public class UserEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DisplayName { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "editor";

    [StringLength(50)]
    public string? HospitalId { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// S3: 标记用户是否需要在首次登录时修改密码
    /// </summary>
    public bool PasswordChangeRequired { get; set; } = false;

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public virtual ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
}
