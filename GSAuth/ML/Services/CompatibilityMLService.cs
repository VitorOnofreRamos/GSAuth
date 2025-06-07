using GSAuth.ML.Models;
using GSAuth.Models;
using Microsoft.ML;

namespace GSAuth.ML.Services;

public class CompatibilityMLService : ICompatibilityMLService
{
    private readonly MLContext _mlContext;
    private readonly FeatureExtractor _featureExtractor;
    private readonly string _modelPath;
    private ITransformer _trainedModel;

    public CompatibilityMLService(IWebHostEnvironment environment)
    {
        _mlContext = new MLContext(seed: 42);
        _featureExtractor = new FeatureExtractor();
        _modelPath = Path.Combine(environment.ContentRootPath, "ML", "compatibility_model.zip");

        // Try to load existing model
        LoadExistingModel();
    }

    public async Task<float> PredictCompatibilityAsync(Need need, Donation donation, User donor, Organization organization = null)
    {
        if (_trainedModel == null)
        {
            // If no model, use rule-based fallback
            return CalculateRuleBasedCompatibility(need, donation, donor, organization);
        }

        var features = _featureExtractor.ExtractFeatures(need, donation, donor, organization);
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<MatchData, MatchPrediction>(_trainedModel);

        var prediction = predictionEngine.Predict(features);

        // Ensure score is between 0 and 100
        return Math.Max(0, Math.Min(100, prediction.CompatibilityScore));
    }

    public async Task TrainModelAsync()
    {
        // Generate synthetic training data (in production, use real historical data)
        var trainingData = GenerateSyntheticTrainingData();

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Define training pipeline
        var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(MatchData.CategoryMatch),
                nameof(MatchData.LocationDistance),
                nameof(MatchData.QuantityRatio),
                nameof(MatchData.UrgencyFactor),
                nameof(MatchData.TimeFactor),
                nameof(MatchData.ExpirationFactor),
                nameof(MatchData.DonorReliability),
                nameof(MatchData.OrganizationTrust))
            .Append(_mlContext.Regression.Trainers.FastTree(
                numberOfLeaves: 20,
                numberOfTrees: 100,
                minimumExampleCountPerLeaf: 10,
                learningRate: 0.2));

        // Train the model
        _trainedModel = pipeline.Fit(dataView);

        // Save the model
        Directory.CreateDirectory(Path.GetDirectoryName(_modelPath));
        _mlContext.Model.Save(_trainedModel, dataView.Schema, _modelPath);
    }

    public async Task<bool> IsModelTrainedAsync()
    {
        return _trainedModel != null || File.Exists(_modelPath);
    }

    private void LoadExistingModel()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _trainedModel = _mlContext.Model.Load(_modelPath, out var schema);
            }
        }
        catch (Exception ex)
        {
            // Log error and continue without model
            Console.WriteLine($"Failed to load ML model: {ex.Message}");
        }
    }

    private float CalculateRuleBasedCompatibility(Need need, Donation donation, User donor, Organization organization)
    {
        var features = _featureExtractor.ExtractFeatures(need, donation, donor, organization);

        // Simple weighted sum as fallback
        var score = (features.CategoryMatch * 30) +
                   ((1 - features.LocationDistance) * 20) +
                   (features.QuantityRatio * 15) +
                   (features.UrgencyFactor * 10) +
                   (features.TimeFactor * 10) +
                   (features.ExpirationFactor * 8) +
                   (features.DonorReliability * 4) +
                   (features.OrganizationTrust * 3);

        return Math.Max(0, Math.Min(100, score));
    }

    private List<MatchData> GenerateSyntheticTrainingData()
    {
        var random = new Random(42);
        var trainingData = new List<MatchData>();

        // Generate 1000 synthetic training samples
        for (int i = 0; i < 1000; i++)
        {
            var data = new MatchData
            {
                CategoryMatch = (float)random.NextDouble(),
                LocationDistance = (float)random.NextDouble(),
                QuantityRatio = (float)random.NextDouble(),
                UrgencyFactor = (float)random.NextDouble(),
                TimeFactor = (float)random.NextDouble(),
                ExpirationFactor = (float)random.NextDouble(),
                DonorReliability = (float)random.NextDouble(),
                OrganizationTrust = (float)random.NextDouble()
            };

            // Calculate target score based on feature importance
            var targetScore = (data.CategoryMatch * 30) +
                             ((1 - data.LocationDistance) * 20) +
                             (data.QuantityRatio * 15) +
                             (data.UrgencyFactor * 10) +
                             (data.TimeFactor * 10) +
                             (data.ExpirationFactor * 8) +
                             (data.DonorReliability * 4) +
                             (data.OrganizationTrust * 3);

            // Add some noise
            targetScore += (float)(random.NextGaussian() * 5);
            data.CompatibilityScore = Math.Max(0, Math.Min(100, targetScore));

            trainingData.Add(data);
        }

        return trainingData;
    }
}

// Extension method for Gaussian random
public static class RandomExtensions
{
    public static double NextGaussian(this Random random, double mean = 0, double stdDev = 1)
    {
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}