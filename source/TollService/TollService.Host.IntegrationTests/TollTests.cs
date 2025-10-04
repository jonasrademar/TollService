using AutoFixture;
using MassTransit;
using Shouldly;
using System.Threading;
using RichardSzalay.MockHttp;
using TollService.Infrastructure.Vehicle.Contracts;
using TollService.Messages;
using TollService.TestHelper;

namespace TollService.Host.IntegrationTests;

public class TollTests : IntegrationTest
{
    public TollTests()
    {
        Fixture.Register(() => DateOnly.FromDateTime(Fixture.Create<DateTime>()));
    }

    [Fact]
    public async Task GetTollRequest_PublishesTollResponse()
    {
        var vehiclePassRequest = Fixture.Build<VehiclePassRegistrationMessage>()
            .With(m => m.Timestamp, new DateTime(2013, 01, 02, 07, 30, 0))
            .Create();

        await Publish(vehiclePassRequest);

        var request = Fixture.Build<GetTollRequest>()
            .With(t => t.VehicleId, vehiclePassRequest.VehicleId)
            .With(t => t.Date, DateOnly.FromDateTime(vehiclePassRequest.Timestamp))
            .Create();

        Http.When(HttpMethod.Get, "http://vehicleservice/vehicle")
            .RespondWithJson(TollableVehicle());

        var response = await BusTestHarness.GetRequestClient<GetTollRequest>()
            .GetResponse<GetTollResponse>(request, CancellationToken);

        response.Message.Toll.ShouldBe(18);
    }

    [Fact]
    public async Task GetTollRequest_NoVehiclePasses_PublishesTollResponse()
    {
        var request = Fixture.Create<GetTollRequest>();

        Http.When(HttpMethod.Get, "http://vehicleservice/vehicle")
            .RespondWithJson(TollableVehicle());

        var response = await BusTestHarness.GetRequestClient<GetTollRequest>()
            .GetResponse<GetTollResponse>(request, CancellationToken);

        response.Message.Toll.ShouldBe(0);
    }

    private Vehicle TollableVehicle()
    {
        return Fixture.Build<Vehicle>()
            .With(v => v.VehicleType, Fixture.Build<VehicleType>().With(c => c.Tollable, true).Create())
            .With(v => v.VehicleClassifications,
                [Fixture.Build<VehicleClassification>().With(c => c.Tollable, true).Create()])
            .Create();
    }
}