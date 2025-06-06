using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GSAuth.Models;

[Table("GS_NEEDS")]
public class Need : _BaseEntity
{
    [Key]
    [Column("ID")]
    public override long Id { get; set; }

    [Required]
    [Column("TITLE")]
    [StringLength(255)]
    public string Title { get; set; }

    [Column("DESCRIPTION")]
    public string Description { get; set; }

    [Required]
    [Column("LOCATION")]
    [StringLength(255)]
    public string Location { get; set; }

    [Column("CATEGORY")]
    [StringLength(20)]
    public string Category { get; set; } // FOOD, WATER, etc.

    [Column("PRIORITY")]
    [StringLength(10)]
    public string Priority { get; set; } // LOW, MEDIUM, etc.

    [Column("STATUS")]
    [StringLength(20)]
    public string Status { get; set; } // ACTIVE, FULFILLED, etc.

    [Required]
    [Column("QUANTITY")]
    public int Quantity { get; set; }

    [Column("UNIT")]
    [StringLength(50)]
    public string Unit { get; set; }

    [Column("DEADLINE_DATE")]
    public DateTime? DeadlineDate { get; set; }

    [Column("CREATED_AT")]
    public override DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("UPDATED_AT")]
    public override DateTime? UpdatedAt { get; set; }

    [Required]
    [Column("CREATOR_ID")]
    public long CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public virtual User Creator { get; set; }

    [Column("ORGANIZATION_ID")]
    public long? OrganizationId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public virtual Organization Organization { get; set; }

    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
}
