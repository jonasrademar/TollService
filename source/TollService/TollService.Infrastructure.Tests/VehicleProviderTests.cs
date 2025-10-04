using Moq;
using Shouldly;
using TollService.Infrastructure.Vehicle;
using TollService.TestHelper;

namespace TollService.Infrastructure.Tests;

public class VehicleProviderTests : UnitTests
{
    private static readonly Mock<IVehicleServiceProxy> Proxy = new();
    private readonly VehicleProvider subject = new(Proxy.Object);
    
    [Fact]
    public async Task GetVehicle_NullVehicle_ReturnsTollable()
    {
        var vehicleId = Guid.NewGuid();
        Proxy.Setup(m => m.GetVehicle(vehicleId)).ReturnsAsync((Vehicle.Contracts.Vehicle?)null);

        var result = await subject.GetVehicle(vehicleId);
        result.Tollable.ShouldBeTrue();
    }
}