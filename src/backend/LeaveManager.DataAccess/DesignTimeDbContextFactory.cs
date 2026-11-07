using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LeaveManager.DataAccess;

// Used only by `dotnet ef` to build migrations; it never opens a connection, so no real credentials.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LeaveManagerDbContext>
{
    public LeaveManagerDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<LeaveManagerDbContext>().UseNpgsql("Host=localhost;Database=leavemanager").Options);
}
