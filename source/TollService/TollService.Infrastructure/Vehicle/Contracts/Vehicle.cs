namespace TollService.Infrastructure.Vehicle.Contracts;

public record Vehicle(
    Guid Id,
    VehicleType VehicleType,
    IEnumerable<VehicleClassification> VehicleClassifications)
{
    public bool IsTollable => VehicleType.Tollable && VehicleClassifications.Any(c => c.Tollable);
}

// Här tänker jag tex "Motorbike", "Tractor" osv
public record VehicleType(Guid Id, string Description, bool Tollable);

// Här tänker jag tex "Diplomat", "Foreign", "Military" osv
public record VehicleClassification(Guid Id, string Description, bool Tollable); 