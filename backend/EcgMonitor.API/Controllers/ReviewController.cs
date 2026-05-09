using EcgMonitor.API.Data;
using EcgMonitor.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcgMonitor.API.Controllers;

[ApiController]
[Route("api/ecg/{ecgId:guid}/review")]
public class ReviewController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DoctorReviewDto>> Create(Guid ecgId, CreateReviewDto dto)
    {
        var record = await db.EcgRecords
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == ecgId);

        if (record is null) return NotFound();
        if (record.Review is not null) return Conflict("This ECG has already been reviewed.");

        var review = new DoctorReview
        {
            EcgRecordId = ecgId,
            DoctorName = dto.DoctorName,
            Diagnosis = dto.Diagnosis,
            Notes = dto.Notes,
            AgreedWithAi = dto.AgreedWithAi,
            ReviewedAt = DateTime.UtcNow
        };

        record.Status = ReviewStatus.Reviewed;
        db.DoctorReviews.Add(review);
        await db.SaveChangesAsync();

        return new DoctorReviewDto(
            review.Id, review.DoctorName, review.Diagnosis,
            review.Notes, review.AgreedWithAi, review.ReviewedAt);
    }
}
