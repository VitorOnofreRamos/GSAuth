using FluentAssertions;
using GSAuth.ML.Services;
using GSAuth.Tests.ML.TestHelpers;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace GSAuth.Tests.ML.Integration;

[Collection("ML Integration Tests")]
public class MLIntegrationTests : IDisposable
{
    private readonly CompatibilityMLService _mlService;
    private readonly string _tempDirectory;

    public MLIntegrationTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"MLTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);

        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);

        _mlService = new CompatibilityMLService(mockEnvironment.Object);
    }

    [Fact]
    public async Task EndToEnd_TrainAndPredict_ShouldWorkCorrectly()
    {
        // Arrange
        var need = TestDataBuilder.CreateTestNeed();
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser();

        // Act - Get initial prediction (should use fallback)
        var initialScore = await _mlService.PredictCompatibilityAsync(need, donation, donor);

        // Train the model
        await _mlService.TrainModelAsync();

        // Get prediction with trained model
        var trainedScore = await _mlService.PredictCompatibilityAsync(need, donation, donor);

        // Assert
        initialScore.Should().BeInRange(0, 100);
        trainedScore.Should().BeInRange(0, 100);
        (await _mlService.IsModelTrainedAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task ModelPersistence_ShouldLoadTrainedModel()
    {
        // Arrange & Act - Train model
        await _mlService.TrainModelAsync();
        var scoreAfterTraining = await _mlService.PredictCompatibilityAsync(
            TestDataBuilder.CreateTestNeed(),
            TestDataBuilder.CreateTestDonation(),
            TestDataBuilder.CreateTestUser());

        // Create new service instance (simulates app restart)
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);
        var newService = new CompatibilityMLService(mockEnvironment.Object);

        var scoreAfterReload = await newService.PredictCompatibilityAsync(
            TestDataBuilder.CreateTestNeed(),
            TestDataBuilder.CreateTestDonation(),
            TestDataBuilder.CreateTestUser());

        // Assert
        (await newService.IsModelTrainedAsync()).Should().BeTrue("model should be loaded from disk");
        scoreAfterReload.Should().BeInRange(0, 100);
    }

    [Fact]
    public async Task StressTest_MultipleSimultaneousPredictions_ShouldBeThreadSafe()
    {
        // Arrange
        await _mlService.TrainModelAsync();
        var tasks = new List<Task<float>>();

        // Act - Run 50 simultaneous predictions
        for (int i = 0; i < 50; i++)
        {
            var task = _mlService.PredictCompatibilityAsync(
                TestDataBuilder.CreateTestNeed(),
                TestDataBuilder.CreateTestDonation(),
                TestDataBuilder.CreateTestUser());
            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(score =>
            score.Should().BeInRange(0, 100, "all predictions should be valid"));
        results.Should().NotContain(float.NaN, "no predictions should be NaN");
        results.Should().NotContain(float.PositiveInfinity, "no predictions should be infinite");
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
            // Ignore cleanup errors
        }
    }
}
