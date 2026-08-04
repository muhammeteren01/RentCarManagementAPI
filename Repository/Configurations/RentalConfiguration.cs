using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class RentalConfiguration : IEntityTypeConfiguration<Rental>
{
    public void Configure(EntityTypeBuilder<Rental> builder)
    {
        builder.ToTable("rentals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.CarId).HasColumnName("car_id").IsRequired();
        builder.Property(x => x.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(x => x.PlannedEndDate).HasColumnName("planned_end_date").IsRequired();
        builder.Property(x => x.ActualReturnDate).HasColumnName("actual_return_date");
        builder.Property(x => x.StartMileage).HasColumnName("start_mileage").IsRequired();
        builder.Property(x => x.EndMileage).HasColumnName("end_mileage");
        builder.Property(x => x.BasePrice).HasColumnName("base_price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.LateFee).HasColumnName("late_fee").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.ExtraKmCharge).HasColumnName("extra_km_charge").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.HasMany(x => x.DamageReports)
            .WithOne(x => x.Rental)
            .HasForeignKey(x => x.RentalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
