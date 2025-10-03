namespace TollService.Domain;

public interface IVehicleRepository
{
    Task<Vehicle?> GetVehicle(Guid id);
}