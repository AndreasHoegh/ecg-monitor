using System.ComponentModel.DataAnnotations;

namespace EcgMonitor.API.Models;

public class DoctorReview
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EcgRecordId { get; set; }

    public EcgRecord EcgRecord { get; set; } = null!;

    [Required]
    public string DoctorName { get; set; } = string.Empty;

    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool AgreedWithAi { get; set; }

    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
}
