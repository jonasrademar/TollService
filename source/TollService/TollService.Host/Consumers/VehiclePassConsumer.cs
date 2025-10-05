using MassTransit;
using TollService.Domain;
using TollService.Messages;

namespace TollService.Host.Consumers;

public class VehiclePassConsumer(IVehiclePassRepository vehiclePassRepository) : IConsumer<VehiclePassRegistrationMessage>
{
    public async Task Consume(ConsumeContext<VehiclePassRegistrationMessage> context)
    {
        await vehiclePassRepository.AddVehiclePass(
            context.Message.PassId, 
            context.Message.VehicleId, 
            context.Message.Timestamp);
    }
}