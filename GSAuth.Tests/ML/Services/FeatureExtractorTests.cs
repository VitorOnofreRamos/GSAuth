using FluentAssertions;
using GSAuth.ML.Services;
using GSAuth.Tests.ML.TestHelpers;
namespace GSAuth.Tests.ML.Services;

public class FeatureExtractorTests
{
    private readonly FeatureExtractor _featureExtractor;

    public FeatureExtractorTests()
    {
        _featureExtractor = new FeatureExtractor();
    }

    [Fact]
    public void ExtractFeatures_WithPerfectMatch_ShouldReturnHighScores()
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed(
            category: "FOOD",
            location: "São Paulo, SP",
            priority: "HIGH",
            quantity: 100);

        var donation = TestDataBuilder.CreateTestDonation(
            category: "FOOD",
            location: "São Paulo, SP",
            quantity: 100);

        var donor = TestDataBuilder.CreateTestUser();
        var organization = TestDataBuilder.CreateTestOrganization();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor, organization);

        // Assert
        features.CategoryMatch.Should().Be(1.0f, "categories match exactly");
        features.LocationDistance.Should().Be(0.0f, "locations match exactly");
        features.QuantityRatio.Should().Be(1.0f, "quantities match exactly");
        features.UrgencyFactor.Should().Be(1.0f, "priority is HIGH");
        features.DonorReliability.Should().BeGreaterThan(0.5f, "donor has some history");
        features.OrganizationTrust.Should().BeGreaterThan(0.5f, "organization has history");
    }

    [Theory]
    [InlineData("FOOD", "FOOD", 1.0f)]
    [InlineData("FOOD", "BEVERAGES", 0.7f)]
    [InlineData("FOOD", "CLOTHING", 0.1f)]
    [InlineData("CLOTHING", "SHOES", 0.7f)]
    [InlineData("MEDICAL", "PHARMACY", 0.7f)]
    [InlineData("EDUCATION", "BOOKS", 0.7f)]
    public void CategoryMatch_ShouldReturnCorrectScores(string needCategory, string donationCategory, float expectedScore)
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed(category: needCategory);
        var donation = TestDataBuilder.CreateTestDonation(category: donationCategory);
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.CategoryMatch.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData(100, 100, 1.0f)]
    [InlineData(100, 50, 0.5f)]
    [InlineData(50, 100, 0.5f)]
    [InlineData(100, 25, 0.25f)]
    [InlineData(0, 100, 0.0f)]
    [InlineData(100, 0, 0.0f)]
    public void QuantityRatio_ShouldCalculateCorrectly(int needQuantity, int donationQuantity, float expectedRatio)
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed(quantity: needQuantity);
        var donation = TestDataBuilder.CreateTestDonation(quantity: donationQuantity);
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.QuantityRatio.Should().Be(expectedRatio);
    }

    [Theory]
    [InlineData("HIGH", 1.0f)]
    [InlineData("MEDIUM", 0.7f)]
    [InlineData("LOW", 0.4f)]
    [InlineData("UNKNOWN", 0.5f)]
    [InlineData(null, 0.5f)]
    public void UrgencyFactor_ShouldMapPriorityCorrectly(string priority, float expectedFactor)
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed(priority: priority);
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.UrgencyFactor.Should().Be(expectedFactor);
    }

    [Fact]
    public void TimeFactor_WithUrgentDeadline_ShouldReturnHighScore()
    {
        // Arrange
        var urgentDeadline = DateTime.Now.AddHours(12); // Very urgent
        var need = TestDataBuilder.CreateTestNeed(deadline: urgentDeadline);
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.TimeFactor.Should().Be(1.0f, "deadline is within 24 hours");
    }

    [Fact]
    public void TimeFactor_WithPastDeadline_ShouldReturnZero()
    {
        // Arrange
        var pastDeadline = DateTime.Now.AddDays(-1);
        var need = TestDataBuilder.CreateTestNeed(deadline: pastDeadline);
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.TimeFactor.Should().Be(0.0f, "deadline has passed");
    }

    [Fact]
    public void ExpirationFactor_WithExpiredDonation_ShouldReturnZero()
    {
        // Arrange
        var expiredDate = DateTime.Now.AddDays(-1);
        var need = TestDataBuilder.CreateTestNeed();
        var donation = TestDataBuilder.CreateTestDonation(expiry: expiredDate);
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.ExpirationFactor.Should().Be(0.0f, "donation has expired");
    }

    [Theory]
    [InlineData(30, 0.5f)]  // New donor
    [InlineData(90, 0.7f)]  // Established donor
    [InlineData(200, 0.9f)] // Veteran donor
    public void DonorReliability_ShouldCalculateBasedOnHistory(int daysAgo, float expectedReliability)
    {
        // Arrange
        var createdAt = DateTime.Now.AddDays(-daysAgo);
        var need = TestDataBuilder.CreateTestNeed();
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser(createdAt: createdAt);

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor);

        // Assert
        features.DonorReliability.Should().Be(expectedReliability);
    }

    [Fact]
    public void ExtractFeatures_WithNullOrganization_ShouldHandleGracefully()
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed();
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var features = _featureExtractor.ExtractFeatures(need, donation, donor, organization: null);

        // Assert
        features.OrganizationTrust.Should().Be(0.6f, "individual need should have default trust");
    }
}
