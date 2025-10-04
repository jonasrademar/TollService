namespace TollService.Domain;

public interface IIVehicleProvider
{
    public Task<Vehicle> GetVehicle(Guid id);
}