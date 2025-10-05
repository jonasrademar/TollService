using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
    private int DailyCap => tollSettings.Value.DailyCap;

    public async Task<int> GetTollFeeAsync(Vehicle vehicle, DateOnly date)
    {
        if (!vehicle.Tollable || await IsTollFreeDate(date))
            return 0;

        var vehiclePasses = await vehiclePassRepository.GetPasses(vehicle.VehicleId, date);
        
        return await GetDailyTollFee(vehiclePasses.ToImmutableList());
    }

    private async Task<int> GetDailyTollFee(IReadOnlyList<VehiclePass> passes)
    {
        var intervalConfiguration = await intervalConfigurationRepository.GetLatestConfiguration();
        var totalFee = 0;

        TimeOnly? intervalStart = null;
        var tollForInterval = 0;

        foreach (var pass in passes.OrderBy(p => p.Timestamp))
        {
            var timeForPass = new TimeOnly(pass.Timestamp.Hour, pass.Timestamp.Minute);
            if (intervalStart == null || intervalStart.Value.AddHours(1) < timeForPass)
            {
                totalFee += tollForInterval;

                intervalStart = timeForPass;
                tollForInterval = 0;
            }

            var nextFee = GetTollFeeForDateTime(intervalConfiguration, pass.Timestamp);
            tollForInterval = Math.Max(nextFee, tollForInterval);

            if ((totalFee + tollForInterval) >= DailyCap) return DailyCap;
        }
        return Math.Min(totalFee + tollForInterval, DailyCap);
    }

    private int GetTollFeeForDateTime(IntervalConfiguration intervalConfiguration, DateTime date)
    {
        var matchingInterval = intervalConfiguration.Intervals
            .OrderBy(x => x.StartTime)
            .LastOrDefault(x => date.Hour >= x.StartTime.Hour &&
                                date.Minute >= x.StartTime.Minute);

        return matchingInterval?.Fee ?? 0;
    }

    private async Task<bool> IsTollFreeDate(DateOnly date)
    {
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return true;

        var holidays = await holidayProvider.GetHolidays(date.Year);
        return holidays.Contains(date);
    }
}