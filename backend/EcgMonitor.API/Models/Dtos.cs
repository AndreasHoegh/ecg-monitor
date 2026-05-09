namespace EcgMonitor.API.Models;

public record EcgRecordDto(
    Guid Id,
    string PatientId,
    DateTime RecordedAt,
    int SampleRateHz,
    double[] DataPoints,
    double HeartRateBpm,
    bool IsAnomaly,
    string? AiDiagnosis,
    string? AiReasoning,
    double? AiConfidence,
    string? AiUrgency,
    ReviewStatus Status,
    DoctorReviewDto? Review
);

public record DoctorReviewDto(
    Guid Id,
    string DoctorName,
    string Diagnosis,
    string Notes,
    bool AgreedWithAi,
    DateTime ReviewedAt
);

public record CreateReviewDto(
    string DoctorName,
    string Diagnosis,
    string Notes,
    bool AgreedWithAi
);

public record EcgSummaryDto(
    Guid Id,
    string PatientId,
    DateTime RecordedAt,
    double HeartRateBpm,
    bool IsAnomaly,
    string? AiDiagnosis,
    string? AiUrgency,
    double? AiConfidence,
    ReviewStatus Status
);

public record DashboardStatsDto(
    int TotalAnomalies,
    int PendingReviews,
    int ReviewedToday,
    Dictionary<string, int> DiagnosisCounts
);
