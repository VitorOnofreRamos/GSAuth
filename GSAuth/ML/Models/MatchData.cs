using Microsoft.ML.Data;

namespace GSAuth.ML.Models;

public class MatchData
{
    [LoadColumn(0)]
    public float CategoryMatch { get; set; } // 1 se categorias são iguais, 0 caso contrário

    [LoadColumn(1)]
    public float LocationDistance { get; set; } // Distância normalizada (0-1)

    [LoadColumn(2)]
    public float QuantityRatio { get; set; } // Min(donation, need) / Max(donation, need)

    [LoadColumn(3)]
    public float UrgencyFactor { get; set; } // Baseado na prioridade da necessidade

    [LoadColumn(4)]
    public float TimeFactor { get; set; } // Baseado na proximidade do deadline

    [LoadColumn(5)]
    public float ExpirationFactor { get; set; } // Baseado na proximidade da expiração

    [LoadColumn(6)]
    public float DonorReliability { get; set; } // Histórico do doador (0-1)

    [LoadColumn(7)]
    public float OrganizationTrust { get; set; } // Confiabilidade da organização (0-1)

    [LoadColumn(8)]
    [ColumnName("Label")]
    public float CompatibilityScore { get; set; } // Score alvo (0-100)
}

public class MatchPrediction
{
    [ColumnName("Score")]
    public float CompatibilityScore { get; set; }
}

