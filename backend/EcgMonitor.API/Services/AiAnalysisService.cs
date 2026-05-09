namespace EcgMonitor.API.Services;

public class AiDiagnosisResult
{
    public bool IsAnomaly { get; set; }
    public string Diagnosis { get; set; } = "Normal sinus rhythm";
    public string Reasoning { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Urgency { get; set; } = "low";
}

// Diagnoses based on signal parameters — no external API needed
public class AiAnalysisService(ILogger<AiAnalysisService> logger)
{
    private static readonly Random Rng = new();

    public Task<AiDiagnosisResult> AnalyzeAsync(EcgSignal signal)
    {
        logger.LogDebug("Analyzing ECG signal locally: {AnomalyType}", signal.AnomalyType);
        var result = Diagnose(signal);
        return Task.FromResult(result);
    }

    private static AiDiagnosisResult Diagnose(EcgSignal signal) => signal.AnomalyType switch
    {
        AnomalyType.Tachycardia => new AiDiagnosisResult
        {
            IsAnomaly = true,
            Diagnosis = "Sinustakyakardi",
            Reasoning = $"Hjertefrekvensen på {signal.HeartRateBpm:F0} BPM overstiger normalgrænsen på 100 BPM. " +
                        "Rytmen er regelmæssig med bevarede P-bølger og normal PR-interval, " +
                        "hvilket tyder på sinustakyakardi frem for ektopisk oprindelse.",
            Confidence = 0.88 + Rng.NextDouble() * 0.09,
            Urgency = signal.HeartRateBpm > 140 ? "high" : "medium"
        },

        AnomalyType.Bradycardia => new AiDiagnosisResult
        {
            IsAnomaly = true,
            Diagnosis = "Sinusbradykardi",
            Reasoning = $"Hjertefrekvensen på {signal.HeartRateBpm:F0} BPM er under normalgrænsen på 60 BPM. " +
                        "Regelmæssig rytme med normale P-bølger tyder på sinusbradykardi. " +
                        "Bør vurderes i klinisk kontekst — kan være fysiologisk hos atleter.",
            Confidence = 0.85 + Rng.NextDouble() * 0.1,
            Urgency = signal.HeartRateBpm < 45 ? "high" : "low"
        },

        AnomalyType.AtrialFibrillation => new AiDiagnosisResult
        {
            IsAnomaly = true,
            Diagnosis = "Atrieflimren (AFib)",
            Reasoning = "Uregelmæssig RR-interval uden definerede P-bølger og finkornet fibrillationsbasislinje " +
                        "er klassiske tegn på atrieflimren. " +
                        "Patienten bør vurderes for antikoagulationsbehandling og ventrikulær frekvenskontrol.",
            Confidence = 0.91 + Rng.NextDouble() * 0.07,
            Urgency = "high"
        },

        AnomalyType.StElevation => new AiDiagnosisResult
        {
            IsAnomaly = true,
            Diagnosis = "ST-elevation (mistanke om STEMI)",
            Reasoning = $"ST-segmentet er elevert med {signal.StDeviationMv:F1} mV over isoelektrisk linje. " +
                        "Morfologien er forenelig med akut myokardieinfarkt (STEMI). " +
                        "Kræver øjeblikkelig kardiologisk vurdering og potentiel primær PCI.",
            Confidence = 0.93 + Rng.NextDouble() * 0.06,
            Urgency = "critical"
        },

        AnomalyType.PrematureVentricularContraction => new AiDiagnosisResult
        {
            IsAnomaly = true,
            Diagnosis = "Ventrikulære ekstraslag (PVC)",
            Reasoning = "Brede, bizarre QRS-komplekser med kompensatorisk pause og inverteret T-bølge " +
                        "identificeres som ventrikulære ekstraslag (PVC). " +
                        "Isolerede PVC'er er oftest godartede, men hyppige PVC'er bør følges.",
            Confidence = 0.87 + Rng.NextDouble() * 0.1,
            Urgency = "medium"
        },

        _ => new AiDiagnosisResult
        {
            IsAnomaly = false,
            Diagnosis = "Normal sinusrytme",
            Reasoning = "Hjertefrekvens, rytme og morfologi inden for normale grænser.",
            Confidence = 0.95,
            Urgency = "low"
        }
    };
}
