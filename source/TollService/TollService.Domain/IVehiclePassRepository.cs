namespace TollService.Domain;

public interface IVehiclePassRepository
{
    Task<IEnumerable<VehiclePass>> GetPasses(Guid vehicleId, DateOnly date);
    void AddVehiclePass(Guid passId, Guid vehicleId, DateTimeOffset timestamp);
}

public class VehiclePass(Guid passId, Guid vehicleId, DateTimeOffset timestamp)
{
    public Guid PassId { get; set; } = passId;
    public Guid VehicleId { get; set; } = vehicleId;
    public DateTimeOffset Timestamp { get; set; } = timestamp;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}