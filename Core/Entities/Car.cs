using Core.Enums;

namespace Core.Entities;

public class Car
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public int CurrentMileage { get; set; }
    public int MaintenanceThresholdKm { get; set; }
    public decimal DailyPrice { get; set; }
    public decimal ExtraKmFee { get; set; }
    public CarStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
