using MassTransit;
using Microsoft.EntityFrameworkCore;
using TollService.Domain;
using TollService.Domain.Settings;
using TollService.Host.Consumers;
using TollService.Infrastructure.Database;
using TollService.Infrastructure.Holiday;
using TollService.Infrastructure.Vehicle;

/*  F�rsta fr�gan var vilken dom�n vi �r i. Fokus i applikationen �r "TollCalculator", vilken
 * k�nns som den h�r hemma i en TollService. Sen f�rs�kt arbeta med separation of concerns f�r
 * att h�lla TollService s� relevant till vad man skulle kunna f�rv�nta sig i en TollService,
 * relaterat till sagd TollCalculator.
 */

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TollServiceDbContext>(ctx => 
    ctx.UseInMemoryDatabase($"tollservice_{Guid.NewGuid().ToByteArray()}"));

builder.Services.AddMemoryCache();

builder.Services.Configure<TollSettings>(builder.Configuration.GetSection("TollSettings"));

builder.Services.AddTransient<ITollCalculator, TollCalculator>();
builder.Services.AddTransient<IVehiclePassRepository, VehiclePassRepository>();
builder.Services.AddTransient<IIntervalConfigurationRepository, IntervalConfigurationRepository>();

builder.Services.AddTransient<IIVehicleProvider, VehicleProvider>();
builder.Services.AddSingleton<IHolidayProvider, HolidayProvider>();

builder.Services.AddHttpClient<IVehicleServiceProxy, VehicleServiceProxy>(client =>
    client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("VehicleService")!));
builder.Services.AddHttpClient<IHolidayServiceProxy, HolidayServiceProxy>(client =>
    client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("HolidayService")!));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<GetTollRequestConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
