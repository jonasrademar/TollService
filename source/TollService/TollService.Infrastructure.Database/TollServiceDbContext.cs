using Microsoft.EntityFrameworkCore;
using TollService.Domain;

namespace TollService.Infrastructure.Database;

public class TollServiceDbContext(DbContextOptions<TollServiceDbContext> options) : DbContext(options)
{
    public DbSet<VehiclePass> VehiclePasses { get; protected set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TollServiceDbContext).Assembly);
    }
}