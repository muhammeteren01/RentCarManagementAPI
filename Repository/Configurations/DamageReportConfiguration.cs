using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Repository.Configurations;

public class DamageReportConfiguration : IEntityTypeConfiguration<DamageReport>
{
    public void Configure(EntityTypeBuilder<DamageReport> builder)
    {
        builder.ToTable("damage_reports");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.RentalId).HasColumnName("rental_id").IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.DamageCost).HasColumnName("damage_cost").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(x => x.IsPaid).HasColumnName("is_paid").IsRequired();
        builder.Property(x => x.ReportedDate).HasColumnName("reported_date").IsRequired();
        builder.Property(x => x.PaidDate).HasColumnName("paid_date");
    }
}
