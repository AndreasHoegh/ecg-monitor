using EcgMonitor.API.Background;
using EcgMonitor.API.Data;
using EcgMonitor.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Application services
builder.Services.AddSingleton<EcgGeneratorService>();
builder.Services.AddScoped<AiAnalysisService>();
builder.Services.AddHostedService<EcgIngestionWorker>();

// CORS for Angular dev server
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:4200", "http://localhost:4201", "http://localhost:4202", "http://localhost:61440")
        .AllowAnyMethod()
        .AllowAnyHeader()));

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.MapControllers();
app.Run();
