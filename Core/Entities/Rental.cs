using Core.Enums;

namespace Core.Entities;

public class Rental
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public int StartMileage { get; set; }
    public int? EndMileage { get; set; }
    public decimal BasePrice { get; set; }
    public decimal LateFee { get; set; }
    public decimal ExtraKmCharge { get; set; }
    public decimal DamageFee { get; set; }
    public RentalStatus Status { get; set; }

    public User User { get; set; } = null!;
    public Car Car { get; set; } = null!;
    public ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();
}
