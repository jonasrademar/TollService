namespace TollService.Infrastructure.Vehicle.Contracts;

public sealed class Vehicle
{
    public Guid Id { get; set; }
    public VehicleType VehicleType { get; set; } = null!;
    public VehicleClassification VehicleClassification { get; set; } = null!;
    public bool IsTollable => VehicleType.Tollable && VehicleClassification.Tollable;
}

public class VehicleType
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public bool Tollable { get; set; }
}

public class VehicleClassification
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public bool Tollable { get; set; }
}