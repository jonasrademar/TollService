using AutoFixture;
using System.Threading;
using Shouldly;
using TollService.Messages;

namespace TollService.Host.IntegrationTests;

public class TollTests : IntegrationTest
{
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