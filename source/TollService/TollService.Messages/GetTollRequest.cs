namespace TollService.Messages;

public record GetTollRequest(Guid VehicleId, DateOnly Date);

public record GetTollResponse(int Toll);
public record GetTollResponseError(Exception Exception);