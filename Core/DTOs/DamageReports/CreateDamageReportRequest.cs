namespace Core.DTOs.DamageReports;

public class CreateDamageReportRequest
{
    public Guid RentalId { get; set; }
    public string Description { get; set; } = null!;
    public decimal DamageCost { get; set; }
}
