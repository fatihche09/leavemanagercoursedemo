using LeaveManager.DataAccess;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("LeaveManager")
    ?? throw new InvalidOperationException("Connection string 'LeaveManager' is not configured (user-secrets or environment).");
builder.Services.AddDataAccess(connectionString);

// Health reports only Healthy/Unhealthy (no details). The probe honours the check's timeout even
// when the database hangs, so the response stays under 5 s.
builder.Services.AddHealthChecks().AddDbContextCheck<LeaveManagerDbContext>(
    "database",
    customTestQuery: (db, ct) => db.CanConnectQuicklyAsync(ct));

// PostConfigure runs after every AddHealthChecks registration, so a check added later still
// gets the timeout unless it sets its own.
builder.Services.PostConfigure<HealthCheckServiceOptions>(options =>
{
    foreach (var registration in options.Registrations.Where(r => r.Timeout == Timeout.InfiniteTimeSpan))
    {
        registration.Timeout = TimeSpan.FromSeconds(4);
    }
});

var app = builder.Build();

app.Services.MigrateDatabase();

app.MapHealthChecks("/health");

app.Run();

public partial class Program;
