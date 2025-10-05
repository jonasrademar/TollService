using AutoFixture;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using TollService.Domain.Models;
using TollService.Domain.Settings;

namespace TollService.Domain.UnitTests;

public class TollCalculatorTests : TestHelper.UnitTests
{
    private static readonly Mock<IHolidayProvider> HolidayProvider = new();
    private static readonly Mock<IVehiclePassRepository> VehiclePassRepository = new();
    private static readonly Mock<IIntervalConfigurationRepository> IntervalConfigurationRepository = new();
    private static readonly TollSettings TollSettings = new() {DailyCap = 100};

    private readonly TollCalculator subject = new(
        Options.Create(TollSettings),
        HolidayProvider.Object, 
        IntervalConfigurationRepository.Object, 
        VehiclePassRepository.Object);

    public TollCalculatorTests()
    {
        // Så att inte något intervall i sig själv överstiger dagstaket
        Fixture.Customize<TollInterval>(c => c.With(i => i.Fee, () => 5 + Fixture.Create<int>() % 20));
    }

    [Fact]
    public async Task GetTollFee_TollableVehicle_SinglePassNonHoliday_ReturnsExpectedToll()
    {
        var intervalConfiguration = Fixture.Create<IntervalConfiguration>();
        var vehicle = TollableVehicle();
        var (dateTime, expectedToll) = TollableDateTimeFromIntervalConfiguration(intervalConfiguration);

        var vehiclePass = Fixture.Build<VehiclePass>()
            .With(p => p.Timestamp, dateTime)
            .Create();

        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);
        HolidayProvider.Setup(p => p.GetHolidays(It.IsAny<int>())).ReturnsAsync([
            DateOnly.FromDateTime(dateTime.AddDays(1)),
            DateOnly.FromDateTime(dateTime.AddDays(-1))
        ]);
        VehiclePassRepository.Setup(r => r.GetPasses(vehicle.VehicleId, DateOnly.FromDateTime(dateTime)))
            .ReturnsAsync([vehiclePass]);

        var result = await subject.GetTollFeeAsync(vehicle, DateOnly.FromDateTime(dateTime));

        result.ShouldBe(expectedToll);
    }

    [Fact]
    public async Task GetTollFee_TollableVehicle_MultiplePasses_ReturnsExpectedToll()
    {
        var date = TollableDayOfTheWeek();
        var vehicle = TollableVehicle();
        var intervalConfiguration = Fixture.Build<IntervalConfiguration>()
            .With(c => c.Intervals, [
                    Fixture.Build<TollInterval>().With(i => i.Fee, 10).With(i => i.StartTime, new TimeOnly(10, 00)).Create(),
                    Fixture.Build<TollInterval>().With(i => i.Fee, 12).With(i => i.StartTime, new TimeOnly(12, 00)).Create(),
                    Fixture.Build<TollInterval>().With(i => i.Fee, 13).With(i => i.StartTime, new TimeOnly(14, 00)).Create(),
                    Fixture.Build<TollInterval>().With(i => i.Fee, 14).With(i => i.StartTime, new TimeOnly(16, 00)).Create()
                ])
            .Create();

        var vehiclePasses = intervalConfiguration.Intervals.Select(i => Fixture.Build<VehiclePass>()
            .With(p => p.Timestamp, date.ToDateTime(i.StartTime))
            .Create()); 
        
        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);
        HolidayProvider.Setup(p => p.GetHolidays(It.IsAny<int>())).ReturnsAsync([]);
        VehiclePassRepository.Setup(r => r.GetPasses(vehicle.VehicleId, date))
            .ReturnsAsync(vehiclePasses);


        var result = await subject.GetTollFeeAsync(vehicle, date);
        result.ShouldBe(49);
    }

    [Fact]
    public async Task GetTollFee_TollFreeVehicle_ReturnsZero()
    {
        var vehicle = TollFreeVehicle();
        var intervalConfiguration = Fixture.Create<IntervalConfiguration>();
        var (tollableDate, _) = TollableDateTimeFromIntervalConfiguration(intervalConfiguration);

        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);
        HolidayProvider.Setup(p => p.GetHolidays(It.IsAny<int>())).ReturnsAsync([]);
        VehiclePassRepository.Setup(r => r.GetPasses(vehicle.VehicleId, DateOnly.FromDateTime(tollableDate)))
            .ReturnsAsync([Fixture.Build<VehiclePass>()
                .With(p => p.Timestamp, tollableDate)
                .Create()]);

        var result = await subject.GetTollFeeAsync(vehicle, DateOnly.FromDateTime(tollableDate));
        result.ShouldBe(0);
    }

    [Fact]
    public async Task GetTollFee_Holiday_ReturnsZero()
    {
        var intervalConfiguration = Fixture.Create<IntervalConfiguration>();
        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);

        var (dateWithTollableTimeSlot, _) = TollableDateTimeFromIntervalConfiguration(intervalConfiguration);

        HolidayProvider.Setup(p => p.GetHolidays(dateWithTollableTimeSlot.Year))
            .ReturnsAsync([DateOnly.FromDateTime(dateWithTollableTimeSlot)]);

        var result = await subject.GetTollFeeAsync(TollableVehicle(), DateOnly.FromDateTime(dateWithTollableTimeSlot));
        result.ShouldBe(0);
    }

    // Osv. Vanligtvis mer komplett testtäckning men lite för hårdkodat för att orka i kodlabben 😵
    // Föreställ er att det finns massa fler goa tester här.

    private Vehicle TollableVehicle() =>
        Fixture.Build<Vehicle>()
            .With(v => v.Tollable, true)
            .Create();
    private Vehicle TollFreeVehicle() => 
        Fixture.Build<Vehicle>()
            .With(v => v.Tollable, false)
            .Create();

    private (DateTime dateTime, int expectedFee) TollableDateTimeFromIntervalConfiguration(IntervalConfiguration intervalConfiguration)
    {
        var date = TollableDayOfTheWeek();
        var randomInterval = intervalConfiguration.Intervals.OrderBy(_ => Guid.NewGuid()).First();
        var dateTime = new DateTime(date, randomInterval.StartTime);
        
        return (dateTime, randomInterval.Fee);
    }

    private DateOnly TollableDayOfTheWeek()
    {
        DateOnly date;
        do
        {
            date = DateOnly.FromDateTime(Fixture.Create<DateTime>());
        }
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);

        return date;

    }
}