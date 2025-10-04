using MassTransit;
using TollService.Domain;
using TollService.Messages;

namespace TollService.Host.Consumers;

public class GetTollRequestConsumer(
    ITollCalculator tollCalculator, 
    IIVehicleProvider vehicleProvider) : IConsumer<GetTollRequest>
{
    public async Task Consume(ConsumeContext<GetTollRequest> context)
    {
        var vehicle = await vehicleProvider.GetVehicle(context.Message.VehicleId);
        var toll = await tollCalculator.GetTollFeeAsync(vehicle, context.Message.Date);

        await context.RespondAsync(new GetTollResponse(toll));
    }
}