using System.Collections.Immutable;

namespace TollService.Domain;

public interface ITollCalculator
{
    Task<int> GetTollFeeAsync(Vehicle vehicle, DateOnly date);
}

public class TollCalculator(
    IHolidayProvider holidayProvider,
    IVehiclePassRepository vehiclePassRepository) : ITollCalculator
{
    /**
 * Calculate the total toll fee for one day
 *
 * @param vehicle - the vehicle
 * @param date    - the date to calculate toll for
 * @return - the total toll fee for that day
 */

    public async Task<int> GetTollFeeAsync(Vehicle vehicle, DateOnly date)
    {
        if (!vehicle.Tollable)
            return 0;

        var vehiclePasses = await vehiclePassRepository.GetPasses(vehicle.VehicleId, date);
        var dates = vehiclePasses.Select(p => p.Timestamp).ToImmutableList();
        
        return await GetTollFee(vehicle, dates);
    }

    public async Task<int> GetTollFee(Vehicle vehicle, IReadOnlyList<DateTime> dates)
    {
        DateTime intervalStart = dates[0];
        int totalFee = 0;
        foreach (DateTime date in dates)
        {
            int nextFee = await GetTollFee(date, vehicle);
            int tempFee = await GetTollFee(intervalStart, vehicle);

            long diffInMillies = date.Millisecond - intervalStart.Millisecond;
            long minutes = diffInMillies/1000/60;

            if (minutes <= 60)
            {
                if (totalFee > 0) totalFee -= tempFee;
                if (nextFee >= tempFee) tempFee = nextFee;
                totalFee += tempFee;
            }
            else
            {
                totalFee += nextFee;
            }
        }
        if (totalFee > 60) totalFee = 60;
        return totalFee;
    }

    public async Task<int> GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (await IsTollFreeDate(date)) return 0;

        var timeOfDay = TimeOnly.FromDateTime(date);
        
        return GetTollFeeForTime(timeOfDay);
    }

    private static int GetTollFeeForTime(TimeOnly time)
    {
        // Det här borde egentligen också ligga i en service och gå att underhålla, snarare än hårdkodat.
        var tollIntervals = new[]
        {
            new TollInterval(new TimeOnly(06, 00), 8),
            new TollInterval(new TimeOnly(06, 30), 13),
            new TollInterval(new TimeOnly(07, 00), 18),
            new TollInterval(new TimeOnly(08, 00), 13),
            new TollInterval(new TimeOnly(08, 30), 8),
            new TollInterval(new TimeOnly(15, 00), 13),
            new TollInterval(new TimeOnly(15, 30), 18),
            new TollInterval(new TimeOnly(17, 00), 13),
            new TollInterval(new TimeOnly(18, 00), 8),
            new TollInterval(new TimeOnly(18, 30), 0)
        };

        var matchingInterval = tollIntervals
            .OrderBy(x => x.StartTime)
            .LastOrDefault(x => time >= x.StartTime);
            
        return matchingInterval?.Fee ?? 0;
    }

    private record TollInterval(TimeOnly StartTime, int Fee);

    private async Task<bool> IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        var holidays = await holidayProvider.GetHolidays(date.Year);
        return holidays.Contains(DateOnly.FromDateTime(date));
    }
}