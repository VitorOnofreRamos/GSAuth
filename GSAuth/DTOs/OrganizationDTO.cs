namespace GSAuth.DTOs;

public class OrganizationDTO
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set;}
}
