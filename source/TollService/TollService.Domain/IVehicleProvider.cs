namespace TollService.Domain;

// Bör inte vara TollService ansvar att underhålla ett fordonsregister. Hurvida det sker en
// Integration mot transportstyrelsen eller om man underhåller all konfiguration själv, så
// sköts det i VehicleService som är en annan tjänst.
public interface IIVehicleProvider
{
    public Task<Vehicle> GetVehicle(Guid id);
}
public record Vehicle(Guid VehicleId, bool Tollable);