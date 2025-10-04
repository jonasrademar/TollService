using MassTransit;
using Microsoft.EntityFrameworkCore;
using TollService.Domain;
using TollService.Host.Consumers;
using TollService.Infrastructure.Database;
using TollService.Infrastructure.Vehicle;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TollServiceDbContext>(ctx => 
    ctx.UseInMemoryDatabase($"tollservice_{Guid.NewGuid().ToByteArray()}"));

builder.Services.AddTransient<ITollCalculator, TollCalculator>();
builder.Services.AddTransient<IVehiclePassRepository, VehiclePassRepository>();

builder.Services.AddHttpClient<IVehicleServiceProxy, VehicleServiceProxy>(client =>
    client.BaseAddress = new Uri(builder.Configuration.GetConnectionString("VehicleProxy")!));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
