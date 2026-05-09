using EcgMonitor.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EcgMonitor.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<EcgRecord> EcgRecords => Set<EcgRecord>();
    public DbSet<DoctorReview> DoctorReviews => Set<DoctorReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EcgRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Review)
             .WithOne(r => r.EcgRecord)
             .HasForeignKey<DoctorReview>(r => r.EcgRecordId);
        });

        modelBuilder.Entity<DoctorReview>(e =>
        {
            e.HasKey(x => x.Id);
        });
    }
}
