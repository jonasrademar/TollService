namespace TollService.Domain;

public interface IVehiclePassRepository
{
    Task<IEnumerable<VehiclePass>> GetPasses(Guid vehicleId, DateOnly date);
    Task AddVehiclePass(Guid passId, Guid vehicleId, DateTime timestamp);
}

public class VehiclePass(Guid passId, Guid vehicleId, DateTime timestamp)
{
    public Guid PassId { get; set; } = passId;
    public Guid VehicleId { get; set; } = vehicleId;
    public DateTime Timestamp { get; set; } = timestamp;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}