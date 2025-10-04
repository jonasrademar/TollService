using TollService.Domain;

namespace TollService.Infrastructure.Vehicle;
public class VehicleProvider(IVehicleServiceProxy proxy) : IIVehicleProvider
{
    public async Task<Domain.Vehicle> GetVehicle(Guid id)
    {
        var vehicle = await proxy.GetVehicle(id);

        return new Domain.Vehicle(id, vehicle?.IsTollable ?? true);
    }
}