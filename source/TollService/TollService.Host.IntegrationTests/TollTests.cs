using AutoFixture;
using RichardSzalay.MockHttp;
using Shouldly;
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

        Http.When(HttpMethod.Get, $"http://vehicleservice/vehicle/{vehiclePassRequest.VehicleId}")
            .RespondWithJson(TollableVehicle());

        var response = await BusTestHarness.GetRequestClient<GetTollRequest>()
            .GetResponse<GetTollResponse>(request, CancellationToken);

        response.Message.Toll.ShouldBe(18);
    }


    [Fact]
    public async Task GetTollRequest_PassesOnDifferentDays_CorrectTollPerDay()
    {
        var vehicleId = Guid.NewGuid();
        var vehiclePassRequest1 = Fixture.Build<VehiclePassRegistrationMessage>()
            .With(m => m.VehicleId, vehicleId)
            .With(m => m.Timestamp, new DateTime(2013, 01, 02, 07, 30, 0))
            .Create();
        await Publish(vehiclePassRequest1);

        var vehiclePassRequest2 = Fixture.Build<VehiclePassRegistrationMessage>()
            .With(m => m.VehicleId, vehicleId)
            .With(m => m.Timestamp, new DateTime(2013, 01, 02, 08, 30, 0))
            .Create();
        await Publish(vehiclePassRequest2);

        var request1 = Fixture.Build<GetTollRequest>()
            .With(t => t.VehicleId, vehicleId)
            .With(t => t.Date, DateOnly.FromDateTime(vehiclePassRequest1.Timestamp))
            .Create();
        var request2 = Fixture.Build<GetTollRequest>()
            .With(t => t.VehicleId, vehicleId)
            .With(t => t.Date, DateOnly.FromDateTime(vehiclePassRequest2.Timestamp))
            .Create();

        Http.When(HttpMethod.Get, $"http://vehicleservice/vehicle/{vehicleId}")
            .RespondWithJson(TollableVehicle());

        var response1 = await BusTestHarness.GetRequestClient<GetTollRequest>().GetResponse<GetTollResponse>(request1, CancellationToken);
        response1.Message.Toll.ShouldBe(18);

        var response2 = await BusTestHarness.GetRequestClient<GetTollRequest>().GetResponse<GetTollResponse>(request2, CancellationToken);
        response2.Message.Toll.ShouldBe(8);
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