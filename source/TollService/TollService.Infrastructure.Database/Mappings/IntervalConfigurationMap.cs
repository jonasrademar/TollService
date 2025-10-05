using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollService.Domain.Models;

namespace TollService.Infrastructure.Database.Mappings;

public class IntervalConfigurationMap : IEntityTypeConfiguration<IntervalConfiguration>
{
    public void Configure(EntityTypeBuilder<IntervalConfiguration> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CreatedAt);
        builder.OwnsMany(p => p.Intervals, ib =>
        {
            ib.Property(b => b.Fee);
            ib.Property(b => b.StartTime);
        });
    }
}