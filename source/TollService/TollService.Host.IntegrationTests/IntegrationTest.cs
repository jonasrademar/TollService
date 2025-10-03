using AutoFixture;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace TollService.Host.IntegrationTests;

public class IntegrationTest : WebApplicationFactory<Program>
{
    public IFixture Fixture { get; set; } = new Fixture();
    protected ITestHarness BusTestHarness => Services.GetRequiredService<ITestHarness>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(svc =>
        {
            svc.AddMassTransitTestHarness();
        });
    }
}