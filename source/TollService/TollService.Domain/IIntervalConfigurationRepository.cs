using TollService.Domain.Models;

namespace TollService.Domain;

public interface IIntervalConfigurationRepository
{
    public Task<IntervalConfiguration> GetLatestConfiguration();
}