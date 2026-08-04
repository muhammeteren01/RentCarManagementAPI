using Core.Enums;

namespace Core.Entities;

public class DamageReport
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Description { get; set; } = null!;
    public decimal DamageCost { get; set; }
    public DamagePaymentStatus PaymentStatus { get; set; }
    public DateTime ReportedDate { get; set; }
    public DateTime? PaidDate { get; set; }

    public Rental Rental { get; set; } = null!;
}
