using Microsoft.EntityFrameworkCore;

namespace LeaveManager.DataAccess;

public class LeaveManagerDbContext(DbContextOptions<LeaveManagerDbContext> options) : DbContext(options);
