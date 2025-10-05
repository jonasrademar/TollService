using AutoFixture;
using Shouldly;
using System.Globalization;
using Microsoft.Extensions.Options;
using Moq;
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
        // Så inte något intervall i sig själv överstiger dagstaket
        Fixture.Customize<TollInterval>(c => c.With(i => i.Fee, () => 5 + Fixture.Create<int>() % 20));
    }

    [Theory, 
     InlineData(8, "2013-01-02 06:00"), 
     InlineData(13, "2013-01-02 06:45"), 
     InlineData(18, "2013-01-02 07:30")]
    public async Task GetTollFee_TollableVehicle_SinglePass_ReturnsExpectedToll(int expectedToll, DateTime dateTime)
    {
        var result = await subject.GetTollFee(dateTime, TollableVehicle());
        result.ShouldBe(expectedToll);
    }

    // Nedan tester blandat röda pga bugg; long diffInMillies = date.Millisecond - intervalStart.Millisecond avser inte göra vad man tror det gör.
    [Theory,
     InlineData(18, new[] { "2013-01-02 06:15", "2013-01-02 07:00" }),
     InlineData(21, new[] { "2013-01-02 06:15", "2013-01-03 07:00" }),
     InlineData(21, new[] { "2013-01-02 06:00", "2013-01-02 07:30" }),
     InlineData(36, new[] { "2013-01-02 07:30", "2013-01-02 16:30" }),
     InlineData(39, new[] { "2013-01-02 06:00", "2013-01-02 07:15", "2013-01-02 08:20" }),
     InlineData(60, new[] { "2013-01-02 06:35", "2013-01-02 07:40", "2013-01-02 15:10", "2013-01-02 16:15" })]
    public async Task GetTollFee_TollableVehicle_MultiplePasses_ReturnsExpectedToll(int expectedToll, string[] dateTimes)
    {
        var parsedDates = dateTimes
            .Select(s => DateTime.ParseExact(s, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))
            .ToList();

        var result = await subject.GetTollFee(TollableVehicle(), parsedDates);
        result.ShouldBe(expectedToll);
    }

    [Fact]
    public async Task GetTollFee_TollFreeVehicle_ReturnsZero()
    {
        var intervalConfiguration = Fixture.Create<IntervalConfiguration>();
        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);

        var (tollableDate, _) = TollableDateTimeFromIntervalConfiguration(intervalConfiguration);

        var result = await subject.GetTollFee(TollFreeVehicle(), [tollableDate]);
        result.ShouldBe(0);
    }

    [Fact]
    public async Task GetTollFee_NullVehicle_Tollable()
    {
        var intervalConfiguration = Fixture.Create<IntervalConfiguration>();
        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);

        var (tollableDate, expectedFee) = TollableDateTimeFromIntervalConfiguration(intervalConfiguration);

        var result = await subject.GetTollFee(null!, [tollableDate]);
        result.ShouldBe(expectedFee);
    }

    [Fact]
    public async Task GetTollFee_Holiday_ReturnsZero()
    {
        var intervalConfiguration = Fixture.Create<IntervalConfiguration>();
        IntervalConfigurationRepository.Setup(r => r.GetLatestConfiguration()).ReturnsAsync(intervalConfiguration);

        var (dateWithTollableTimeSlot, _) = TollableDateTimeFromIntervalConfiguration(intervalConfiguration);

        HolidayProvider.Setup(p => p.GetHolidays(dateWithTollableTimeSlot.Year))
            .ReturnsAsync([DateOnly.FromDateTime(dateWithTollableTimeSlot)]);

        var result = await subject.GetTollFee(TollableVehicle(), [dateWithTollableTimeSlot]);
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
        var dateTime = Fixture.Create<DateTime>().Date;
        var randomInterval = intervalConfiguration.Intervals.OrderBy(_ => Guid.NewGuid()).First();
        dateTime = dateTime.AddHours(randomInterval.StartTime.Hour).AddMinutes(randomInterval.StartTime.Minute);
        
        return (dateTime, randomInterval.Fee);
    }
}