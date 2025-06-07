using FluentAssertions;
using GSAuth.ML.Services;
using GSAuth.Tests.ML.TestHelpers;
using Microsoft.AspNetCore.Hosting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSAuth.Tests.ML.Services;

public class CompatibilityMLServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly CompatibilityMLService _service;
    private readonly string _tempDirectory;

    public CompatibilityMLServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);

        _service = new CompatibilityMLService(_mockEnvironment.Object);
    }

    [Fact]
    public async Task PredictCompatibilityAsync_WithoutTrainedModel_ShouldUseFallback()
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed(
            category: "FOOD",
            location: "São Paulo, SP",
            priority: "HIGH");

        var donation = TestDataBuilder.CreateTestDonation(
            category: "FOOD",
            location: "São Paulo, SP");

        var donor = TestDataBuilder.CreateTestUser();
        var organization = TestDataBuilder.CreateTestOrganization();

        // Act
        var score = await _service.PredictCompatibilityAsync(need, donation, donor, organization);

        // Assert
        score.Should().BeInRange(0, 100, "score should be within valid range");
        score.Should().BeGreaterThan(50, "perfect match should have high score");
    }

    [Fact]
    public async Task PredictCompatibilityAsync_WithPerfectMatch_ShouldReturnHighScore()
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

        var donor = TestDataBuilder.CreateTestUser(createdAt: DateTime.Now.AddDays(-365));
        var organization = TestDataBuilder.CreateTestOrganization(createdAt: DateTime.Now.AddDays(-730));

        // Act
        var score = await _service.PredictCompatibilityAsync(need, donation, donor, organization);

        // Assert
        score.Should().BeGreaterThan(80, "perfect match should have very high score");
    }

    [Fact]
    public async Task PredictCompatibilityAsync_WithPoorMatch_ShouldReturnLowScore()
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed(
            category: "FOOD",
            location: "São Paulo, SP",
            priority: "HIGH",
            deadline: DateTime.Now.AddDays(-1)); // Past deadline

        var donation = TestDataBuilder.CreateTestDonation(
            category: "CLOTHING", // Different category
            location: "Rio de Janeiro, RJ", // Different location
            expiry: DateTime.Now.AddDays(-1)); // Expired

        var donor = TestDataBuilder.CreateTestUser(createdAt: DateTime.Now.AddDays(-10)); // New donor

        // Act
        var score = await _service.PredictCompatibilityAsync(need, donation, donor);

        // Assert
        score.Should().BeLessThan(30, "poor match should have low score");
    }

    [Fact]
    public async Task TrainModelAsync_ShouldCompleteSuccessfully()
    {
        // Act & Assert
        var act = async () => await _service.TrainModelAsync();
        await act.Should().NotThrowAsync("training should complete without errors");

        // Verify model file was created
        var modelPath = Path.Combine(_tempDirectory, "ML", "compatibility_model.zip");
        File.Exists(modelPath).Should().BeTrue("model file should be saved");
    }

    [Fact]
    public async Task IsModelTrainedAsync_WithoutModel_ShouldReturnFalse()
    {
        // Act
        var isModelTrained = await _service.IsModelTrainedAsync();

        // Assert
        isModelTrained.Should().BeFalse("no model should be available initially");
    }

    [Fact]
    public async Task IsModelTrainedAsync_AfterTraining_ShouldReturnTrue()
    {
        // Arrange
        await _service.TrainModelAsync();

        // Act
        var isModelTrained = await _service.IsModelTrainedAsync();

        // Assert
        isModelTrained.Should().BeTrue("model should be available after training");
    }

    [Theory]
    [InlineData("FOOD", "FOOD", "HIGH", 0, 0, 1.0)] // Perfect match, urgent, fresh
    [InlineData("FOOD", "CLOTHING", "LOW", 10, 5, 0.5)] // Category mismatch, not urgent
    [InlineData("MEDICAL", "MEDICAL", "HIGH", -1, 0, 0.0)] // Past deadline
    public async Task PredictCompatibilityAsync_WithVariousScenarios_ShouldReturnExpectedRanges(
        string needCategory,
        string donationCategory,
        string priority,
        int deadlineDaysFromNow,
        int expiryDaysFromNow,
        double expectedScoreRatio)
    {
        // Arrange
        var deadline = deadlineDaysFromNow == 0 ? (DateTime?)null : DateTime.Now.AddDays(deadlineDaysFromNow);
        var expiry = expiryDaysFromNow == 0 ? (DateTime?)null : DateTime.Now.AddDays(expiryDaysFromNow);

        var need = TestDataBuilder.CreateTestNeed(
            category: needCategory,
            priority: priority,
            deadline: deadline);

        var donation = TestDataBuilder.CreateTestDonation(
            category: donationCategory,
            expiry: expiry);

        var donor = TestDataBuilder.CreateTestUser();

        // Act
        var score = await _service.PredictCompatibilityAsync(need, donation, donor);

        // Assert
        var expectedMinScore = expectedScoreRatio * 100 * 0.7; // Allow 30% variance
        var expectedMaxScore = Math.Min(100, expectedScoreRatio * 100 * 1.3);

        score.Should().BeInRange((float)expectedMinScore, (float)expectedMaxScore,
            $"score should be roughly {expectedScoreRatio * 100}% for this scenario");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }
}
