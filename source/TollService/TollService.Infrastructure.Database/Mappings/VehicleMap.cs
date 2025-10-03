using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TollService.Domain;

namespace TollService.Infrastructure.Database.Mappings;

public class VehicleMap : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasOne(p => p.VehicleClassification).WithMany();
        builder.HasOne(p => p.VehicleType).WithMany();
    }
}

public class VehicleTypeMap : IEntityTypeConfiguration<VehicleType>
{
    public void Configure(EntityTypeBuilder<VehicleType> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Description).HasMaxLength(32);
        builder.Property(p => p.Tollable).IsRequired();
    }
}

public class VehicleClassificationMap : IEntityTypeConfiguration<VehicleClassification>
{
    public void Configure(EntityTypeBuilder<VehicleClassification> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.Property(p => p.Description).HasMaxLength(32);
        builder.Property(p => p.Tollable).IsRequired();
    }
}