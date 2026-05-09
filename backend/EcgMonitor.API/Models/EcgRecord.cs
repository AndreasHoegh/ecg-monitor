using System.ComponentModel.DataAnnotations;

namespace EcgMonitor.API.Models;

public enum ReviewStatus { Pending, Reviewed }

public class EcgRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string PatientId { get; set; } = string.Empty;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public int SampleRateHz { get; set; } = 360;

    public double[] DataPoints { get; set; } = [];

    public double HeartRateBpm { get; set; }

    public bool IsAnomaly { get; set; }

    public string? AiDiagnosis { get; set; }

    public string? AiReasoning { get; set; }

    public double? AiConfidence { get; set; }

    public string? AiUrgency { get; set; }

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    public DoctorReview? Review { get; set; }
}
