using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSAuth.Models;

[Table("GS_USERS")]
public class User : _BaseEntity
{
    [Key]
    [Column("ID")]
    public override long Id { get; set; }

    [Required]
    [Column("EMAIL")]
    [StringLength(255)]
    public string Email { get; set; }

    [Column("PHONE")]
    [StringLength(20)]
    public string Phone { get; set; }

    [Required]
    [Column("NAME")]
    [StringLength(255)]
    public string Name { get; set; }

    [Column("PASSWORD_HASH")]
    [StringLength(255)]
    public string PasswordHash { get; set; }

    [Column("ROLE")]
    [StringLength(20)]
    public string Role { get; set; }

    [Column("IS_ACTIVE")]
    [StringLength(1)]
    public string IsActive { get; set; } = "Y";

    [Column("LAST_LOGIN")]
    public DateTime? LastLogin { get; set; }

    [Column("CREATED_AT")]
    public override DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("UPDATED_AT")]
    public override DateTime? UpdatedAt { get; set; }

    [Column("ORGANIZATION_ID")]
    public long? OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; }

    public virtual ICollection<Need> CreatedNeeds { get; set; } = new List<Need>();
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();
}
