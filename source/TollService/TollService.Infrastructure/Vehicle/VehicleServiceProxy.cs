using System.Net.Http.Json;

namespace TollService.Infrastructure.Vehicle;

public interface IVehicleServiceProxy
{
    Task<Contracts.Vehicle?> GetVehicle(Guid id);
}
public class VehicleServiceProxy(HttpClient httpClient) : IVehicleServiceProxy
{
    public async Task<Contracts.Vehicle?> GetVehicle(Guid id)
    {
        var result = await httpClient.GetAsync($"vehicle/{id}");
        return await result.Content.ReadFromJsonAsync<Contracts.Vehicle>();
    }
}