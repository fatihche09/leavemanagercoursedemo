using System.Diagnostics;
using System.Net;
using LeaveManager.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
}
