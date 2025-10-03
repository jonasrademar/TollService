using Microsoft.EntityFrameworkCore;
using TollService.Domain;

namespace TollService.Infrastructure.Database;

public class TollServiceDbContext(DbContextOptions<TollServiceDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles { get; protected set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TollServiceDbContext).Assembly);
    }
}