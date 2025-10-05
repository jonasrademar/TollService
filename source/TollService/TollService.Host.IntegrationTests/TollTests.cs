using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using Shouldly;
using TollService.Domain.Models;
using TollService.Infrastructure.Database;
using TollService.Infrastructure.Holiday.Contracts;
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

    [Theory, InlineData(true), InlineData(false)]
    public async Task GetTollRequest_PublishesTollResponse(bool vehicleFound)
    {
        await PrepareIntervals();

        var vehiclePassRequest = Fixture.Build<VehiclePassRegistrationMessage>()
            .With(m => m.Timestamp, new DateTime(2013, 01, 02, 07, 30, 0))
            .Create();
        Http.When(HttpMethod.Get, $"http://holidayservice/holidays/{vehiclePassRequest.Timestamp.Year}").RespondWithJson(Array.Empty<Holiday>());

        await Publish(vehiclePassRequest);

        var request = Fixture.Build<GetTollRequest>()
            .With(t => t.VehicleId, vehiclePassRequest.VehicleId)
            .With(t => t.Date, DateOnly.FromDateTime(vehiclePassRequest.Timestamp))
            .Create();

        Http.When(HttpMethod.Get, $"http://vehicleservice/vehicle/{vehiclePassRequest.VehicleId}")
            .RespondWithJson(vehicleFound ? TollableVehicle() : null);

        var response = await BusTestHarness.GetRequestClient<GetTollRequest>()
            .GetResponse<GetTollResponse>(request, CancellationToken);

        response.Message.Toll.ShouldBe(18);
    }

    [Fact]
    public async Task GetTollRequest_PassesExceedsDailyCap_PublishesTollResponseWithDailyCap()
    {
        await PrepareIntervals();
        var vehicleId = Guid.NewGuid();
        var date = new DateOnly(2025, 10, 06);

        Http.When(HttpMethod.Get, $"http://holidayservice/holidays/{date.Year}").RespondWithJson(Array.Empty<Holiday>());

        await PublishPassagesWithTime(new(06, 00), new(07, 15), new(08, 20), new(10, 00), new(15, 00), new(16, 15));

        async Task PublishPassagesWithTime(params TimeOnly[] times)
        {
            foreach (var timeOnly in times)
            {
                await Publish(Fixture.Build<VehiclePassRegistrationMessage>()
                    .With(m => m.VehicleId, vehicleId)
                    .With(m => m.Timestamp, new DateTime(date, timeOnly))
                    .Create());
            }
        }

        var request = Fixture.Build<GetTollRequest>()
            .With(t => t.VehicleId, vehicleId)
            .With(t => t.Date, date)
            .Create();

        Http.When(HttpMethod.Get, $"http://vehicleservice/vehicle/{vehicleId}")
            .RespondWithJson(TollableVehicle());

        var response = await BusTestHarness.GetRequestClient<GetTollRequest>()
            .GetResponse<GetTollResponse>(request, CancellationToken);

        response.Message.Toll.ShouldBe(60);
    }


    [Fact]
    public async Task GetTollRequest_PassesOnDifferentDays_CorrectTollPerDay()
    {
        await PrepareIntervals();

        Http.When(HttpMethod.Get, $"http://holidayservice/holidays/2013").RespondWithJson(Array.Empty<Holiday>());
        var vehicleId = Guid.NewGuid();
        var vehiclePassRequest1 = Fixture.Build<VehiclePassRegistrationMessage>()
            .With(m => m.VehicleId, vehicleId)
            .With(m => m.Timestamp, new DateTime(2013, 01, 02, 07, 30, 0))
            .Create();
        await Publish(vehiclePassRequest1);

        var vehiclePassRequest2 = Fixture.Build<VehiclePassRegistrationMessage>()
            .With(m => m.VehicleId, vehicleId)
            .With(m => m.Timestamp, new DateTime(2013, 01, 03, 08, 30, 0))
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
        await PrepareIntervals();
        var request = Fixture.Create<GetTollRequest>();

        Http.When(HttpMethod.Get, $"http://vehicleservice/vehicle/{request.VehicleId}")
            .RespondWithJson(TollableVehicle());
        Http.When(HttpMethod.Get, $"http://holidayservice/holidays/{request.Date.Year}")
            .RespondWithJson(Array.Empty<Holiday>());

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

    private async Task PrepareIntervals()
    {
        using var scope = Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TollServiceDbContext>();
        dbContext.IntervalConfigurations.Add(new IntervalConfiguration
        {
            Intervals = [
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
            ]
        });
        await dbContext.SaveChangesAsync();
    }
}