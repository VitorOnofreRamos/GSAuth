using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSAuth.Models;

public class Match : _BaseEntity
{
    [Key]
    [Column("ID")]
    public override long Id { get; set; }

    [Required]
    [Column("NEED_ID")]
    public long NeedId { get; set; }

    [ForeignKey(nameof(NeedId))]
    public virtual Need Need { get; set; }

    [Required]
    [Column("DONATION_ID")]
    public long DonationId { get; set; }

    [ForeignKey(nameof(DonationId))]
    public virtual Donation Donation { get; set; }

    [Column("STATUS")]
    [StringLength(20)]
    public string Status { get; set; } // PENDING, CONFIRMED, etc.

    [Column("MATCHED_QUANTITY")]
    public int? MatchedQuantity { get; set; }

    [Column("COMPATIBILITY_SCORE")]
    public int? CompatibilityScore { get; set; } // 0 - 100

    [Column("CONFIRMED_AT")]
    public DateTime? ConfirmedAt { get; set; }

    [Column("NOTES")]
    public string Notes { get; set; }

    [Column("CREATED_AT")]
    public override DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("UPDATED_AT")]
    public override DateTime? UpdatedAt { get; set; }
}
