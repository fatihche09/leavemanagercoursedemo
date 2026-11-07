using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManager.DataAccess;

public static class DataAccessExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString) =>
        services.AddDbContext<LeaveManagerDbContext>(options => options.UseNpgsql(connectionString));

    public static void MigrateDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider.GetRequiredService<LeaveManagerDbContext>().Database.Migrate();
    }
}
