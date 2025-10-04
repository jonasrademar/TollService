using System.Net.Http.Json;

namespace TollService.Infrastructure.Holiday;

public interface IHolidayServiceProxy
{
    Task<IEnumerable<Contracts.Holiday>> GetHolidays(int year);
}

public class HolidayServiceProxy(HttpClient httpClient) : IHolidayServiceProxy
{
    public async Task<IEnumerable<Contracts.Holiday>> GetHolidays(int year)
    {
        var result = await httpClient.GetAsync($"holidays/{year}");
        result.EnsureSuccessStatusCode();
        return await result.Content.ReadFromJsonAsync<IEnumerable<Contracts.Holiday>>() ?? [];
    }
}
