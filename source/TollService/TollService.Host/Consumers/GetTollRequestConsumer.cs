using MassTransit;
using TollService.Domain;
using TollService.Messages;

namespace TollService.Host.Consumers;

public class GetTollRequestConsumer(ITollCalculator tollCalculator, IVehicleRepository vehicleRepository) : IConsumer<GetTollRequest>
{
    public async Task Consume(ConsumeContext<GetTollRequest> context)
    {
        var vehicle = await vehicleRepository.GetVehicle(context.Message.VehicleId);

        var toll = tollCalculator.GetTollFee(vehicle, context.Message.Dates);

        await context.RespondAsync(new GetTollResponse(toll));
    }
}