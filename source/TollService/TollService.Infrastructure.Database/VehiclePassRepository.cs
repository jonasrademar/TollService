using Microsoft.EntityFrameworkCore;
using TollService.Domain;

namespace TollService.Infrastructure.Database;

public class VehiclePassRepository(TollServiceDbContext dbContext) : IVehiclePassRepository
{
    public async Task<IEnumerable<VehiclePass>> GetPasses(Guid vehicleId, DateOnly date)
        => await dbContext.VehiclePasses.Where(p => 
                p.VehicleId == vehicleId &&
                p.Timestamp <= date.ToDateTime(TimeOnly.MaxValue) &&
                p.Timestamp >= date.ToDateTime(TimeOnly.MinValue))
            .ToListAsync();

    public void AddVehiclePass(Guid passId, Guid vehicleId, DateTimeOffset timestamp)
        => dbContext.VehiclePasses.Add(new VehiclePass(passId, vehicleId, timestamp));
}