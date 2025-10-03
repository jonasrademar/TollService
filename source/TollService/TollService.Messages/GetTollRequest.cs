namespace TollService.Messages;

public record GetTollRequest(Guid VehicleId, DateTime[] Dates);

public record GetTollResponse(int Toll);