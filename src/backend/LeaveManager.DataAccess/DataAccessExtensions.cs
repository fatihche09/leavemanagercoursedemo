using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LeaveManager.DataAccess;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        // An unreachable or hung server must fail fast: Npgsql's defaults wait 15 s to connect and,
        // once cancelled, 2 s more for the server to acknowledge (docs/conventions.md, Known traps).
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Timeout = 3,
            CancellationTimeout = 500,
        };
        return services.AddDbContext<LeaveManagerDbContext>(options => options.UseNpgsql(builder.ConnectionString));
    }

    // Health probe on its own short-lived, unpooled connection. On a hung server Npgsql ignores the
    // token and waits out its timeouts, and an abandoned pooled query (30 s command timeout) holds the
    // request open; tight timeouts here keep the probe at ~3 s worst case (docs/conventions.md, Known traps).
    public static async Task<bool> CanConnectQuicklyAsync(this LeaveManagerDbContext db, CancellationToken ct)
    {
        var builder = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString())
        {
            Pooling = false,
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
