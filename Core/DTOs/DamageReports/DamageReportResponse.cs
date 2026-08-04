using Core.Enums;

namespace Core.DTOs.DamageReports;

public class DamageReportResponse
{
    public Guid Id { get; set; }
    public Guid RentalId { get; set; }
    public string Description { get; set; } = null!;
    public decimal DamageCost { get; set; }
    public DamagePaymentStatus PaymentStatus { get; set; }
    public DateTime ReportedDate { get; set; }
    public DateTime? PaidDate { get; set; }
}
