namespace TollService.Messages;

public record VehiclePassRegistrationMessage(Guid PassId, Guid VehicleId, DateTime Timestamp);