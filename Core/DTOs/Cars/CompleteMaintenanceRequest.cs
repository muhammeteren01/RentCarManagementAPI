namespace Core.DTOs.Cars;

public class CompleteMaintenanceRequest
{
    /// <summary>
    /// Next km milestone for maintenance. If omitted, defaults to current mileage + 10000.
    /// </summary>
    public int? NextMaintenanceThresholdKm { get; set; }
}
