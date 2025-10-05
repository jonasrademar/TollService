using TollService.Domain.Models;

namespace TollService.Domain;

public interface IVehiclePassRepository
{
    Task<IEnumerable<VehiclePass>> GetPasses(Guid vehicleId, DateOnly date);
    Task AddVehiclePass(Guid passId, Guid vehicleId, DateTime timestamp);
}