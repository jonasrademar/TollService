using Microsoft.EntityFrameworkCore;
using TollService.Domain;
using TollService.Domain.Exceptions;
using TollService.Domain.Models;

namespace TollService.Infrastructure.Database;

public class IntervalConfigurationRepository(TollServiceDbContext dbContext) : IIntervalConfigurationRepository
{
    public async Task<IntervalConfiguration> GetLatestConfiguration() =>
        await dbContext.IntervalConfigurations.OrderBy(c => c.Id).LastOrDefaultAsync() 
        ?? throw new NotFoundException("Missing IntervalConfigurations");
}