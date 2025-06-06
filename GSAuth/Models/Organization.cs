using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSAuth.Models;

[Table("GS_ORGANIZATIONS")]
public class Organization : _BaseEntity
{
    [Key]
    [Column("ID")]
    public override long Id { get; set; }

    [Required]
    [Column("NAME")]
    [StringLength(255)]
    public string Name { get; set; }

    [Column("DESCRIPTION")]
    public string Description { get; set; }

    [Required]
    [Column("LOCATION")]
    [StringLength(255)]
    public string Location { get; set; }

    [Column("CONTACT_EMAIL")]
    [StringLength(255)]
    public string ContactEmail { get; set; }

    [Column("CONTACT_PHONE")]
    [StringLength(20)]
    public string ContactPhone { get; set; }

    [Column("TYPE")]
    [StringLength(20)]
    public string Type { get; set; } // NGO, CHARITY, GOVERNMENT, etc.

    [Column("CREATED_AT")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public override DateTime? UpdatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Need> Needs { get; set; } = new List<Need>();
}
