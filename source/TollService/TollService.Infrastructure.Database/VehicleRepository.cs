using Microsoft.EntityFrameworkCore;
using TollService.Domain;

namespace TollService.Infrastructure.Database;

public class VehicleRepository(TollServiceDbContext dbContext) : IVehicleRepository
{
    public async Task<Vehicle?> GetVehicle(Guid id) 
        => await dbContext.Vehicles.SingleOrDefaultAsync(v => v.Id == id);
}