using System.Collections.Immutable;
using MassTransit;
using TollService.Domain;
using TollService.Infrastructure.Vehicle;
using TollService.Messages;

namespace TollService.Host.Consumers;

public class GetTollRequestConsumer(
    ITollCalculator tollCalculator, 
    IVehicleServiceProxy vehicleService,
    IVehiclePassRepository vehiclePassRepository) : IConsumer<GetTollRequest>
{
    public async Task Consume(ConsumeContext<GetTollRequest> context)
    {
        var vehicle = await vehicleService.GetVehicle(context.Message.VehicleId);
        var vehiclePasses = await vehiclePassRepository.GetPasses(context.Message.VehicleId, context.Message.Date);
        var dates = vehiclePasses.Select(p => p.Timestamp);

        var toll = tollCalculator.GetTollFee(vehicle, dates.ToImmutableList());

        await context.RespondAsync(new GetTollResponse(toll));
    }
}