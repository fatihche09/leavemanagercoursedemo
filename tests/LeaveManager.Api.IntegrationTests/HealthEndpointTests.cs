using System.Diagnostics;
using System.Net;
using LeaveManager.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LeaveManager.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class HealthEndpointTests
{
    [Fact]
    public async Task Health_WhenDatabaseMigratedAndReachable_ReturnsHealthy()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = await PostgresApp.StartAsync();
        using var client = app.Api.CreateClient();

        var response = await client.GetAsync("/health", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(ct));
        using var scope = app.Api.Services.CreateScope();
        var applied = await scope.ServiceProvider.GetRequiredService<LeaveManagerDbContext>().Database.GetAppliedMigrationsAsync(ct);
        Assert.Contains(applied, m => m.EndsWith("_Initial", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Health_WhenDatabaseStopped_ReturnsUnhealthyWithin5sWithoutDetails()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = await PostgresApp.StartAsync();
        using var client = app.Api.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health", ct)).StatusCode);

        await app.Database.StopAsync(ct);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync("/health", ct);
        stopwatch.Stop();

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"Unhealthy took {stopwatch.Elapsed}.");
        var body = await response.Content.ReadAsStringAsync(ct);
        Assert.Equal("Unhealthy", body);
        Assert.DoesNotContain("Host=", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Npgsql", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(" at ", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Health_WhenDatabaseHangs_ReturnsUnhealthyWithin5s()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = await PostgresApp.StartAsync();
        using var client = app.Api.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health", ct)).StatusCode);

        // A paused server accepts nothing and answers nothing: the network-partition case,
        // unlike a stopped one, which refuses connections at once.
        await app.Database.PauseAsync(ct);
        var stopwatch = Stopwatch.StartNew();
        var response = await client.GetAsync("/health", ct);
        stopwatch.Stop();
        await app.Database.UnpauseAsync(ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"Unhealthy took {stopwatch.Elapsed}.");
        Assert.Equal("Unhealthy", await response.Content.ReadAsStringAsync(ct));
    }

    [Fact]
    public async Task Health_UnderConcurrentRequests_KeepsAtMostTwoProbeConnections()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var app = await PostgresApp.StartAsync();
        using var client = app.Api.CreateClient();

        var responses = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => client.GetAsync("/health", ct)));

        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        // The probe has its own small pool: its idle connections stay open, and never more than two.
        await using var connection = new NpgsqlConnection(app.Database.GetConnectionString());
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(
            "SELECT count(*) FROM pg_stat_activity WHERE application_name = 'health-probe'", connection);
        var probeConnections = (long)(await command.ExecuteScalarAsync(ct))!;
        Assert.InRange(probeConnections, 1, 2);
    }
}
