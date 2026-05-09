namespace EcgMonitor.API.Services;

public enum AnomalyType
{
    Normal,
    Tachycardia,
    Bradycardia,
    AtrialFibrillation,
    StElevation,
    PrematureVentricularContraction
}

public class EcgSignal
{
    public double[] DataPoints { get; set; } = [];
    public double HeartRateBpm { get; set; }
    public AnomalyType AnomalyType { get; set; }
    public bool IsAnomaly => AnomalyType != AnomalyType.Normal;
    public double PrIntervalMs { get; set; }
    public double QrsDurationMs { get; set; }
    public double QtIntervalMs { get; set; }
    public double StDeviationMv { get; set; }
    public bool RhythmRegular { get; set; }
    public bool PwavePresent { get; set; }
}

public class EcgGeneratorService
{
    private readonly Random _rng = new();
    private const int SampleRate = 360;
    private const double DurationSeconds = 10.0;

    public (EcgSignal Signal, string PatientId) GenerateEcg()
    {
        var patientId = $"P{_rng.Next(1000, 9999)}";
        var anomalyRoll = _rng.NextDouble();

        // 30% chance of anomaly
        var anomalyType = anomalyRoll switch
        {
            < 0.70 => AnomalyType.Normal,
            < 0.82 => AnomalyType.Tachycardia,
            < 0.88 => AnomalyType.Bradycardia,
            < 0.93 => AnomalyType.AtrialFibrillation,
            < 0.97 => AnomalyType.StElevation,
            _ => AnomalyType.PrematureVentricularContraction
        };

        var signal = anomalyType switch
        {
            AnomalyType.Normal => GenerateNormalSinus(),
            AnomalyType.Tachycardia => GenerateTachycardia(),
            AnomalyType.Bradycardia => GenerateBradycardia(),
            AnomalyType.AtrialFibrillation => GenerateAFib(),
            AnomalyType.StElevation => GenerateStElevation(),
            AnomalyType.PrematureVentricularContraction => GeneratePvc(),
            _ => GenerateNormalSinus()
        };

        signal.AnomalyType = anomalyType;
        return (signal, patientId);
    }

    private EcgSignal GenerateNormalSinus()
    {
        var hr = 60 + _rng.Next(0, 41); // 60-100 bpm
        return new EcgSignal
        {
            DataPoints = GenerateBeats(hr, stDeviation: 0, regular: true, pWave: true),
            HeartRateBpm = hr,
            PrIntervalMs = 160 + _rng.Next(-20, 21),
            QrsDurationMs = 90 + _rng.Next(-10, 11),
            QtIntervalMs = 380 + _rng.Next(-30, 31),
            StDeviationMv = 0.0,
            RhythmRegular = true,
            PwavePresent = true
        };
    }

    private EcgSignal GenerateTachycardia()
    {
        var hr = 101 + _rng.Next(0, 50); // 101-150 bpm
        return new EcgSignal
        {
            DataPoints = GenerateBeats(hr, stDeviation: 0, regular: true, pWave: true),
            HeartRateBpm = hr,
            PrIntervalMs = 130 + _rng.Next(-10, 11),
            QrsDurationMs = 85 + _rng.Next(-5, 6),
            QtIntervalMs = 320 + _rng.Next(-20, 21),
            StDeviationMv = 0.0,
            RhythmRegular = true,
            PwavePresent = true
        };
    }

    private EcgSignal GenerateBradycardia()
    {
        var hr = 40 + _rng.Next(0, 20); // 40-59 bpm
        return new EcgSignal
        {
            DataPoints = GenerateBeats(hr, stDeviation: 0, regular: true, pWave: true),
            HeartRateBpm = hr,
            PrIntervalMs = 180 + _rng.Next(-10, 11),
            QrsDurationMs = 100 + _rng.Next(-5, 6),
            QtIntervalMs = 440 + _rng.Next(-20, 21),
            StDeviationMv = 0.0,
            RhythmRegular = true,
            PwavePresent = true
        };
    }

    private EcgSignal GenerateAFib()
    {
        var hr = 80 + _rng.Next(0, 61); // 80-140 irregular
        return new EcgSignal
        {
            DataPoints = GenerateAfibBeats(hr),
            HeartRateBpm = hr,
            PrIntervalMs = 0, // no defined PR in AFib
            QrsDurationMs = 85 + _rng.Next(-5, 6),
            QtIntervalMs = 380 + _rng.Next(-30, 31),
            StDeviationMv = 0.0,
            RhythmRegular = false,
            PwavePresent = false
        };
    }

    private EcgSignal GenerateStElevation()
    {
        var hr = 70 + _rng.Next(0, 30);
        var stElev = 1.5 + _rng.NextDouble() * 2.5; // 1.5-4.0 mV elevation
        return new EcgSignal
        {
            DataPoints = GenerateBeats(hr, stDeviation: stElev, regular: true, pWave: true),
            HeartRateBpm = hr,
            PrIntervalMs = 160 + _rng.Next(-10, 11),
            QrsDurationMs = 110 + _rng.Next(0, 21),
            QtIntervalMs = 420 + _rng.Next(-20, 21),
            StDeviationMv = stElev,
            RhythmRegular = true,
            PwavePresent = true
        };
    }

