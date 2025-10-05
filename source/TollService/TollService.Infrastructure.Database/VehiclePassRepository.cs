using Microsoft.EntityFrameworkCore;
using TollService.Domain;
using TollService.Domain.Models;

namespace TollService.Infrastructure.Database;

public class VehiclePassRepository(TollServiceDbContext dbContext) : IVehiclePassRepository
{
    public async Task<IEnumerable<VehiclePass>> GetPasses(Guid vehicleId, DateOnly date)
        => await dbContext.VehiclePasses.Where(p => 
                p.VehicleId == vehicleId &&
                p.Timestamp <= date.ToDateTime(TimeOnly.MaxValue) &&
                p.Timestamp >= date.ToDateTime(TimeOnly.MinValue))
            .ToListAsync();

    public async Task AddVehiclePass(Guid passId, Guid vehicleId, DateTime timestamp)
    {
        dbContext.VehiclePasses.Add(new VehiclePass(passId, vehicleId, timestamp));
        await dbContext.SaveChangesAsync();
    }
}