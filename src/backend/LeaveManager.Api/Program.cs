using LeaveManager.DataAccess;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LeaveManager")
    ?? throw new InvalidOperationException("Connection string 'LeaveManager' is not configured (user-secrets or environment).");
builder.Services.AddDataAccess(connectionString);

// Health reports only Healthy/Unhealthy (no details) and gives up on the database after 5 s.
builder.Services.AddHealthChecks().AddDbContextCheck<LeaveManagerDbContext>("database");
builder.Services.Configure<HealthCheckServiceOptions>(options =>
{
    foreach (var registration in options.Registrations)
    {
        registration.Timeout = TimeSpan.FromSeconds(5);
    }
});

var app = builder.Build();

app.Services.MigrateDatabase();

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
