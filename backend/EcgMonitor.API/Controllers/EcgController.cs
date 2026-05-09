using EcgMonitor.API.Data;
using EcgMonitor.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcgMonitor.API.Controllers;

[ApiController]
[Route("api/ecg")]
public class EcgController(AppDbContext db) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<DashboardStatsDto> GetStats()
    {
        var today = DateTime.UtcNow.Date;

        var total = await db.EcgRecords.CountAsync();
        var pending = await db.EcgRecords.CountAsync(r => r.Status == ReviewStatus.Pending);
        var reviewedToday = await db.DoctorReviews.CountAsync(r => r.ReviewedAt >= today);
        var diagnoses = await db.EcgRecords
            .Where(r => r.AiDiagnosis != null)
            .GroupBy(r => r.AiDiagnosis!)
            .Select(g => new { Diagnosis = g.Key, Count = g.Count() })
            .ToListAsync();

        return new DashboardStatsDto(
            total,
            pending,
            reviewedToday,
            diagnoses.ToDictionary(d => d.Diagnosis, d => d.Count)
        );
    }

    [HttpGet]
    public async Task<List<EcgSummaryDto>> GetAll(
        [FromQuery] ReviewStatus? status,
        [FromQuery] string? urgency,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = db.EcgRecords.AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        if (!string.IsNullOrEmpty(urgency))
            query = query.Where(r => r.AiUrgency == urgency);

        return await query
            .OrderByDescending(r => r.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new EcgSummaryDto(
                r.Id,
                r.PatientId,
                r.RecordedAt,
                r.HeartRateBpm,
                r.IsAnomaly,
                r.AiDiagnosis,
                r.AiUrgency,
                r.AiConfidence,
                r.Status))
            .ToListAsync();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EcgRecordDto>> GetById(Guid id)
    {
        var record = await db.EcgRecords
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (record is null) return NotFound();

        return ToDto(record);
    }

    private static EcgRecordDto ToDto(EcgRecord r) => new(
        r.Id, r.PatientId, r.RecordedAt, r.SampleRateHz, r.DataPoints,
        r.HeartRateBpm, r.IsAnomaly, r.AiDiagnosis, r.AiReasoning,
        r.AiConfidence, r.AiUrgency, r.Status,
        r.Review is null ? null : new DoctorReviewDto(
            r.Review.Id, r.Review.DoctorName, r.Review.Diagnosis,
            r.Review.Notes, r.Review.AgreedWithAi, r.Review.ReviewedAt));
}
