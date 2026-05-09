# EKG Monitor — Setup

## Krav
- .NET 10 SDK
- Node.js 22 + Angular CLI 18
- PostgreSQL (kørende på localhost:5432)

## 1. Anthropic API-nøgle
Åbn `backend/EcgMonitor.API/appsettings.json` og indsæt din nøgle:
```json
"Anthropic": { "ApiKey": "sk-ant-..." }
```

## 2. Database
Opret en PostgreSQL-database:
```sql
CREATE DATABASE ecgmonitor;
```
Tilpas connection string i `appsettings.json` hvis brugernavn/password afviger fra `postgres/postgres`.

## 3. Start backend
```bash
cd backend/EcgMonitor.API
dotnet run
# Kører på http://localhost:5000
# Migrationer køres automatisk ved opstart
```

## 4. Start frontend
```bash
cd frontend
ng serve
# Kører på http://localhost:4200
```

## Sådan virker systemet
- Baggrundstjenesten genererer et nyt syntetisk EKG-signal hvert **20. sekund**
- Claude (`claude-sonnet-4-6`) analyserer EKG-egenskaberne og returnerer diagnose, konfidans og prioritet
- Kun anomalier (AFib, takyakardi, bradykardi, ST-elevation, PVC) gemmes i databasen
- Lægen kan se en liste over afventende anomalier, se EKG-kurven og indsende sit eget review

## API-endepunkter
| Metode | URL | Beskrivelse |
|--------|-----|-------------|
| GET | /api/ecg/stats | Dashboard statistik |
| GET | /api/ecg | Liste af anomalier (filter: status, urgency) |
| GET | /api/ecg/{id} | Enkelt EKG med AI-diagnose og kurvedata |
| POST | /api/ecg/{id}/review | Indsend læge-review |
