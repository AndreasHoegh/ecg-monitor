using EcgMonitor.API.Data;
using EcgMonitor.API.Models;
using EcgMonitor.API.Services;

namespace EcgMonitor.API.Background;

public class EcgIngestionWorker(
    IServiceScopeFactory scopeFactory,
    EcgGeneratorService generator,
    ILogger<EcgIngestionWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(20);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ECG ingestion worker started. Generating a record every {Interval}s", Interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);
            await IngestOneAsync(stoppingToken);
        }
    }

    private async Task IngestOneAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ai = scope.ServiceProvider.GetRequiredService<AiAnalysisService>();

        var (signal, patientId) = generator.GenerateEcg();
        var diagnosis = await ai.AnalyzeAsync(signal);

        var record = new EcgRecord
        {
            PatientId = patientId,
            RecordedAt = DateTime.UtcNow,
            SampleRateHz = 360,
            DataPoints = signal.DataPoints,
            HeartRateBpm = signal.HeartRateBpm,
            IsAnomaly = diagnosis.IsAnomaly,
            AiDiagnosis = diagnosis.Diagnosis,
            AiReasoning = diagnosis.Reasoning,
            AiConfidence = diagnosis.Confidence,
            AiUrgency = diagnosis.Urgency,
            Status = ReviewStatus.Pending
        };

        db.EcgRecords.Add(record);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Stored ECG {Id} for {PatientId}: {Diagnosis} (anomaly={IsAnomaly}, urgency={Urgency})",
            record.Id, patientId, diagnosis.Diagnosis, diagnosis.IsAnomaly, diagnosis.Urgency);
    }
}
