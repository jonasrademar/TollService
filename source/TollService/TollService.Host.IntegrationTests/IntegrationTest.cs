using AutoFixture;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using RichardSzalay.MockHttp;

namespace TollService.Host.IntegrationTests;

public class IntegrationTest :  WebApplicationFactory<Program>
{
    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;
    protected MockHttpMessageHandler Http { get; } = new(BackendDefinitionBehavior.Always);
    public IFixture Fixture { get; set; } = new Fixture();
    protected ITestHarness BusTestHarness => Services.GetRequiredService<ITestHarness>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(svc =>
        {
            svc.AddMassTransitTestHarness();
            svc.ConfigureAll<HttpClientFactoryOptions>(
                opt => opt.HttpMessageHandlerBuilderActions.Add(ba => ba.PrimaryHandler = Http));
        });
    }

    protected async Task Publish<T>(T message) where T : class
    {
        // Publish och sen invänta att meddelandet hanterats färdigt för att undvika timing problematik i testen.
        await BusTestHarness.Bus.Publish(message, CancellationToken);
        await BusTestHarness.Consumed.Any<T>(x => x.Context.Message == message, CancellationToken);
    }
}