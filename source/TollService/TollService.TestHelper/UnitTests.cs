using AutoFixture;

namespace TollService.TestHelper;

public abstract class UnitTests
{
    protected Fixture Fixture = new();

    protected UnitTests()
    {
        Fixture.Register(() => TimeOnly.FromDateTime(Fixture.Create<DateTime>()));
        Fixture.Register(() => DateOnly.FromDateTime(Fixture.Create<DateTime>()));
    }
}