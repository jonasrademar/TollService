using Microsoft.Extensions.Caching.Memory;
using TollService.Domain;

namespace TollService.Infrastructure.Holiday;

public class HolidayProvider(IHolidayServiceProxy proxy, IMemoryCache memoryCache) : IHolidayProvider
{
    private readonly MemoryCacheEntryOptions cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        SlidingExpiration = TimeSpan.FromMinutes(10)
    };

    public async Task<IEnumerable<DateOnly>> GetHolidays(int year)
    {
        var cacheKey = $"holidays_{year}";
        
        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<DateOnly>? cachedHolidays))
        {
            return cachedHolidays!;
        }

        var holidays = await proxy.GetHolidays(year);
        var holidayDates = holidays.Select(h => h.Date).ToList();
        
        
        memoryCache.Set(cacheKey, holidayDates, cacheOptions);
        return holidayDates;
    }
}
