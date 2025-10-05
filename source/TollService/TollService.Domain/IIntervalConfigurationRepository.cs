using TollService.Domain.Models;

namespace TollService.Domain;

// Skall finnas möjlighet att konfigurera dessa mot något interface mot TollService.
public interface IIntervalConfigurationRepository
{
    public Task<IntervalConfiguration> GetLatestConfiguration();
}