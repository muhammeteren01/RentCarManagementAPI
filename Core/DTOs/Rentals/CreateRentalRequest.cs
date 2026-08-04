namespace Core.DTOs.Rentals;

public class CreateRentalRequest
{
    public Guid CarId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
}
