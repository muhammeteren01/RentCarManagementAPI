using Core.Enums;

namespace Core.DTOs.Rentals;

public class RentalResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CarId { get; set; }
    public string? CarBrand { get; set; }
    public string? CarModel { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public int StartMileage { get; set; }
    public int? EndMileage { get; set; }
    public int RentalDays { get; set; }
    public int IncludedKm { get; set; }
    public decimal BasePrice { get; set; }
    public decimal LateFee { get; set; }
    public decimal ExtraKmCharge { get; set; }
    public decimal DamageFee { get; set; }
    public decimal TotalPrice { get; set; }
    public RentalStatus Status { get; set; }
}
