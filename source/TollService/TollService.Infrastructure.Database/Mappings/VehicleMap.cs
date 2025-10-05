using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollService.Domain.Models;

namespace TollService.Infrastructure.Database.Mappings;


public class VehiclePassMap : IEntityTypeConfiguration<VehiclePass>
{
    public void Configure(EntityTypeBuilder<VehiclePass> builder)
    {
        builder.HasKey(p => p.PassId);
        builder.Property(p => p.PassId).ValueGeneratedNever();
        builder.Property(p => p.VehicleId);
        builder.Property(p => p.Timestamp);
        builder.Property(p => p.CreatedAt);
    }
}