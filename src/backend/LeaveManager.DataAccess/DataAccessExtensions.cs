using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LeaveManager.DataAccess;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        // Deliberate app-wide defaults, overriding the connection string: fail fast on an unreachable
        // or hung server instead of Npgsql's 15 s connect wait (which also bounds waiting for a pool
        // slot) and 2 s cancellation wait (docs/conventions.md, Known traps).
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Timeout = 3,
            CancellationTimeout = 500,
        };
        return services.AddDbContext<LeaveManagerDbContext>(options => options.UseNpgsql(builder.ConnectionString));
    }

    // Health probe on its own pool of at most two connections, so an anonymous request flood cannot
    // exhaust the server's connections. On a hung server Npgsql ignores the token and waits out its
    // timeouts. Measured worst case ~3 s: 1 s command timeout plus ~2 x Timeout for the cancel request
    // (CancellationTimeout = -1 does not skip it); up to ~4 s when also waiting 1 s for a pool slot.
    // Raising Timeout to 2 s already breaks the 5 s bound of AC-2 (docs/conventions.md, Known traps).
    public static async Task<bool> CanConnectQuicklyAsync(this LeaveManagerDbContext db, CancellationToken ct)
    {
        var builder = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString())
        {
            MaxPoolSize = 2,
            ApplicationName = "health-probe",
            Timeout = 1,
            CommandTimeout = 1,
            CancellationTimeout = -1,
        };
        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        try
        {
            await connection.OpenAsync(ct);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(ct);
            return true;
        }
        catch (Exception e) when (e is NpgsqlException or TimeoutException or OperationCanceledException)
        {
            return false;
        }
    }

    public static void MigrateDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<LeaveManagerDbContext>().Database.Migrate();
    }
}
