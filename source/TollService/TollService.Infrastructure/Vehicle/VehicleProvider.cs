using TollService.Domain;

namespace TollService.Infrastructure.Vehicle;
public class VehicleProvider(IVehicleServiceProxy proxy) : IIVehicleProvider
{
    public async Task<Domain.Vehicle> GetVehicle(Guid id)
    {
        var vehicle = await proxy.GetVehicle(id);

        // Motsvarar originalkodens IsTollFreeVehicle : "if (vehicle == null) return false;"
        return new Domain.Vehicle(id, vehicle?.IsTollable ?? true);
    }
}