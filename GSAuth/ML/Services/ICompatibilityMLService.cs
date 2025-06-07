using GSAuth.Models;

namespace GSAuth.ML.Services;

public interface ICompatibilityMLService
{
    Task<float> PredictCompatibilityAsync(Need need, Donation donation, User donor, Organization organization = null);
    Task TrainModelAsync();
    Task<bool> IsModelTrainedAsync();
}
