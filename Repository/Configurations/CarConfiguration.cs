using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.ToTable("cars");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Year).HasColumnName("year").IsRequired();
        builder.Property(x => x.CurrentMileage).HasColumnName("current_mileage").IsRequired();
        builder.Property(x => x.MaintenanceThresholdKm).HasColumnName("maintenance_threshold_km").IsRequired();
        builder.Property(x => x.DailyPrice).HasColumnName("daily_price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ExtraKmFee).HasColumnName("extra_km_fee").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasMany(x => x.Rentals)
            .WithOne(x => x.Car)
            .HasForeignKey(x => x.CarId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
