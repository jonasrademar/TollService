using AutoFixture;
using Shouldly;
using System.Globalization;
using System.Linq;

namespace TollService.Domain.UnitTests;

public class TollCalculatorTests : TestHelper.UnitTests
{
    private readonly TollCalculator subject = new();

    [Theory, 
     InlineData(8, "2013-01-02 06:00"), 
     InlineData(13, "2013-01-02 06:45"), 
     InlineData(18, "2013-01-02 07:30")]
    public void GetTollFee_TollableVehicle_SinglePass_ReturnsExpectedToll(int expectedToll, DateTime dateTime)
    {
        var result = subject.GetTollFee(dateTime, TollableVehicle());
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
    public void GetTollFee_TollableVehicle_MultiplePasses_ReturnsExpectedToll(int expectedToll, string[] dateTimes)
    {
        var parsedDates = dateTimes
            .Select(s => DateTime.ParseExact(s, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture))
            .ToArray();

        var result = subject.GetTollFee(TollableVehicle(), parsedDates);
        result.ShouldBe(expectedToll);
    }

    [Fact]
    public void GetTollFee_TollFreeVehicle_ReturnsZero()
    {
        var tollableDate = TollableDateTime();

        var result = subject.GetTollFee(TollFreeVehicle(), [tollableDate]);
        result.ShouldBe(0);
    }

    [Fact]
    public void GetTollFee_NullVehicle_Tollable()
    {
        var tollableDate = TollableDateTime();

        var result = subject.GetTollFee(null!, [tollableDate]);
        result.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void GetTollFee_TollFreeDateTime_ReturnsZero()
    {
        var tollableDate = TollFreeDateTime();

        var result = subject.GetTollFee(TollableVehicle(), [tollableDate]);
        result.ShouldBe(0);
    }

    // Osv. Vanligtvis mer komplett testtäckning men lite för hårdkodat för att orka i kodlabben 😵
    // Föreställ er att det finns massa fler goa tester här.

    private Vehicle TollableVehicle()
    {
        return Fixture.Build<Vehicle>()
            .With(v => v.VehicleType, Fixture.Build<VehicleType>().With(c => c.Tollable, true).Create())
            .With(v => v.VehicleClassification,
                Fixture.Build<VehicleClassification>().With(c => c.Tollable, true).Create())
            .Create();
    }
    private Vehicle TollFreeVehicle()
    {
        var tollableSwitch = Fixture.Create<int>() % 3;
        return Fixture.Build<Vehicle>()
            .With(v => v.VehicleType, Fixture.Build<VehicleType>()
                .With(c => c.Tollable, tollableSwitch is not (0 or 2)).Create())
            .With(v => v.VehicleClassification, Fixture.Build<VehicleClassification>()
                .With(c => c.Tollable, tollableSwitch is not (0 or 1)).Create())
            .Create();
    }

    private DateTime TollableDateTime() => new DateTime(2013, 01, 02, 06, 30, 00);
    private DateTime TollFreeDateTime() => new DateTime(2013, 01, 01, 12, 00, 00);
}