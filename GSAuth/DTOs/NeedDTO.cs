namespace GSAuth.DTOs;

public class NeedCreateDTO
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public DateTime? DeadlineDate { get; set; }
    public long CreatorId { get; set; }
    public long? OrganizationId { get; set; }
}

public class NeedReadDTO
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public string Priority { get; set; }
    public string Status { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public DateTime? DeadlineDate { get; set; }
    public long CreatorId { get; set; }
    public long? OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
