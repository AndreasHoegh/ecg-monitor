# EKG Monitor

Et medicinsk overvågningssystem der automatisk genererer EKG-signaler, analyserer dem med AI og præsenterer anomalier til læge-review.

## Arkitektur

```
┌─────────────────────────────────────────────────────────┐
│  Angular Frontend (port 4200)                           │
│  Dashboard · Anomali-liste · Normal-liste · EKG-kurve   │
└────────────────────┬────────────────────────────────────┘
                     │ HTTP REST
┌────────────────────▼────────────────────────────────────┐
│  .NET 10 Web API (port 5000)                            │
│  ┌──────────────────┐  ┌────────────────────────────┐  │
│  │ EcgIngestionWorker│  │ AI Analysis Service        │  │
│  │ (hvert 20. sek.) │→ │ (lokal signalklassifikation│  │
│  └──────────────────┘  └────────────────────────────┘  │
└────────────────────┬────────────────────────────────────┘
                     │ EF Core / Npgsql
┌────────────────────▼────────────────────────────────────┐
│  PostgreSQL Database                                    │
│  EcgRecords · DoctorReviews                             │
└─────────────────────────────────────────────────────────┘
```

## Features

- **Automatisk EKG-generering** — syntetiske signaler med realistiske PQRST-morfologier hvert 20. sekund
- **AI-diagnose** — klassificerer 5 anomali-typer med konfidansscore og urgency-niveau
- **Anomali-typer**: Sinustakyakardi, Sinusbradykardi, Atrieflimren (AFib), ST-elevation (STEMI), Ventrikulære ekstraslag (PVC)
- **Læge-review** — second opinion workflow hvor lægen kan godkende/afvise AI-diagnosen
- **Dashboard** — live statistik med auto-refresh, kritiske sager og diagnoser-fordeling
- **EKG-visualisering** — interaktiv kurve med Chart.js

## Tech Stack

| Lag | Teknologi |
|-----|-----------|
| Frontend | Angular 18, Chart.js, SCSS |
| Backend | .NET 10, ASP.NET Core Web API |
| Database | PostgreSQL 17/18, Entity Framework Core 8 |
| ORM | Npgsql.EntityFrameworkCore.PostgreSQL |

## Kom i gang

### Krav

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) og Angular CLI 18 (`npm install -g @angular/cli@18`)
- [PostgreSQL](https://www.postgresql.org/download/) (kørende lokalt)

### 1. Database

Opret databasen i psql eller pgAdmin:

```sql
CREATE DATABASE ecgmonitor;
```

### 2. Konfiguration

Rediger `backend/EcgMonitor.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=ecgmonitor;Username=postgres;Password=DIN_ADGANGSKODE"
  }
}
```

> Tabeller oprettes automatisk ved første opstart via EF Core migrations.

### 3. Start backend

```bash
cd backend/EcgMonitor.API
dotnet run --no-launch-profile
```

Backend kører på `http://localhost:5000`. Du ser i terminalen når EKG-data genereres og gemmes.

### 4. Start frontend

```bash
cd frontend
ng serve --open
```

Frontend kører på `http://localhost:4200` og åbner automatisk i browseren.

## API-endepunkter

| Metode | Sti | Beskrivelse |
|--------|-----|-------------|
| `GET` | `/api/ecg/stats` | Dashboard-statistik |
| `GET` | `/api/ecg` | Liste af EKG-optagelser (filter: `status`, `urgency`, `page`) |
| `GET` | `/api/ecg/{id}` | Enkelt optagelse med kurvedata og AI-diagnose |
| `POST` | `/api/ecg/{id}/review` | Indsend læge-review |

### Eksempel: Hent stats

```bash
curl http://localhost:5000/api/ecg/stats
```

```json
{
  "totalAnomalies": 12,
  "pendingReviews": 8,
  "reviewedToday": 4,
  "diagnosisCounts": {
    "Atrieflimren (AFib)": 3,
    "Sinustakyakardi": 5
  }
}
```

### Eksempel: Indsend review

```bash
curl -X POST http://localhost:5000/api/ecg/{id}/review \
  -H "Content-Type: application/json" \
  -d '{
    "doctorName": "Dr. Jensen",
    "diagnosis": "Atrieflimren (AFib)",
    "notes": "Bekræftet — anbefaler kardiologisk vurdering",
    "agreedWithAi": true
  }'
```

## Projektstruktur

```
ecg-monitor/
├── backend/
│   └── EcgMonitor.API/
│       ├── Background/        # EcgIngestionWorker (baggrundstjeneste)
│       ├── Controllers/       # EcgController, ReviewController
│       ├── Data/              # AppDbContext (EF Core)
│       ├── Migrations/        # Database-migrationer
│       ├── Models/            # EcgRecord, DoctorReview, DTOs
│       ├── Services/          # EcgGeneratorService, AiAnalysisService
│       └── Program.cs
└── frontend/
    └── src/app/
        ├── dashboard/         # Statistik-dashboard
        ├── ecg/               # Liste og detaljeside
        └── services/          # EcgService (HTTP-klient)
```

## Udvidelsesmuligheder

- Skift den lokale AI-klassifikation ud med [Claude API](https://console.anthropic.com) for ægte AI-diagnose
- Tilføj brugerautentifikation til læge-reviewet
- Kobl til rigtige EKG-enheder via HL7 FHIR eller proprietære APIs
- Tilføj SignalR for real-time push i stedet for polling
