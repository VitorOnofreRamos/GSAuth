using Xunit;
using FluentAssertions;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Moq;
using GSAuth.ML.Services;
using GSAuth.Tests.ML.TestHelpers;

namespace GSAuth.Tests.ML.Perfomance;

public class PerformanceTests : IDisposable
{
    private readonly CompatibilityMLService _mlService;
    private readonly string _tempDirectory;

    public PerformanceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"PerfTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDirectory);

        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);

        _mlService = new CompatibilityMLService(mockEnvironment.Object);
    }

    [Fact]
    public async Task PredictionPerformance_ShouldBeFastEnough()
    {
        // Arrange
        await _mlService.TrainModelAsync();
        var need = TestDataBuilder.CreateTestNeed();
        var donation = TestDataBuilder.CreateTestDonation();
        var donor = TestDataBuilder.CreateTestUser();

        var stopwatch = new Stopwatch();
        var predictions = new List<float>();

        // Act - Measure 100 predictions
        stopwatch.Start();
        for (int i = 0; i < 100; i++)
        {
            var score = await _mlService.PredictCompatibilityAsync(need, donation, donor);
            predictions.Add(score);
        }
        stopwatch.Stop();

        // Assert
        var averageTime = stopwatch.ElapsedMilliseconds / 100.0;
        averageTime.Should().BeLessThan(10, "each prediction should take less than 10ms on average");

        predictions.Should().AllSatisfy(score =>
            score.Should().BeInRange(0, 100, "all predictions should be valid"));
    }

    [Fact]
    public async Task TrainingPerformance_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _mlService.TrainModelAsync();
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000, "training should complete within 30 seconds");
        (await _mlService.IsModelTrainedAsync()).Should().BeTrue("model should be trained after completion");
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