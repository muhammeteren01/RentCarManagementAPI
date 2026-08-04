namespace Core.DTOs.Rentals;

public class ReturnRentalRequest
{
    public DateTime? ActualReturnDate { get; set; }
    public int EndMileage { get; set; }
}
