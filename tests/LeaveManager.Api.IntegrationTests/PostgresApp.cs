using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace LeaveManager.Api.IntegrationTests;

// One fresh PostgreSQL container and one API instance per test: no data reaches the next test or run.
// Disposing stops the API and removes the container; Ryuk reaps it if the run is killed.
public sealed class PostgresApp : IAsyncDisposable
{
    private PostgresApp(PostgreSqlContainer database, WebApplicationFactory<Program> api)
    {
        Database = database;
        Api = api;
    }

    public PostgreSqlContainer Database { get; }

    public WebApplicationFactory<Program> Api { get; }

    public static async Task<PostgresApp> StartAsync()
    {
        var database = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await database.StartAsync(TestContext.Current.CancellationToken);

        var api = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:LeaveManager", database.GetConnectionString()));

        return new PostgresApp(database, api);
    }

    public async ValueTask DisposeAsync()
    {
        await Api.DisposeAsync();
        await Database.DisposeAsync();
    }
}
