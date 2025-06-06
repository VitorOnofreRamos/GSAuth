namespace GSAuth.DTOs;

public class DonationCreateDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public string Status { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long DonorId { get; set; }
}

public class DonationReadDto
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string Category { get; set; }
    public string Status { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long DonorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
