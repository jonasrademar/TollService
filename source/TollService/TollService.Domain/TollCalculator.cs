using System.Collections.Immutable;
using Microsoft.Extensions.Options;
using TollService.Domain.Models;
using TollService.Domain.Settings;

namespace TollService.Domain;

public interface ITollCalculator
{
    Task<int> GetTollFeeAsync(Vehicle vehicle, DateOnly date);
}

public class TollCalculator(
    IOptions<TollSettings> tollSettings,
    IHolidayProvider holidayProvider,
    IIntervalConfigurationRepository intervalConfigurationRepository,
    IVehiclePassRepository vehiclePassRepository) : ITollCalculator
{
    /**
 * Calculate the total toll fee for one day
 *
 * @param vehicle - the vehicle
 * @param date    - the date to calculate toll for
 * @return - the total toll fee for that day
 */
    private IntervalConfiguration IntervalConfiguration { get; set; } = null!;

    private int DailyCap => tollSettings.Value.DailyCap;

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
        // WIP flytta till rätt ställe vid refact
        IntervalConfiguration = await intervalConfigurationRepository.GetLatestConfiguration();
        DateTime intervalStart = dates[0];
        int totalFee = 0;
        foreach (DateTime date in dates)
        {
            int nextFee = await GetTollFee(date, vehicle);
            int tempFee = await GetTollFee(intervalStart, vehicle);

            long diffInMillies = date.Millisecond - intervalStart.Millisecond;
            long minutes = diffInMillies/1000/60;

            if (minutes <= DailyCap)
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
        if (totalFee > DailyCap) totalFee = DailyCap;
        return totalFee;
    }

    public async Task<int> GetTollFee(DateTime date, Vehicle vehicle)
    {
        if (await IsTollFreeDate(date)) return 0;

        var timeOfDay = TimeOnly.FromDateTime(date);
        
        return GetTollFeeForTime(timeOfDay);
    }

    private int GetTollFeeForTime(TimeOnly time)
    {
        var matchingInterval = IntervalConfiguration.Intervals
            .OrderBy(x => x.StartTime)
            .LastOrDefault(x => time >= x.StartTime);
            
        return matchingInterval?.Fee ?? 0;
    }

    private async Task<bool> IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

        var holidays = await holidayProvider.GetHolidays(date.Year);
        return holidays.Contains(DateOnly.FromDateTime(date));
    }
}