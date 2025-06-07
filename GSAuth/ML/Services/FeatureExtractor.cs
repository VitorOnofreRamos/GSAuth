using GSAuth.ML.Models;
using GSAuth.Models;

namespace GSAuth.ML.Services;

public class FeatureExtractor
{
    public MatchData ExtractFeatures(Need need, Donation donation, User donor, Organization organization = null)
    {
        return new MatchData
        {
            CategoryMatch = CalculateCategoryMatch(need.Category, donation.Category),
            LocationDistance = CalculateLocationDistance(need.Location, donation.Location),
            QuantityRatio = CalculateQuantityRatio(need.Quantity, donation.Quantity),
            UrgencyFactor = CalculateUrgencyFactor(need.Priority),
            TimeFactor = CalculateTimeFactor(need.DeadlineDate),
            ExpirationFactor = CalculateExpirationFactor(donation.ExpiryDate),
            DonorReliability = CalculateDonorReliability(donor),
            OrganizationTrust = CalculateOrganizationTrust(organization)
        };
    }

    private float CalculateCategoryMatch(string needCategory, string donationCategory)
    {
        if (string.IsNullOrEmpty(needCategory) || string.IsNullOrEmpty(donationCategory))
            return 0.5f;

        // Exact match
        if (needCategory.Equals(donationCategory, StringComparison.OrdinalIgnoreCase))
            return 1.0f;

        // Related categories (you can expand this logic)
        var relatedCategories = new Dictionary<string, string[]>
        {
            ["FOOD"] = new[] { "BEVERAGES", "NUTRITION" },
            ["CLOTHING"] = new[] { "SHOES", "ACCESSORIES" },
            ["MEDICAL"] = new[] { "PHARMACY", "HEALTH" },
            ["EDUCATION"] = new[] { "BOOKS", "SUPPLIES" }
        };

        foreach (var category in relatedCategories)
        {
            if (category.Key.Equals(needCategory, StringComparison.OrdinalIgnoreCase) &&
                category.Value.Contains(donationCategory, StringComparer.OrdinalIgnoreCase))
                return 0.7f;
        }

        return 0.1f; // No match
    }

    private float CalculateLocationDistance(string needLocation, string donationLocation)
    {
        if (string.IsNullOrEmpty(needLocation) || string.IsNullOrEmpty(donationLocation))
            return 0.5f;

        // Simple string similarity (you could implement geocoding for real distances)
        var similarity = CalculateStringSimilarity(needLocation, donationLocation);
        return 1.0f - similarity; // Convert similarity to distance
    }

    private float CalculateQuantityRatio(int needQuantity, int donationQuantity)
    {
        if (needQuantity <= 0 || donationQuantity <= 0)
            return 0.0f;

        var min = Math.Min(needQuantity, donationQuantity);
        var max = Math.Max(needQuantity, donationQuantity);
        return (float)min / max;
    }

    private float CalculateUrgencyFactor(string priority)
    {
        return priority?.ToUpper() switch
        {
            "HIGH" => 1.0f,
            "MEDIUM" => 0.7f,
            "LOW" => 0.4f,
            _ => 0.5f
        };
    }

    private float CalculateTimeFactor(DateTime? deadlineDate)
    {
        if (!deadlineDate.HasValue)
            return 0.5f;

        var daysUntilDeadline = (deadlineDate.Value - DateTime.Now).TotalDays;

        if (daysUntilDeadline <= 0)
            return 0.0f; // Past deadline

        if (daysUntilDeadline <= 1)
            return 1.0f; // Very urgent

        if (daysUntilDeadline <= 7)
            return 0.8f; // Urgent

        if (daysUntilDeadline <= 30)
            return 0.6f; // Moderate

        return 0.4f; // Not urgent
    }

    private float CalculateExpirationFactor(DateTime? expiryDate)
    {
        if (!expiryDate.HasValue)
            return 1.0f; // No expiration

        var daysUntilExpiry = (expiryDate.Value - DateTime.Now).TotalDays;

        if (daysUntilExpiry <= 0)
            return 0.0f; // Already expired

        if (daysUntilExpiry <= 3)
            return 0.3f; // Expiring soon

        if (daysUntilExpiry <= 7)
            return 0.7f; // Some time left

        return 1.0f; // Good shelf life
    }

    private float CalculateDonorReliability(User donor)
    {
        // This would be based on historical data
        // For now, we'll use a simple heuristic
        var daysSinceRegistration = (DateTime.Now - donor.CreatedAt).TotalDays;

        if (daysSinceRegistration < 30)
            return 0.5f; // New donor

        if (daysSinceRegistration < 180)
            return 0.7f; // Established donor

        return 0.9f; // Veteran donor
    }

    private float CalculateOrganizationTrust(Organization organization)
    {
        if (organization == null)
            return 0.6f; // Individual need

        var daysSinceRegistration = (DateTime.Now - organization.CreatedAt).TotalDays;

        if (daysSinceRegistration < 90)
            return 0.6f; // New organization

        if (daysSinceRegistration < 365)
            return 0.8f; // Established organization

        return 1.0f; // Veteran organization
    }

    private float CalculateStringSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
            return 0.0f;

        s1 = s1.ToLower().Trim();
        s2 = s2.ToLower().Trim();

        if (s1 == s2)
            return 1.0f;

        // Simple Levenshtein distance normalized
        var distance = LevenshteinDistance(s1, s2);
        var maxLength = Math.Max(s1.Length, s2.Length);
        return 1.0f - (float)distance / maxLength;
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        var matrix = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[s1.Length, s2.Length];
    }
}
