using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSAuth.Models;

[Table("GS_DONATIONS")]
public class Donation : _BaseEntity
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
    public string Category { get; set; }

    [Column("STATUS")]
    [StringLength(20)]
    public string Status { get; set; } = "AVAILABLE";

    [Required]
    [Column("QUANTITY")]
    public int Quantity { get; set; }

    [Column("UNIT")]
    [StringLength(50)]
    public string Unit { get; set; }

    [Column("EXPIRY_DATE")]
    public DateTime? ExpiryDate { get; set; }

    [Column("CREATED_AT")]
    public override DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("UPDATED_AT")]
    public override DateTime? UpdatedAt { get; set; }

    [Required]
    [Column("DONOR_ID")]
    public long DonorId { get; set; }

    [ForeignKey(nameof(DonorId))]
    public virtual User Donor { get; set; }

    public virtual ICollection<Match> Users { get; set; } = new List<Match>();
}
