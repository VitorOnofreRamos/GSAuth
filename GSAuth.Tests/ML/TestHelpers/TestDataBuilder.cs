using GSAuth.Models;

namespace GSAuth.Tests.ML.TestHelpers;

public class TestDataBuilder
{
    public static Need CreateTestNeed(
        string category = "FOOD",
        string location = "São Paulo, SP",
        string priority = "HIGH",
        int quantity = 100,
        DateTime? deadline = null)
    {
        return new Need
        {
            Id = 1,
            Title = "Test Need",
            Description = "Test Description",
            Category = category,
            Location = location,
            Priority = priority,
            Status = "ACTIVE",
            Quantity = quantity,
            Unit = "kg",
            DeadlineDate = deadline ?? DateTime.Now.AddDays(7),
            CreatorId = 1,
            OrganizationId = 1,
            CreatedAt = DateTime.Now
        };
    }

    public static Donation CreateTestDonation(
        string category = "FOOD",
        string location = "São Paulo, SP",
        int quantity = 80,
        DateTime? expiry = null)
    {
        return new Donation
        {
            Id = 1,
            Title = "Test Donation",
            Description = "Test Description",
            Category = category,
            Location = location,
            Status = "AVAILABLE",
            Quantity = quantity,
            Unit = "kg",
            ExpiryDate = expiry ?? DateTime.Now.AddDays(30),
            DonorId = 1,
            CreatedAt = DateTime.Now
        };
    }

    public static User CreateTestUser(DateTime? createdAt = null)
    {
        return new User
        {
            Id = 1,
            Email = "test@example.com",
            Name = "Test User",
            Phone = "123456789",
            Role = "DONOR",
            IsActive = "Y",
            CreatedAt = createdAt ?? DateTime.Now.AddDays(-180),
            OrganizationId = null
        };
    }

    public static Organization CreateTestOrganization(DateTime? createdAt = null)
    {
        return new Organization
        {
            Id = 1,
            Name = "Test Organization",
            Description = "Test Description",
            Location = "São Paulo, SP",
            ContactEmail = "org@example.com",
            ContactPhone = "987654321",
            Type = "NGO",
            CreatedAt = createdAt ?? DateTime.Now.AddDays(-365)
        };
    }
}