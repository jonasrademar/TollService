using AutoFixture;
using MassTransit;
using Shouldly;
using System.Threading;
using TollService.Messages;

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
        var request = Fixture.Create<GetTollRequest>();

        var client = BusTestHarness.GetRequestClient<GetTollRequest>();
        var response = await client.GetResponse<GetTollResponse>(request, TestContext.Current.CancellationToken);

        // Todo Prepare consistently tollable dates
        response.Message.Toll.ShouldBeGreaterThan(0);
    }
}