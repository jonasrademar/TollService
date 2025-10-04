using MassTransit;
using TollService.Domain;
using TollService.Messages;

namespace TollService.Host.Consumers;

public class VehiclePassConsumer(IVehiclePassRepository vehiclePassRepository) : IConsumer<VehiclePassRegistrationMessage>
{
    public Task Consume(ConsumeContext<VehiclePassRegistrationMessage> context)
    {
        vehiclePassRepository.AddVehiclePass(
            context.Message.PassId, 
            context.Message.VehicleId, 
            context.Message.Timestamp);
        
        return Task.CompletedTask;
    }
}