    private EcgSignal GeneratePvc()
    {
        var hr = 65 + _rng.Next(0, 36);
        return new EcgSignal
        {
            DataPoints = GeneratePvcBeats(hr),
            HeartRateBpm = hr,
            PrIntervalMs = 160 + _rng.Next(-10, 11),
            QrsDurationMs = 140 + _rng.Next(0, 41), // wide QRS during PVC
            QtIntervalMs = 400 + _rng.Next(-20, 21),
            StDeviationMv = 0.0,
            RhythmRegular = false,
            PwavePresent = true
        };
    }

    // Gaussian pulse: amplitude * exp(-(t-center)^2 / (2*width^2))
    private static double Gaussian(double t, double center, double amplitude, double width)
        => amplitude * Math.Exp(-Math.Pow(t - center, 2) / (2 * width * width));

    private double[] GenerateBeats(double bpm, double stDeviation, bool regular, bool pWave)
    {
        var totalSamples = (int)(SampleRate * DurationSeconds);
        var signal = new double[totalSamples];
        var beatPeriod = 60.0 / bpm;

        var t = 0.0;
        while (t < DurationSeconds)
        {
            AddBeat(signal, t, beatPeriod, stDeviation, isWidePvc: false, hasPWave: pWave);
            t += beatPeriod + (_rng.NextDouble() - 0.5) * (regular ? 0.01 : 0.15);
        }

        AddNoise(signal);
        return signal;
    }

    private double[] GenerateAfibBeats(double avgBpm)
    {
        var totalSamples = (int)(SampleRate * DurationSeconds);
        var signal = new double[totalSamples];
        var avgPeriod = 60.0 / avgBpm;

        // Fibrillatory baseline
        for (var i = 0; i < totalSamples; i++)
        {
            var t = i / (double)SampleRate;
            signal[i] += 0.05 * Math.Sin(2 * Math.PI * 350 * t + _rng.NextDouble())
                       + 0.03 * Math.Sin(2 * Math.PI * 520 * t + _rng.NextDouble());
        }

        var time = 0.0;
        while (time < DurationSeconds)
        {
            AddBeat(signal, time, avgPeriod, 0, isWidePvc: false, hasPWave: false);
            var jitter = (_rng.NextDouble() - 0.2) * avgPeriod * 0.5;
            time += Math.Max(0.25, avgPeriod + jitter);
        }

        AddNoise(signal);
        return signal;
    }

    private double[] GeneratePvcBeats(double bpm)
    {
        var totalSamples = (int)(SampleRate * DurationSeconds);
        var signal = new double[totalSamples];
        var beatPeriod = 60.0 / bpm;

        var beatCount = 0;
        var t = 0.0;
        while (t < DurationSeconds)
        {
            var isPvc = beatCount > 0 && beatCount % 3 == 2; // every 3rd beat is PVC
            if (isPvc)
            {
                AddBeat(signal, t, beatPeriod, 0, isWidePvc: true, hasPWave: false);
                t += beatPeriod * 1.5; // compensatory pause
            }
            else
            {
                AddBeat(signal, t, beatPeriod, 0, isWidePvc: false, hasPWave: true);
                t += beatPeriod;
            }
            beatCount++;
        }

        AddNoise(signal);
        return signal;
    }

    private void AddBeat(double[] signal, double beatStart, double beatPeriod,
        double stDeviation, bool isWidePvc, bool hasPWave)
    {
        if (isWidePvc)
        {
            // Wide, bizarre QRS for PVC
            AddComponent(signal, beatStart + 0.18, amplitude: -0.5, width: 0.035);  // Q
            AddComponent(signal, beatStart + 0.21, amplitude: 1.8, width: 0.045);   // R (high)
            AddComponent(signal, beatStart + 0.26, amplitude: -0.8, width: 0.04);   // S
            AddComponent(signal, beatStart + 0.36, amplitude: -0.4, width: 0.06);   // inverted T
        }
        else
        {
            if (hasPWave)
                AddComponent(signal, beatStart + 0.08, amplitude: 0.15, width: 0.025); // P
            AddComponent(signal, beatStart + 0.17, amplitude: -0.1, width: 0.012);  // Q
            AddComponent(signal, beatStart + 0.20, amplitude: 1.0, width: 0.015);   // R
            AddComponent(signal, beatStart + 0.23, amplitude: -0.25, width: 0.013); // S
            // ST segment + T wave
            AddComponent(signal, beatStart + 0.36, amplitude: 0.2 + stDeviation * 0.3, width: 0.04); // T
            // Flat ST segment deviation
            if (Math.Abs(stDeviation) > 0.1)
            {
                var stStart = (int)((beatStart + 0.24) * SampleRate);
                var stEnd = (int)((beatStart + 0.34) * SampleRate);
                for (var i = stStart; i < stEnd && i < signal.Length; i++)
                    signal[i] += stDeviation * 0.4;
            }
        }
    }

    private static void AddComponent(double[] signal, double center, double amplitude, double width)
    {
        var startIdx = (int)Math.Max(0, (center - width * 4) * SampleRate);
        var endIdx = (int)Math.Min(signal.Length - 1, (center + width * 4) * SampleRate);
        for (var i = startIdx; i <= endIdx; i++)
        {
            var t = i / (double)SampleRate;
            signal[i] += Gaussian(t, center, amplitude, width);
        }
    }

    private void AddNoise(double[] signal)
    {
        for (var i = 0; i < signal.Length; i++)
            signal[i] += (_rng.NextDouble() - 0.5) * 0.03;
    }
}
