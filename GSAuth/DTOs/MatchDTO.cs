namespace GSAuth.DTOs;

public class MatchCreateDto
{
    public long NeedId { get; set; }
    public long DonationId { get; set; }
    public string Status { get; set; }
    public int? MatchedQuantity { get; set; }
    public int? CompatibilityScore { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string Notes { get; set; }
}

public class MatchReadDto
{
    public long Id { get; set; }
    public long NeedId { get; set; }
    public long DonationId { get; set; }
    public string Status { get; set; }
    public int? MatchedQuantity { get; set; }
    public int? CompatibilityScore { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
